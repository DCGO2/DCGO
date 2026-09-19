using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT15
{
    public class BT15_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon gains Security Attack +1 and can attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("SAttack_BT15_015");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main][Once per turn] By paying 2 memory, this Digimon gains [Security A+1] for the turn. Then, this Digimon may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.MaxMemoryCost >= 2)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.MaxMemoryCost >= 2)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-2, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                        targetPermanent: card.PermanentOfThisCard(),
                        changeValue: 1,
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));

                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            if (card.PermanentOfThisCard().CanAttack(activateClass))
                            {
                                SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                selectAttackEffect.SetUp(
                                    attacker: card.PermanentOfThisCard(),
                                    canAttackPlayerCondition: () => true,
                                    defenderCondition: (permanent) => true,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}