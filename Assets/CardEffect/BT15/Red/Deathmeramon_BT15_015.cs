using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public class Deathmeramon_BT15_015 : CEntity_Effect
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
                return "[Main] If you have a level 5 or higher green Digimon in playCyou can suspend this Tamer to reveal the top card of your deck. If that card is a Digimon cardCadd it to your hand. Otherwise place it at the bottom of your deck.";
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
