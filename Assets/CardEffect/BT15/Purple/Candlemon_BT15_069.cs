using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class Candlemon_BT15_069 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Draw 1 and/or gain Memory +1", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Deletion] If your opponent has 1 or less memory, <Draw 1>. If your opponent has 1 or more memory, gain 1 memory.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.Enemy.MemoryForPlayer <= 1)
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    if (card.Owner.Enemy.MemoryForPlayer >= 1)
                    {
                        if (card.Owner.CanAddMemory(activateClass))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.Enemy.MemoryForPlayer <= 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }

                if (card.Owner.Enemy.MemoryForPlayer >= 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
        }

        return cardEffects;
    }
}
