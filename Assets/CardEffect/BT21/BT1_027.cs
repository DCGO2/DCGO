using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT1
{
    public class BT1_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card, 1, 2000));
            }

            if(timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Testing When Linked", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When linked] Testing Log";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenLinked(hashtable, null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    UnityEngine.Debug.Log("CARD SUCCESSFULLY LINKED");
                    yield return null;
                }
            }

            if(timing == EffectTiming.None)
            {
                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(permanentCondition: null, isInheritedEffect: true, card: card, condition: CanUseCondition));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(
                    changeValue: 1,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }

            return cardEffects;
        }
    }
}