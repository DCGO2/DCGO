using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX7
{
    public class EX7_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your Turn - ESS

            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain 1 Memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Gain1Memory_EX7_005");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Your Turn] [Once Per Turn] When an effect places an Option card with the [Three Musketeers] trait in this Digimon's digivolution cards, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                               hashtable: hashtable,
                               permanentCondition: permanent => permanent == card.PermanentOfThisCard(),
                               cardEffectCondition: cardEffect => cardEffect.EffectSourceCard != null,
                               cardCondition: cardSource => 
                                   cardSource.IsOption &&
                                   cardSource.ContainsTraits("Three Musketeers"));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}