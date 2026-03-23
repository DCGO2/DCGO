using System.Collections;
using System.Collections.Generic;

// Pulsemon
public class BT7_032 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("If you have 3 security cards, gain 2 memory", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
            activateClass.SetIsOptionalOverride(_ => card.Owner.SecurityCards.Count != 3);//Effect is optional while you do not have 3 mem to whiff
            activateClass.SetIsInheritedEffect(true);
            activateClass.SetHashString("Memory+2_BT7_032");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking][Once Per Turn] If you have 3 security cards, gain 2 memory.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.SecurityCards.Count == 3)
                { 
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
                }
            }
        }

        return cardEffects;
    }
}
