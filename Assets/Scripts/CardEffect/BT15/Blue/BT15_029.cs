using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT15
{
    public class BT15_029 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play/When Digivolving Shared

            bool CanSelectSharedOwnPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.TopCard.CardColors.Contains(CardColor.Blue))
                    {
                        if (!permanent.TopCard.Equals(card))
                            return true;
                    }
                }

                return false;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.Level <= card.PermanentOfThisCard().cardSources.Last().Level)
                        return true;
                }

                return false;
            }

            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Count > 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 Digimon card to digivolution cards to return 1 of your opponent's Digimon whose level is less than or equal to the placed card's level to the bottom of the deck.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, true, EffectSharedDiscription());
                cardEffects.Add(activateClass);

                string EffectSharedDiscription()
                {
                    return "[On Play] By placing 1 of your other blue Digimon as this Digimon's bottom digivolution card, return 1 of your opponent's Digimon whose level is less than or equal to the placed card's level to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CanActivateSharedCondition(_hashtable))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        SelectPermanentEffect selectPermanentSourcecEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentSourcecEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectSharedOwnPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentSourceCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentSourcecEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                        yield return StartCoroutine(selectPermanentSourcecEffect.Activate());

                        IEnumerator SelectPermanentSourceCoroutine(Permanent permanent)
                        {
                            selectedCards.Add(permanent.TopCard);

                            yield return null;
                        }

                        if (selectedCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                selectedCards,
                                activateClass));

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage(
                                    "Select 1 Digimon to return to bottom of deck.",
                                    "The opponent is selecting 1 Digimon to return to bottom of deck.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(new List<Permanent> { permanent }, _hashtable).DeckBounce());
                                }
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
                activateClass.SetUpICardEffect("Place 1 Digimon card to digivolution cards to return 1 of your opponent's Digimon whose level is less than or equal to the placed card's level to the bottom of the deck.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, true, EffectSharedDiscription());
                cardEffects.Add(activateClass);

                string EffectSharedDiscription()
                {
                    return "[When Digivolving] By placing 1 of your other blue Digimon as this Digimon's bottom digivolution card, return 1 of your opponent's Digimon whose level is less than or equal to the placed card's level to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CanActivateSharedCondition(_hashtable))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        SelectPermanentEffect selectPermanentSourcecEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentSourcecEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectSharedOwnPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentSourceCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentSourcecEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                        yield return StartCoroutine(selectPermanentSourcecEffect.Activate());

                        IEnumerator SelectPermanentSourceCoroutine(Permanent permanent)
                        {
                            selectedCards.Add(permanent.TopCard);

                            yield return null;
                        }

                        if (selectedCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                selectedCards,
                                activateClass));

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage(
                                    "Select 1 Digimon to return to bottom of deck.",
                                    "The opponent is selecting 1 Digimon to return to bottom of deck.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(new List<Permanent> { permanent }, _hashtable).DeckBounce());
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Inerited Effect

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon, by placing 1 blue Digimon as bottom source", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_BT15_020");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] By placing 1 of your other blue Digimon as this Digimon's bottom digivolution card, unsuspend this Digimon.";
                }

                bool CanSelectOwnPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            if (permanent.TopCard.CardColors.Contains(CardColor.Blue))
                            {
                                if (!permanent.TopCard.Equals(card))
                                    return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CanActivateCondition(_hashtable))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        SelectPermanentEffect selectPermanentSourcecEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentSourcecEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOwnPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentSourceCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentSourcecEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                        yield return StartCoroutine(selectPermanentSourcecEffect.Activate());

                        IEnumerator SelectPermanentSourceCoroutine(Permanent permanent)
                        {
                            selectedCards.Add(permanent.TopCard);

                            yield return null;
                        }

                        if (selectedCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                selectedCards,
                                activateClass));

                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}