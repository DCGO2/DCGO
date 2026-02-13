using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT13
{
    public class BT13_029 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon's attack target can't be switched", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] If your opponent has 8 or more cards in their hand, for the turn, this Digimon's attack target can't be switched.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.HandCards.Count >= 8)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent != null)
                    {
                        CanNotSwitchAttackTargetClass canNotSwitchAttackTargetClass = new CanNotSwitchAttackTargetClass();
                        canNotSwitchAttackTargetClass.SetUpICardEffect("This Digimon's attack target can't be switched", CanUseCondition1, selectedPermanent.TopCard);
                        canNotSwitchAttackTargetClass.SetUpCanNotSwitchAttackTargetClass(PermanentCondition: PermanentCondition);
                        selectedPermanent.UntilEachTurnEndEffects.Add((_timing) => canNotSwitchAttackTargetClass);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            if (selectedPermanent.TopCard != null)
                            {
                                return true;
                            }

                            return false;
                        }

                        bool PermanentCondition(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent == selectedPermanent)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAddHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_BT13_029");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When an effect adds cards to your opponent's hand, unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenAddHand(hashtable, player => player == card.Owner.Enemy, cardEffect => cardEffect != null))
                        {
                            return true;
                        }
                    }

                    return false;
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
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                }
            }

            return cardEffects;
        }
    }
}