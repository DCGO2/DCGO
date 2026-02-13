using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_002 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Attacking - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Draw1_BT20_002");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] (Once Per Turn) If this Digimon has [Dracomon]/[Examon] in its text, <Draw 1>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           (card.PermanentOfThisCard().TopCard.HasText("Dracomon") ||
                            card.PermanentOfThisCard().TopCard.HasText("Examon"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}