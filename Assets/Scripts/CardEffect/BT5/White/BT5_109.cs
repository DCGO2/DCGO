using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT5_109 : CEntity_Effect
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
                return "[Main] The next time one of your Digimon digivolves from level 6 to level 7 this turn, reduce the memory cost of the digivolution by 6. At the end of the turn, return the Digimon that digivolved with this effect to the bottom of its owner's deck. Trash all of the digivolution cards of that Digimon.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                yield return new WaitForSeconds(0.2f);

                ActivateClass activateClass1 = new ActivateClass();
                Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                activateClass1.SetUpICardEffect("Digivolution Cost -6", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(CanActivateCondition, ActivateCoroutine1, -1, true, EffectDiscription1());
                CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: null, timing: EffectTiming.None, getCardEffect: getCardEffect);

                ActivateClass activateClass2 = new ActivateClass();
                Func<EffectTiming, ICardEffect> getCardEffect1 = GetCardEffect1;
                activateClass2.SetUpICardEffect("Remove Effect", CanUseCondition1, card);
                activateClass2.SetUpActivateClass(null, ActivateCoroutine2, -1, false, "");
                activateClass2.SetIsBackgroundProcess(true);
                CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: null, timing: EffectTiming.None, getCardEffect: getCardEffect1);

                Permanent playedPermanent = ;

                ActivateClass activateClass3 = new ActivateClass();
                activateClass3.SetUpICardEffect("Bottom deck the Digimon", CanUseCondition2, playedPermanent.TopCard);
                activateClass3.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine3, -1, false, EffectDiscription2());
                activateClass3.SetEffectSourcePermanent(playedPermanent);
                playedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                #region Reduce evo cost

                string EffectDiscription1()
                {
                    return "Reduce the memory cost of the digivolution by 6.";
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card)
                        && targetPermanent.TopCard.IsLevel6;
                }

                bool CanUseCondition1(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(
                        hashtable: hashtable,
                        permanentCondition: PermanentCondition,
                        cardCondition: null))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                {
                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Digivolution Cost -6", CanUseCondition3, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true); card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable1));

                    bool CanUseCondition3(Hashtable hashtable)
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
                                    Cost -= 6;
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

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return cardSource.HasLevel
                            && cardSource.Level == 7;
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

                ICardEffect GetCardEffect(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.BeforePayCost)
                    {
                        return activateClass1;
                    }

                    return null;
                }


                IEnumerator ActivateCoroutine2(Hashtable _hashtable1)
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(
                        hashtable: _hashtable1,
                        permanentCondition: PermanentCondition,
                        cardCondition: null))
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
                        return activateClass2;
                    }

                    return null;
                }

                yield return new WaitForSeconds(0.2f);

                #endregion

                #region Bot deck end of turn

                if (!playedPermanent.TopCard.CanNotBeAffected(activateClass))
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(playedPermanent));
                }

                string EffectDiscription2()
                {
                    return "[End of Your Turn] Return the Digimon that digivolved with this effect to the bottom of its owner's deck. (Trash all of the digivolution cards of that Digimon.)";
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

                IEnumerator ActivateCoroutine3(Hashtable _hashtable1)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(playedPermanent))
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(
                        new List<Permanent>() { playedPermanent },
                        CardEffectCommons.CardEffectHashtable(activateClass1)).DeckBounce());
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

                #endregion
            }
        }

        #region Security Effect

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Add this card to its owner's hand.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
            }
        }

        #endregion

        return cardEffects;
    }
}
