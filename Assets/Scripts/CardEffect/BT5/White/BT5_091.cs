using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT5_091 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn] When one of your Digimon digivolves, you may suspend this Tamer to trigger <Draw 1>. (Draw 1 card from your deck.)";
            }

            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition))
                        {
                            return true;

                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanActivateSuspendCostEffect(card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
            }
        }

        if (timing == EffectTiming.None)
        {
            AddSkillClass addSkillClass = new AddSkillClass();
            addSkillClass.SetUpICardEffect("Lv3 Digimons gain \"[When Attacking] Lose 1 memory.\"", CanUseCondition, card);
            addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
            cardEffects.Add(addSkillClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                {
                    if (permanent.Level == 3)
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            if (permanent.IsDigimon)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(addSkillClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                if (PermanentCondition(cardSource.PermanentOfThisCard()))
                {
                    if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                    {
                        return true;
                    }
                }

                return false;
            }

            List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
            {
                if (_timing == EffectTiming.OnAllyAttack)
                {
                    ActivateClass activateClass1 = new ActivateClass();
                    activateClass1.SetUpICardEffect("Memory -1", CanUseCondition1, cardSource);
                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription());
                    activateClass1.SetRootCardEffect(addSkillClass);
                    activateClass1.SetHashString("Memory-1_BT5_091");
                    cardEffects.Add(activateClass1);

                    string EffectDiscription()
                    {
                        return "[When Attacking] Lose 1 memory.";
                    }

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (CardEffectCommons.CanTriggerOnAttack(hashtable, cardSource))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool CanActivateCondition1(Hashtable hashtable)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(cardSource))
                        {
                            return true;
                        }

                        return false;
                    }

                    IEnumerator ActivateCoroutine1(Hashtable _hashtable)
                    {
                        yield return ContinuousController.instance.StartCoroutine(cardSource.Owner.AddMemory(-1, activateClass1));
                    }
                }

                return cardEffects;
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        return cardEffects;
    }
}
