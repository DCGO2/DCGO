using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects
{
    public class Trailmon_BT17_054 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

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

            #region All Turns - ESS
            if (timing == EffectTiming.None)
            {
                bool condition()
                {
                    if (!CardEffectCommons.IsOwnerTurn(card))
                        return false;

                    if (card.PermanentOfThisCard().TopCard.ContainsTraits("Machine"))
                        return false;

                    return true;
                }

                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: condition));
            }
            #endregion

            return cardEffects;
        }
    }
}