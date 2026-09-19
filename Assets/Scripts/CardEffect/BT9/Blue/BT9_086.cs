using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT9_086 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnStartTurn)
        {
            cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
        }

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn] When you attack with a Digimon that has [Jellymon] in its name or is level 5 or higher, if you have 7 or fewer cards in hand, you may suspend this Tamer to <Draw 1>. (Draw 1 card from your deck.)";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (isExistOnField(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (GManager.instance.attackProcess.AttackingPermanent != null)
                        {
                            if (GManager.instance.attackProcess.AttackingPermanent.TopCard != null)
                            {
                                if (GManager.instance.attackProcess.AttackingPermanent.TopCard.Owner == card.Owner)
                                {
                                    if (GManager.instance.attackProcess.AttackingPermanent.IsDigimon)
                                    {
                                        if (GManager.instance.attackProcess.AttackingPermanent.TopCard.ContainsCardName("Jellymon"))
                                        {
                                            return true;
                                        }

                                        if (GManager.instance.attackProcess.AttackingPermanent.Level >= 5)
                                        {
                                            if (GManager.instance.attackProcess.AttackingPermanent.TopCard.HasLevel)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (isExistOnField(card))
                {
                    if (!card.PermanentOfThisCard().IsSuspended && card.PermanentOfThisCard().CanSuspend)
                    {
                        if (card.Owner.HandCards.Count <= 7)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (isExistOnField(card))
                {
                    if (!card.PermanentOfThisCard().IsSuspended && card.PermanentOfThisCard().CanSuspend)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        return cardEffects;
    }
}
