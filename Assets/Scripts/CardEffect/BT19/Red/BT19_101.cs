using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_101 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("MoonMillenniummon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Overclock

            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.OverclockSelfEffect(trait: "Composite", isInheritedEffect: false, card: card,
                    condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 card from opponent's trash to deck top return 1 digimon to bottom of the deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By returning 1 Digimon card from your opponent's trash to the top of the deck, return 1 of their Digimon to the bottom of the deck.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.TrashCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.Enemy.TrashCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int maxCount = 1;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 1 card to place at the top of the deck.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.Owner.Enemy.TrashCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        selectCardEffect.SetNotAddLog();

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(cardSources));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Top Cards", true, true));

                                bool CanSelectPermanentToBounce(Permanent permanent)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                    {
                                        return true;
                                    }

                                    return false;
                                }

                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentToBounce));

                                SelectPermanentEffect selectPermanentEffectBounce = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffectBounce.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentToBounce,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                    cardEffect: activateClass);

                                selectPermanentEffectBounce.SetUpCustomMessage("Select 1 Digimon to return to the bottom of the deck.", "The opponent is selecting 1 Digimon to return to the bottom of the deck.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffectBounce.Activate());
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
                activateClass.SetUpICardEffect("Return 1 card from opponent's trash to deck top return 1 digimon to bottom of the deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By returning 1 Digimon card from your opponent's trash to the top of the deck, return 1 of their Digimon to the bottom of the deck.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.TrashCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.Enemy.TrashCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int maxCount = 1;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 1 card to place at the top of the deck.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.Owner.Enemy.TrashCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        selectCardEffect.SetNotAddLog();

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(cardSources));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Top Cards", true, true));

                                bool CanSelectPermanentToBounce(Permanent permanent)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                    {
                                        return true;
                                    }

                                    return false;
                                }

                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentToBounce));

                                SelectPermanentEffect selectPermanentEffectBounce = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffectBounce.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentToBounce,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                    cardEffect: activateClass);

                                selectPermanentEffectBounce.SetUpCustomMessage("Select 1 Digimon to return to the bottom of the deck.", "The opponent is selecting 1 Digimon to return to the bottom of the deck.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffectBounce.Activate());
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 card from opponent's trash to deck top return 1 digimon to bottom of the deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By returning 1 Digimon card from your opponent's trash to the top of the deck, return 1 of their Digimon to the bottom of the deck.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.TrashCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.Enemy.TrashCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int maxCount = 1;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 1 card to place at the top of the deck.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.Owner.Enemy.TrashCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        selectCardEffect.SetNotAddLog();

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(cardSources));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Top Cards", true, true));

                                bool CanSelectPermanentToBounce(Permanent permanent)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                    {
                                        return true;
                                    }

                                    return false;
                                }

                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentToBounce));

                                SelectPermanentEffect selectPermanentEffectBounce = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffectBounce.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentToBounce,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                    cardEffect: activateClass);

                                selectPermanentEffectBounce.SetUpCustomMessage("Select 1 Digimon to return to the bottom of the deck.", "The opponent is selecting 1 Digimon to return to the bottom of the deck.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffectBounce.Activate());
                            }
                        }
                    }
                }
            }

            #endregion

            #region All Turns - Shared

            bool ImmunityAndNoSuspendCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count == 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #region All Turns - Immunity

            if (timing == EffectTiming.None)
            {
                CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effects", ImmunityAndNoSuspendCondition, card);
                canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                cardEffects.Add(canNotAffectedClass);

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            if (cardSource == card.PermanentOfThisCard().TopCard)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool SkillCondition(ICardEffect cardEffect)
                {
                    if (CardEffectCommons.IsOpponentEffect(cardEffect, card))
                    {
                        return true;
                    }

                    return false;
                }
            }

            #endregion

            #region All Turns - Cannot Suspend

            if (timing == EffectTiming.None)
            {
                CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                canNotSuspendClass.SetUpICardEffect("Can't be suspended", ImmunityAndNoSuspendCondition, card);
                canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                cardEffects.Add(canNotSuspendClass);

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (permanent == card.PermanentOfThisCard())
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}