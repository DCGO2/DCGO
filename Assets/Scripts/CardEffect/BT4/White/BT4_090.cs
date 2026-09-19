using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT4_090 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDetermineDoSecurityCheck)
        {
            cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Unsuspend this Digimon and it can Attack", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Unsuspend this Digimon. Then, it can attack your opponent's Digimon. This effect allows you to attack unsuspended Digimon as well.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
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
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());

                        if (card.PermanentOfThisCard().CanAttack(activateClass))
                        {
                            //Permanent selectedPermanent = card.PermanentOfThisCard();

                            CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
                            canAttackTargetDefendingPermanentClass.SetUpICardEffect($"Can attack to unsuspended Digimon", CanUseCondition1, card);
                            canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(attackerCondition: AttackerCondition, defenderCondition: DefenderCondition, CardEffectCondition);
                            Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                            selectedPermanent.UntilEndAttackEffects.Add(GetCardEffect);

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                if (selectedPermanent.TopCard != null)
                                {
                                    return true;
                                }

                                return false;
                            }

                            bool AttackerCondition(Permanent permanent)
                            {
                                return permanent == card.PermanentOfThisCard();
                            }

                            bool DefenderCondition(Permanent permanent)
                            {
                                if (permanent != null)
                                {
                                    if (permanent.TopCard != null)
                                    {
                                        if (permanent.TopCard.Owner == card.Owner.Enemy)
                                        {
                                            if (permanent.IsDigimon)
                                            {
                                                if (!permanent.IsSuspended)
                                                {
                                                    if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
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

                            bool CardEffectCondition(ICardEffect cardEffect)
                            {
                                if (cardEffect == activateClass)
                                {
                                    return true;
                                }

                                return false;
                            }

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                return canAttackTargetDefendingPermanentClass;
                            }

                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: selectedPermanent,
                                canAttackPlayerCondition: () => false,
                                defenderCondition: (permanent) => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());

                            if (selectedPermanent.TopCard != null)
                            {
                                selectedPermanent.UntilEndAttackEffects.Remove(GetCardEffect);
                            }
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
