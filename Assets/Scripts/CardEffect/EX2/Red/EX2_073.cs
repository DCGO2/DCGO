using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX2
{
    public class EX2_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete oppponent's all Digimons with the highest DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete all of your opponent's Digimon with the highest DP.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        if (permanent.TopCard != null)
                        {
                            if (permanent.IsDigimon)
                            {
                                if (permanent.TopCard.Owner == card.Owner.Enemy)
                                {
                                    if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                    {
                                        int maxDP = 0;

                                        foreach (Permanent permanent1 in permanent.TopCard.Owner.GetBattleAreaDigimons())
                                        {
                                            if (permanent1.DP > maxDP)
                                            {
                                                maxDP = permanent1.DP;
                                            }
                                        }

                                        if (permanent.DP == maxDP)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                List<Permanent> destroyedPermanetns = new List<Permanent>();

                                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                                {
                                    if (CanSelectPermanentCondition(permanent))
                                    {
                                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            destroyedPermanetns.Add(permanent);
                                        }
                                    }
                                }

                                if (destroyedPermanetns.Count >= 1)
                                {
                                    Hashtable hashtable = new Hashtable();
                                    hashtable.Add("CardEffect", activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyedPermanetns, hashtable).Destroy());
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                int count()
                {
                    return 1 + card.Owner.Enemy.TrashCards.Count / 10;
                }

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Trash {count()} cards of opponent's security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] Trash the top card of your opponent's security stack. Add 1 to the number of cards trashed by this effect for every 10 cards in your opponent's trash.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.SecurityCards.Count >= 1)
                        {
                            if (count() >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: count(),
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }

            return cardEffects;
        }
    }
}