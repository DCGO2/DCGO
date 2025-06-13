using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Kimeramon
namespace DCGO.CardEffects.EX9
{
    public class EX9_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect($"Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition, "7 level 4 [DM] trait Digimon cards w/different names");

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.IsLevel4)
                                        {
                                            if (cardSource.HasDMTraits)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<string> cardNames = new List<string>();

                            foreach (CardSource cardSource1 in cardSources)
                            {
                                foreach (string cardName in cardSource1.CardNames)
                                {
                                    if (!cardNames.Contains(cardName))
                                    {
                                        cardNames.Add(cardName);
                                    }
                                }
                            }

                            if (cardSource.CardNames.Count((cardName) => cardNames.Contains(cardName)) >= 1)
                            {
                                return false;
                            }

                            return true;
                        }

                        List<AssemblyConditionElement> elements = new List<AssemblyConditionElement>();

                        for (int i = 0; i < 7; i++)
                        {
                            elements.Add(element);
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(elements, CanTargetCondition_ByPreSelecetedList, 7);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Sec +1

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(
                changeValue: 1,
                isInheritedEffect: false,
                card: card,
                condition: null));
            }

            #endregion

            #region Rush

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place digimon from trash as top source card, delete digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may place 1 level 4 or lower [DM] trait Digimon card from your trash as this Digimon's top digivolution card. Then, delete 1 of your opponent's Digimon with the same color as any of this Digimon's digivolution cards. If this Digimon has 6 or more colors in its digivolution cards, instead delete 1 of each of your opponent's Digimon with different colors.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= 4 && cardSource.HasDMTraits;
                }

                bool CanSelectPermanentCondition(Permanent permanent, List<CardColor> cardColors)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) && permanent.TopCard.CardColors.Exists(color => cardColors.Contains(color));
                }

                bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent, List<CardColor> cardColors)
                {
                    var currentColours = permanents
                        .SelectMany(e => e.TopCard.CardColors)
                        .Distinct()
                        .ToList();

                    if (permanent.TopCard.CardColors.Exists(x => currentColours.Contains(x))) return false;
                    return true;
                }

                bool CanEndSelectCondition(List<Permanent> permanents)
                {
                    if (permanents.Count <= 0)
                        return false;

                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        CardSource selectedCard = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition));
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [DM] digimon to add as top source",
                                    maxCount: maxCount,
                                    canEndNotMax: true,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 [DM] digimon to add as top source", "The opponent is selecting 1 digimon to add as top source");
                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        if (selectedCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(selectedCard));
                            selectedCard.SetReverse();
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsTop(new List<CardSource>() { selectedCard }, activateClass));

                            List<CardColor> colours = card.PermanentOfThisCard().DigivolutionCards
                                .SelectMany(e => e.CardColors)
                                .Distinct()
                                .ToList();

                            if (colours.Count <= 5 && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, permanent => CanSelectPermanentCondition(permanent, colours)))
                            {
                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(permanent => CanSelectPermanentCondition(permanent, colours)));
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: permanent => CanSelectPermanentCondition(permanent, colours),
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }

                            if (colours.Count >= 6 && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, permanent => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)))
                            {
                                int maxCount2 = 7;
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: null,
                                    canTargetCondition_ByPreSelecetedList: (permanents, permament) => CanTargetCondition_ByPreSelecetedList(permanents, permament, colours),
                                    canEndSelectCondition: CanEndSelectCondition,
                                    maxCount: maxCount2,
                                    canNoSelect: false,
                                    canEndNotMax: true,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place digimon from trash as top source card, delete digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may place 1 level 4 or lower [DM] trait Digimon card from your trash as this Digimon's top digivolution card. Then, delete 1 of your opponent's Digimon with the same color as any of this Digimon's digivolution cards. If this Digimon has 6 or more colors in its digivolution cards, instead delete 1 of each of your opponent's Digimon with different colors.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= 4 && cardSource.HasDMTraits;
                }

                bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent, List<CardColor> cardColors)
                {
                    var currentColours = permanents
                        .SelectMany(e => e.TopCard.CardColors)
                        .Distinct()
                        .ToList();

                    if (permanent.TopCard.CardColors.Exists(x => currentColours.Contains(x))) return false;
                    return true;
                }

                bool CanEndSelectCondition(List<Permanent> permanents)
                {
                    if (permanents.Count <= 0)
                        return false;

                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        CardSource selectedCard = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition));
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [DM] digimon to add as top source",
                                    maxCount: maxCount,
                                    canEndNotMax: true,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 [DM] digimon to add as top source", "The opponent is selecting 1 digimon to add as top source");
                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        if (selectedCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(selectedCard));
                            selectedCard.SetReverse();
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsTop(new List<CardSource>() { selectedCard }, activateClass));
                        }
                    }

                    List<CardColor> colours = card.PermanentOfThisCard().DigivolutionCards
                                .SelectMany(e => e.CardColors)
                                .Distinct()
                                .ToList();

                    bool OpponentsDigimonWithMatchingColor(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                               permanent.TopCard.CardColors.Exists(x => colours.Contains(x));
                    }

                    if (colours.Count <= 5)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonWithMatchingColor))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: OpponentsDigimonWithMatchingColor,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }                        
                    }

                    if (colours.Count >= 6)
                    {
                        if(CardEffectCommons.HasMatchConditionOpponentsPermanent(card, permanent => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)))
                        {
                            int maxCount2 = 7;
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: null,
                                canTargetCondition_ByPreSelecetedList: (permanents, permament) => CanTargetCondition_ByPreSelecetedList(permanents, permament, colours),
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: maxCount2,
                                canNoSelect: false,
                                canEndNotMax: true,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.None)
            {
                int count()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return card.PermanentOfThisCard().DigivolutionCardsColors.Count;
                    }

                    return 0;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (count() >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect<Func<int>>(changeValue: () => 1000 * count(), isInheritedEffect: false, card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}