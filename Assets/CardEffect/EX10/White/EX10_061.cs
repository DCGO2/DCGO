using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;

namespace DCGO.CardEffects.EX10
{
    public class EX10_061 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Requirements

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasDarkMastersTrait &&
                           targetPermanent.TopCard.IsLevel6;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Cost Reduction

            #region Before Pay Cost - Condition Effect

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Placing 1 [Dark Masters] to get Play Cost -4", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("PlayCost-_EX10-061");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When this card would be played, by placing 1 of each face-up [Dark Masters] trait card with different names from your security stack under this card, reduce the play cost by 4 for each card placed.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           !cardSource.IsFlipped &&
                           cardSource.HasDarkMastersTrait;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool CanNoSelect(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost)
                        {
                            return false;
                        }

                        if (cardSource.PayingCost(SelectCardEffect.Root.Trash, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost)
                        {
                            return false;
                        }

                        if (cardSource.PayingCost(SelectCardEffect.Root.DigivolutionCards, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost)
                        {
                            return false;
                        }

                        if (cardSource.PayingCost(SelectCardEffect.Root.Security, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost)
                        {
                            return false;
                        }

                        if (cardSource.PayingCost(SelectCardEffect.Root.Library, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionOwnersSecurity(card, CanSelectCardCondition, false);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> digivolutionCards = new List<CardSource>();

                    bool CanSelectSecurityCardCondition(CardSource cardSource)
                    {
                        if (CanSelectCardCondition(cardSource))
                        {
                            if (digivolutionCards.Count((filteredCard) => filteredCard.CardNames.Contains(cardSource.CardNames[0])) == 0)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersSecurity(card, CanSelectSecurityCardCondition, false))
                    {
                        bool noSelect = CanNoSelect(CardEffectCommons.GetCardFromHashtable(_hashtable));
                        List<CardSource> selectedCards = new List<CardSource>();

                        List<string> cardNames = card.Owner.SecurityCards.Filter(CanSelectSecurityCardCondition)
                                            .Map(cardSource1 => cardSource1.CardNames)
                                            .Flat()
                                            .Distinct()
                                            .ToList();

                        int maxCount = Math.Min(4, cardNames.Count);

                        if (maxCount >= 1)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectSecurityCardCondition,
                                canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                canNoSelect: () => noSelect,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select cards to place in Digivolution cards.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Security,
                                customRootCardList: null,
                                canLookReverseCard: false,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                            {
                                List<string> cardNames = new List<string>();

                                foreach (CardSource cardSource1 in cardSources)
                                {
                                    if (!cardNames.Contains(cardSource1.CardNames[0]))
                                    {
                                        cardNames.Add(cardSource1.CardNames[0]);
                                    }
                                }

                                foreach (CardSource cardSource1 in digivolutionCards)
                                {
                                    if (!cardNames.Contains(cardSource1.CardNames[0]))
                                    {
                                        cardNames.Add(cardSource1.CardNames[0]);
                                    }
                                }

                                if (cardNames.Contains(cardSource.CardNames[0]))
                                {
                                    return false;
                                }

                                return true;
                            }

                            bool CanEndSelectCondition(List<CardSource> cardSources)
                            {
                                List<string> cardNames = new List<string>();

                                foreach (CardSource cardSource1 in cardSources)
                                {
                                    if (!cardNames.Contains(cardSource1.CardNames[0]))
                                    {
                                        cardNames.Add(cardSource1.CardNames[0]);
                                    }
                                }

                                foreach (CardSource cardSource1 in digivolutionCards)
                                {
                                    if (!cardNames.Contains(cardSource1.CardNames[0]))
                                    {
                                        cardNames.Add(cardSource1.CardNames[0]);
                                    }
                                }

                                if (cardNames.Count != cardSources.Count + digivolutionCards.Count)
                                {
                                    return false;
                                }

                                return true;
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                digivolutionCards.Add(cardSource);
                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                GManager.instance.GetComponent<SelectDigiXrosClass>().AddDigivolutionCardInfos(new AddDigivolutionCardsInfo(activateClass, selectedCards));
                                yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                    player: card.Owner,
                                    refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                                    cardEffect: activateClass).ReduceSecurity());
                                yield return ContinuousController.instance.StartCoroutine(AfterSelectCardCoroutine(selectedCards));
                            }
                        }
                    }

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (card.Owner.CanReduceCost(null, card))
                        {
                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                        }

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                        {
                            if (CardSourceCondition(cardSource))
                            {
                                if (RootCondition(root))
                                {
                                    if (PermanentsCondition(targetPermanents))
                                    {
                                        Cost -= cardSources.Count * 4;
                                    }
                                }
                            }

                            return Cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            if (targetPermanents == null)
                            {
                                return true;
                            }
                            else
                            {
                                if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            return cardSource == card;
                        }

                        bool RootCondition(SelectCardEffect.Root root)
                        {
                            return true;
                        }

                        bool isUpDown()
                        {
                            return true;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                        yield return null;
                    }
                }
            }

            #endregion

            #region Reduce Play Cost - Not Shown

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           !cardSource.IsFlipped &&
                           cardSource.EqualsTraits("Dark Masters");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    ICardEffect activateClass = card.EffectList(EffectTiming.BeforePayCost).Find(cardEffect => cardEffect.EffectName == "Placing 1 [Dark Masters] to get Play Cost -4");
                    
                    if (activateClass != null)
                    {
                        return CardEffectCommons.HasMatchConditionOwnersSecurity(card, CanSelectCardCondition, false);
                    }

                    return false;
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            if (PermanentsCondition(targetPermanents))
                            {
                                List<CardSource> securitySources = card.Owner.SecurityCards.Filter(CanSelectCardCondition);
                                int targetCount = (from securityCard in securitySources select securityCard.CardNames).Distinct().Count();

                                Cost -= targetCount * 4;
                            }
                        }
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    if (targetPermanents == null)
                    {
                        return true;
                    }
                    else
                    {
                        if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource == card)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool isUpDown()
                {
                    return true;
                }
            }

