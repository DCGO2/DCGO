using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX1
{
    public class EX1_068 : CEntity_Effect
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
                    return "[Main] All of your opponent's Digimon gain \"[When Attacking] Lose 2 memory\" until the end of their next turn.";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool PermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaPermanents())
                    {
                        if (PermanentCondition(permanent))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
                        }
                    }

                    AddSkillClass addSkillClass = new AddSkillClass();
                    addSkillClass.SetUpICardEffect("Memory -2", CanUseCondition1, card);
                    addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                    card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => addSkillClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
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
                            activateClass1.SetUpICardEffect("Memory -2", CanUseCondition2, cardSource);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                            cardEffects.Add(activateClass1);

                            if (cardSource.PermanentOfThisCard() != null)
                            {
                                activateClass1.SetEffectSourcePermanent(cardSource.PermanentOfThisCard());
                            }

                            string EffectDiscription1()
                            {
                                return "[When Attacking] Lose 2 memory.";
                            }

                            bool CanUseCondition2(Hashtable hashtable)
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
                                yield return ContinuousController.instance.StartCoroutine(cardSource.Owner.AddMemory(-2, activateClass1));
                            }
                        }

                        return cardEffects;
                    }
                }
            }


            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Memory +2", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Gain 2 memory.";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
                }
            }

            return cardEffects;
        }
    }
}