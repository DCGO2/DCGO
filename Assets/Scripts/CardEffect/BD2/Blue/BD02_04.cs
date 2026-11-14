using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Sekkamon
namespace DCGO.CardEffects.BD02
{
    public class BD02_04 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Attacking Inherited
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_BD02_04");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[When Attacking] [Once Per Turn] If your opponent has no Digimon with digivolution cards, this Digimon unsuspends.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (!card.Owner.Enemy.GetBattleAreaDigimons().Any((permanent) => permanent.DigivolutionCards.Count > 0))
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
