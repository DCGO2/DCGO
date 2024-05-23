using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT16
{
    public class PhoenixmonXAntibody_BT16_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Phoenixmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's 1 Digimon gains effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable,card);
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
                    Debug.Log("Running Coroutine");

                    if(CardEffectCommons.IsOwnerTurn(card))
                    {
                        Debug.Log("IsOwnerTurn");

                        foreach (CardSource cardSource1 in card.PermanentOfThisCard().DigivolutionCards)
                        {
                            foreach (ICardEffect cardEffect in cardSource1.PermanentOfThisCard().EffectList(EffectTiming.OnDestroyedAnyone))
                            {
                                if (!cardEffect.IsSecurityEffect && cardEffect.IsOnDeletion && cardEffect.IsInheritedEffect)
                                {
                                    Debug.Log("Looping through effects");
                                    ActivateClass activateClass1 = new ActivateClass();
                                    activateClass1.SetUpICardEffect(cardEffect.EffectName, CanUseCondition2, card);
                                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                    activateClass1.SetIsInheritedEffect(true);
                                    activateClass1.SetEffectSourcePermanent(cardSource1.PermanentOfThisCard());

                                    string EffectDiscription1()
                                    {
                                        return cardEffect.EffectDiscription;
                                    }

                                    bool CanUseCondition2(Hashtable hashtable1)
                                    {
                                        return CardEffectCommons.CanTriggerOnEndAttack(hashtable1, card);
                                    }

                                    bool CanActivateCondition1(Hashtable hashtable1)
                                    {
                                        Debug.Log("Test");
                                        return CardEffectCommons.IsExistOnBattleArea(card);
                                    }

                                    IEnumerator ActivateCoroutine1(Hashtable hashtable)
                                    {
                                        Debug.Log("Activating Added effect");
                                        yield return ContinuousController.instance.StartCoroutine(((ActivateICardEffect)cardEffect).Activate(hashtable));
                                    }


                                    CardEffectCommons.AddEffectToPermanent(
                                        targetPermanent: cardSource1.PermanentOfThisCard(),
                                        effectDuration: EffectDuration.UntilOwnerTurnEnd,
                                        card: card,
                                        cardEffect: activateClass1,
                                        timing: EffectTiming.OnEndAttack);

                                }
                            }
                        }
                        
                    }
                    yield return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}