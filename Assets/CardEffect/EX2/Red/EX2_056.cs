using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX2
{
    public class EX2_056 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When an opponent's Digimon is deleted, you may suspend this Tamer to gain 1 memory.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your Digimon gets Blitz", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("Digisorption-1_BT2_088");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When one of your Digimon would digivolve into a Digimon with [Gallantmon] or [Growlmon] in its name, that Digimon gains \"[When Digivolving] EX8-053 (This Digimon can attack when your opponent has 1 or more memory.)\" for the turn.";
                }

                bool DigivolvePermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card);
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.ContainsCardName("Gallantmon") || cardSource.ContainsCardName("Growlmon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, DigivolvePermanentCondition, CardCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        List<Permanent> Permanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable);

                        if (Permanents != null)
                        {
                            if (Permanents.Count(CardEffectCommons.IsPermanentExistsOnBattleArea) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    List<Permanent> Permanents = CardEffectCommons.GetPermanentsFromHashtable(_hashtable);

                    bool PermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        {
                            if (Permanents.Contains(permanent))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    if (Permanents != null)
                    {
                        if (Permanents.Count(PermanentCondition) >= 1)
                        {
                            foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents().Filter(PermanentCondition))
                            {
                                AddSkillClass addSkillClass = new AddSkillClass();
                                addSkillClass.SetUpICardEffect("Your Digimons get Blitz", CanUseCondition1, permanent.TopCard);
                                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                                CardEffectCommons.AddEffectToPermanent(permanent, effectDuration: EffectDuration.UntilEachTurnEnd, card: permanent.TopCard, cardEffect: addSkillClass, timing: EffectTiming.None);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));

                                bool CardSourceCondition(CardSource source)
                                {
                                    return true;
                                }

                                bool CanUseCondition1(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(permanent.TopCard))
                                    {
                                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable1, card))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                                {
                                    if (_timing == EffectTiming.OnEnterFieldAnyone)
                                    {
                                        bool Condition()
                                        {
                                            if (CardSourceCondition(cardSource))
                                            {
                                                return true;
                                            }

                                            return false;
                                        }

                                        cardEffects.Add(CardEffectFactory.BlitzSelfEffect(isInheritedEffect: false,
                                            card: cardSource,
                                            condition: Condition,
                                            isWhenDigivolving: false));
                                    }

                                    return cardEffects;
                                }
                            }
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
}