            #endregion

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Dark Masters] from sources, give <Rush>, Delete them at end of turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may play 1 of each [Dark Masters] trait card with different names from this Digimon's digivolution cards without paying the costs. Then, all of your [Dark Masters] trait Digimon gain <Rush> for the turn. At turn end, delete the Digimon this effect played.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.EqualsTraits("Dark Masters") &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, root:SelectCardEffect.Root.DigivolutionCards);
                }

                bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                {
                    List<string> cardNames = GetNamesList(cardSources);

                    foreach (string name in cardNames)
                    {
                        if (cardSource.CardNames.Contains(name))
                            return false;
                    }

                    return true;
                }

                List<string> GetNamesList(List<CardSource> cardSources)
                {
                    List<string> cardNames = new List<string>();

                    foreach (CardSource cardName in cardSources)
                    {
                        foreach (string name in cardName.CardNames)
                        {
                            if (!cardNames.Contains(name))
                            {
                                cardNames.Add(name);
                            }
                        }
                    }

                    return cardNames;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int maxCount = Math.Min(4, selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 digivolution card to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.", "The opponent is selecting 1 digivolution card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.DigivolutionCards,
                                    activateETB: true));

                        bool PermanentCondition(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                                   permanent.TopCard.EqualsTraits("Dark Masters");
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRushPlayerEffect(
                                    permanentCondition: PermanentCondition,
                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                    activateClass: activateClass));

                        #region Delete Digimon Played

                        foreach (CardSource source in selectedCards)
                        {
                            Permanent playedPermanent = source.PermanentOfThisCard();

                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect($"[{source.BaseENGCardNameFromEntity}] Delete the Digimon", CanUseCondition2, playedPermanent.TopCard);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                            activateClass1.SetEffectSourcePermanent(selectedPermanent);
                            playedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                            if (!playedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(playedPermanent));
                            }

                            string EffectDiscription1()
                            {
                                return "[End of Your Turn] Delete this Digimon.";
                            }

                            bool CanUseCondition2(Hashtable hashtable1)
                            {
                                if (CardEffectCommons.IsOwnerTurn(card))
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(playedPermanent, playedPermanent.TopCard))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool CanActivateCondition1(Hashtable hashtable1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(playedPermanent))
                                {
                                    if (!playedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(playedPermanent))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                    new List<Permanent>() { playedPermanent },
                                    CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                                }
                            }

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.OnEndTurn)
                                {
                                    return activateClass1;
                                }

                                return null;
                            }
                        }

                        #endregion
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Dark Masters] from sources, give <Rush>, Delete them at end of turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may play 1 of each [Dark Masters] trait card with different names from this Digimon's digivolution cards without paying the costs. Then, all of your [Dark Masters] trait Digimon gain <Rush> for the turn. (This Digimon can attack the turn it comes into play.) At turn end, delete the Digimon this effect played.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.EqualsTraits("Dark Masters") &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, root:SelectCardEffect.Root.DigivolutionCards);
                }

                bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                {
                    List<string> cardNames = GetNamesList(cardSources);

                    foreach (string name in cardNames)
                    {
                        if (cardSource.CardNames.Contains(name))
                            return false;
                    }

                    return true;
                }

                List<string> GetNamesList(List<CardSource> cardSources)
                {
                    List<string> cardNames = new List<string>();

                    foreach (CardSource cardName in cardSources)
                    {
                        foreach (string name in cardName.CardNames)
                        {
                            if (!cardNames.Contains(name))
                            {
                                cardNames.Add(name);
                            }
                        }
                    }

                    return cardNames;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int maxCount = Math.Min(4, selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 digivolution card to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.", "The opponent is selecting 1 digivolution card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.DigivolutionCards,
                                    activateETB: true));

                        bool PermanentCondition(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                                   permanent.TopCard.EqualsTraits("Dark Masters");
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRushPlayerEffect(
                                    permanentCondition: PermanentCondition,
                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                    activateClass: activateClass));

                        #region Delete Digimon Played

                        foreach (CardSource source in selectedCards)
                        {
                            Permanent playedPermanent = source.PermanentOfThisCard();

                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect("Delete the Digimon", CanUseCondition2, playedPermanent.TopCard);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                            activateClass1.SetEffectSourcePermanent(selectedPermanent);
                            playedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                            if (!playedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(playedPermanent));
                            }

                            string EffectDiscription1()
                            {
                                return "[End of Your Turn] Delete this Digimon.";
                            }

                            bool CanUseCondition2(Hashtable hashtable1)
                            {
                                if (CardEffectCommons.IsOwnerTurn(card))
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(playedPermanent, playedPermanent.TopCard))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool CanActivateCondition1(Hashtable hashtable1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(playedPermanent))
                                {
                                    if (!playedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(playedPermanent))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                    new List<Permanent>() { playedPermanent },
                                    CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                                }
                            }

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.OnEndTurn)
                                {
                                    return activateClass1;
                                }

                                return null;
                            }
                        }

                        #endregion
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}