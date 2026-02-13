using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT14
{
    public class BT14_051 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend your 1 Digimon to reveal the top 5 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Reveal_BT14_051");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Opponent's Turn][Once Per Turn] By suspending 1 of your Digimon, reveal the top 5 cards of your deck. Add 2 green Digimon cards among them to the hand. Return the rest to the bottom of deck.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(permanent.TopCard))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardColors.Contains(CardColor.Green) && cardSource.IsDigimon;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool suspended = false;

                    Permanent selectedPermanent = null;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }
                    }

                    if (selectedPermanent != null)
                    {
                        if (selectedPermanent.TopCard != null)
                        {
                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                if (!selectedPermanent.IsSuspended && selectedPermanent.CanSuspend)
                                {
                                    Permanent suspendTargetPermanent = selectedPermanent;

                                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { suspendTargetPermanent }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                                    if (suspendTargetPermanent.TopCard != null)
                                    {
                                        if (suspendTargetPermanent.IsSuspended)
                                        {
                                            suspended = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (suspended)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                                            revealCount: 5,
                                            simplifiedSelectCardConditions:
                                            new SimplifiedSelectCardConditionClass[]
                                            {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 2 green Digimon cards.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 2,
                            selectCardCoroutine: null),
                                            },
                                            remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                                            activateClass: activateClass
                                        ));
                    }
                }
            }

            return cardEffects;
        }
    }
}