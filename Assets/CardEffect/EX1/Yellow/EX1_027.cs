using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX1
{
    public class EX1_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Recovery +1 (Deck) at the end of the battle", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] At the end of the battle, if you have 3 or fewer security cards, <Recovery +1 (Deck)>. (Place the top card of your deck on top of your security stack.)";
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
                    activateClass1.SetUpICardEffect("Recovery +1 (Deck)", CanUseCondition1, card);
                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                    card.Owner.UntilEndBattleEffects.Add(GetCardEffect1);

                    string EffectDiscription1()
                    {
                        return "Trigger <Recovery +1 (Deck)>. (Place the top card of your deck on top of your security stack.)";
                    }

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool CanActivateCondition1(Hashtable hashtable)
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            if (card.Owner.SecurityCards.Count <= 3)
                            {
                                if (card.Owner.CanAddSecurity(activateClass1))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass1).Recovery());
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
}