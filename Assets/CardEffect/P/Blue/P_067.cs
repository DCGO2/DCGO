using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_067 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Draw 2 and add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] At the end of the battle, <Draw 2>. (Draw 2 cards from your deck.) Then, add this card to its ownerÅfs hand.";
            }


            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnExecutingArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return null;

                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                ActivateClass activateClass1 = new ActivateClass();
                activateClass1.SetUpICardEffect("Draw 2 and add this card to hand", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                card.Owner.UntilEndBattleEffects.Add(GetCardEffect1);

                string EffectDiscription1()
                {
                    return "<Draw 2>. (Draw 2 cards from your deck.) Then, add this card to its ownerÅfs hand.";
                }

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition1(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());

                    if (card.Owner.ExecutingCards.Contains(card))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { card }, false, activateClass));
                    }
                }

                ICardEffect GetCardEffect1(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnEndBattle)
                    {
                        return activateClass1;
                    }

                    return null;
                }
            }
        }

        return cardEffects;
    }
}
