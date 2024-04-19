using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class Dorimon_BT16_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When another Digimon with <Blocker> is deleted, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return null;
                }
            }


            return cardEffects;
        }
    }
}