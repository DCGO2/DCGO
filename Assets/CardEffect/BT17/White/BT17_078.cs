using System.Collections;
using System.Collections.Generic;
using System;

namespace DCGO.CardEffects.BT17
{
    public class BT17_078 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Condition
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                            {
                                                if (permanent.TopCard.ContainsCardName("Greymon"))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(6))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                            {
                                                if (permanent.TopCard.ContainsCardName("Garurumon"))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(6))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "Lv.6 w/[Greymon] in name"),

                        new JogressConditionElement(PermanentCondition2, "Lv.6 w/[Garurumon] in name"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Blast DNA
            if (timing == EffectTiming.OnCounterTiming)
            {
                List<BlastDNACondition> blastDNAConditions = new List<BlastDNACondition>
                {
                    new("WarGreymon"),
                    new("MetalGarurumon")
                };
                cardEffects.Add(CardEffectFactory.BlastDNADigivolveEffect(card: card,
                    blastDNAConditions: blastDNAConditions, condition: null));
            }
            #endregion

            #region Raid/Blocker
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(
                    isInheritedEffect: false, 
                    card: card, 
                    condition: null));
            }

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region On Play Shared
            bool IsOpponentsDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return all digimon of a selected level, Then delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] If DNA Digivolving, choose 1 opponent's Digimon and return that Digimon and all of their Digimon with the same level to the bottom of the deck. Then, delete 1 of their Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsJogress(hashtable))
                    {
                        Permanent selectedPermanent = null;

                        if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentsDigimon));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsOpponentsDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentLevelCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to bottom deck all of same level.", "The opponent is selecting 1 Digimon to bottom deck all of same level.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        IEnumerator SelectPermanentLevelCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                                selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent.TopCard.HasLevel)
                        {
                            List<Permanent> returnedPermanents = card.Owner.Enemy.GetBattleAreaDigimons()
                                .Filter(permanent =>
                                permanent.Level == selectedPermanent.Level &&
                                !permanent.TopCard.CanNotBeAffected(activateClass));

                            Hashtable _hashtable = new Hashtable();
                            _hashtable.Add("CardEffect", activateClass);

                            if (returnedPermanents.Count == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(returnedPermanents, _hashtable).DeckBounce());
                            }
                            else
                            {
                                List<CardSource> cardSources = returnedPermanents.Map(permanent => permanent.TopCard);

                                List<SkillInfo> skillInfos = cardSources.Map(cardSource =>
                                {
                                    ChangeBaseDPClass cardEffect = new ChangeBaseDPClass();
                                    cardEffect.SetUpICardEffect(" ", null, cardSource);

                                    return new SkillInfo(cardEffect, null, EffectTiming.None);
                                });

                                List<CardSource> selectedCards = new List<CardSource>();

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: (cardSource) => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                                    message: "Specify the order to place the card at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                                    maxCount: cardSources.Count,
                                    canEndNotMax: false,
                                    isShowOpponent: false,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: cardSources,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetNotShowCard();
                                selectCardEffect.SetNotAddLog();
                                selectCardEffect.SetUpSkillInfos(skillInfos);

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                {
                                    if (cardSources.Count >= 1)
                                    {
                                        selectedCards = cardSources.Clone();

                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Cards", true, true));
                                    }
                                }

                                if (selectedCards.Count >= 1)
                                {
                                    List<Permanent> libraryPermanets = selectedCards.Map(cardSource => cardSource.PermanentOfThisCard());

                                    if (libraryPermanets.Count >= 1)
                                    {
                                        DeckBottomBounceClass putLibraryBottomPermanent = new DeckBottomBounceClass(libraryPermanets, _hashtable);

                                        putLibraryBottomPermanent.SetNotShowCards();

                                        yield return ContinuousController.instance.StartCoroutine(putLibraryBottomPermanent.DeckBounce());
                                    }
                                }
                            }
                        }
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentsDigimon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return all digimon of a selected level, Then delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If DNA Digivolving, choose 1 opponent's Digimon and return that Digimon and all of their Digimon with the same level to the bottom of the deck. Then, delete 1 of their Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsJogress(hashtable))
                    {
                        Permanent selectedPermanent = null;

                        if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentsDigimon));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsOpponentsDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentLevelCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to bottom deck all of same level.", "The opponent is selecting 1 Digimon to bottom deck all of same level.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        IEnumerator SelectPermanentLevelCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                                selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent.TopCard.HasLevel)
                        {
                            List<Permanent> returnedPermanents = card.Owner.Enemy.GetBattleAreaDigimons()
                                .Filter(permanent =>
                                permanent.Level == selectedPermanent.Level &&
                                !permanent.TopCard.CanNotBeAffected(activateClass));

                            Hashtable _hashtable = new Hashtable();
                            _hashtable.Add("CardEffect", activateClass);

                            if (returnedPermanents.Count == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(returnedPermanents, _hashtable).DeckBounce());
                            }
                            else
                            {
                                List<CardSource> cardSources = returnedPermanents.Map(permanent => permanent.TopCard);

                                List<SkillInfo> skillInfos = cardSources.Map(cardSource =>
                                {
                                    ChangeBaseDPClass cardEffect = new ChangeBaseDPClass();
                                    cardEffect.SetUpICardEffect(" ", null, cardSource);

                                    return new SkillInfo(cardEffect, null, EffectTiming.None);
                                });

                                List<CardSource> selectedCards = new List<CardSource>();

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: (cardSource) => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                                    message: "Specify the order to place the card at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                                    maxCount: cardSources.Count,
                                    canEndNotMax: false,
                                    isShowOpponent: false,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: cardSources,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetNotShowCard();
                                selectCardEffect.SetNotAddLog();
                                selectCardEffect.SetUpSkillInfos(skillInfos);

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                {
                                    if (cardSources.Count >= 1)
                                    {
                                        selectedCards = cardSources.Clone();

                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Cards", true, true));
                                    }
                                }

                                if (selectedCards.Count >= 1)
                                {
                                    List<Permanent> libraryPermanets = selectedCards.Map(cardSource => cardSource.PermanentOfThisCard());

                                    if (libraryPermanets.Count >= 1)
                                    {
                                        DeckBottomBounceClass putLibraryBottomPermanent = new DeckBottomBounceClass(libraryPermanets, _hashtable);

                                        putLibraryBottomPermanent.SetNotShowCards();

                                        yield return ContinuousController.instance.StartCoroutine(putLibraryBottomPermanent.DeckBounce());
                                    }
                                }
                            }
                        }
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentsDigimon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}