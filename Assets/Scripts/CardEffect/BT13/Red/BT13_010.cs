using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_010 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 [Kristy Damon] to hand and this Digimon digivolves to [Garudamon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] If played by an effect, by returning 1 of your [Kristy Damon]s to the hand, this Digimon may digivolve into [Garudamon] in the hand, ignoring its digivolution requirements and without paying the cost.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Kristy Damon"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardNames.Contains("KristyDamon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardNames.Contains("Garudamon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card) && CardEffectCommons.IsByEffect(hashtable, null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            if (CardEffectCommons.CanTriggerOnPlay(hashtable, card))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent thisCardPermanent = card.PermanentOfThisCard();

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            Permanent bounceTargetPermanent = null;

                            int maxCount = Math.Min(1, card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 [Kristy Damon] to return to hand.", "The opponent is selecting 1 [Kristy Damon] to return to hand.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                bounceTargetPermanent = permanent;

                                yield return null;
                            }

                            if (bounceTargetPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BouncePeremanentAndProcessAccordingToResult(
                                    targetPermanents: new List<Permanent>() { bounceTargetPermanent },
                                    activateClass: activateClass,
                                    successProcess: SuccessProcess(),
                                    failureProcess: null));

                                IEnumerator SuccessProcess()
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                            targetPermanent: thisCardPermanent,
                                            cardCondition: CanSelectCardCondition,
                                            payCost: false,
                                            reduceCostTuple: null,
                                            fixedCostTuple: null,
                                            ignoreDigivolutionRequirementFixedCost: 0,
                                            isHand: true,
                                            activateClass: activateClass,
                                            successProcess: null));
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] <Draw 1> (Draw 1 card from your deck.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletionInherited(hashtable, card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            return cardEffects;
        }
    }
}