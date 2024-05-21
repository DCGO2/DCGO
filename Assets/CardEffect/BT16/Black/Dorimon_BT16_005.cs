using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace DCGO.CardEffects.BT16
{
    public class Dorimon_BT16_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT16-005-Memory+1");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When another Digimon with <Blocker> is deleted, gain 1 memory.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                    {
                        if (permanent.IsDigimon)
                        {
                            if (permanent.HasBlocker) 
                            {
                                if (permanent != card.PermanentOfThisCard())
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    Debug.Log($"CanUseCondition: {CardEffectCommons.IsExistOnBattleArea(card)}");
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Debug.Log($"CanUseCondition: {CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition)}");
                        if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    Debug.Log($"CanActivateCondition: {CardEffectCommons.IsExistOnBattleArea(card)}");
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Debug.Log($"CanActivateCondition: {card.Owner.CanAddMemory(activateClass)}");
                        if (card.Owner.CanAddMemory(activateClass))
                        {
                            Debug.Log($"CanActivateCondition");
                            List<Hashtable> hashtables = CardEffectCommons.GetHashtablesFromHashtable(hashtable);

                            if (hashtables != null)
                            {
                                Debug.Log($"CanActivateCondition: {hashtables.Count}");
                                if (hashtables.Count >= 1)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Debug.Log($"ActivateCoroutine");
                    List<Hashtable> hashtables = CardEffectCommons.GetHashtablesFromHashtable(hashtable);

                    if (hashtables != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(hashtables.Count, activateClass));
                    }
                }
            }


            return cardEffects;
        }
    }
}