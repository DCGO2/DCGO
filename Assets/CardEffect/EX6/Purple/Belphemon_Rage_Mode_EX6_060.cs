using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects
{
    public class Belphemon_Rage_Mode_EX6_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Belphemon: Sleep Mode");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, 
                    digivolutionCost: 1, 
                    ignoreDigivolutionRequirement: false,
                    card: card, 
                    condition: null));
            }
            #endregion

            #region On Play
            #endregion

            #region When Digivolving
            #endregion

            #region All Turns
            #endregion

            if (timing == EffectTiming.None)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "";
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