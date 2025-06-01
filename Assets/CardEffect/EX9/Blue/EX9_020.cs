using System.Collections;
using System.Collections.Generic;

//CresGarurumon
namespace DCGO.CardEffects.EX9
{
    public class EX9_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            #endregion

            #region Blast Digivolve

            #endregion

            #region Alliance/Sec Atk +1/Blocker

            #endregion

            #region On Play

            #endregion

            #region When Digivolving

            #endregion

            #region All Turns

            #endregion

            #region Your Turn - ESS

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