using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class ST12_15 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] Reveal the top 3 cards of your deck. Add 1 card with [Huckmon] or [Sistermon] in its name or [Royal Knight] in its traits among them to your hand. Trash the rest. Then, place this card in your Battle Area.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.ContainsCardName("Huckmon"))
                {
                    return true;
                }

                if (cardSource.ContainsCardName("Sistermon"))
                {
                    return true;
                }

                if (cardSource.HasRoyalKnightTraits)
                {
                    return true;
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Huckmon] or [Sistermon] in its name or [Royal Knight] in its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.Trash,
                    activateClass: activateClass
                ));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
            }
        }

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] <Delay> (Trash this card in your battle area to activate the effect below. You can't activate this effect the turn this card enters play.) - The next time one of your Digimon would digivolve this turn, reduce the digivolution cost by 1.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanDeclareOptionDelayEffect(card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                IEnumerator SuccessProcess()
                {
                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    yield return new WaitForSeconds(0.2f);

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                    changeCostClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: null, timing: EffectTiming.None, getCardEffect: getCardEffect);

                    ActivateClass activateClass1 = new ActivateClass();
                    Func<EffectTiming, ICardEffect> getCardEffect1 = GetCardEffect1;
                    activateClass1.SetUpICardEffect("Remove Effect", CanUseCondition1, card);
                    activateClass1.SetUpActivateClass(null, ActivateCoroutine1, -1, false, "");
                    activateClass1.SetIsBackgroundProcess(true);
                    CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: null, timing: EffectTiming.None, getCardEffect: getCardEffect1);

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
                                    Cost -= 1;
                                }
                            }
                        }

                        return Cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        if (targetPermanents != null)
                        {
                            if (targetPermanents.Count(PermanentCondition) >= 1)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card);
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return cardSource.Owner == card.Owner;
                    }

                    bool RootCondition(SelectCardEffect.Root root)
                    {
                        return true;
                    }

                    bool isUpDown()
                    {
                        return true;
                    }

                    ICardEffect GetCardEffect(EffectTiming _timing)
                    {
                        return changeCostClass;
                    }

                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(
                            hashtable: _hashtable1,
                            permanentCondition: PermanentCondition,
                            cardCondition: CardSourceCondition))
                        {
                            card.Owner.UntilEachTurnEndEffects.Remove(getCardEffect);
                            card.Owner.UntilEachTurnEndEffects.Remove(getCardEffect1);
                            yield return null;
                        }
                    }


                    ICardEffect GetCardEffect1(EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.AfterPayCost)
                        {
                            return activateClass1;
                        }

                        return null;
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top 3 cards of deck and place this card in battle area", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Reveal 3 cards from the top of your deck. Add 1 card with [Huckmon] or [Sistermon] in its name or [Royal Knight] in its traits among them to your hand. Trash the rest. Then, place this card in your Battle Area.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.ContainsCardName("Huckmon"))
                {
                    return true;
                }

                if (cardSource.ContainsCardName("Sistermon"))
                {
                    return true;
                }

                if (cardSource.HasRoyalKnightTraits)
                {
                    return true;
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Huckmon] or [Sistermon] in its name or [Royal Knight] in its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.Trash,
                    activateClass: activateClass
                ));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
            }
        }

        return cardEffects;
    }
}
