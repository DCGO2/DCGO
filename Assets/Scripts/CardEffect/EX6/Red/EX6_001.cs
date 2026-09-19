using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX6
{
    public class EX6_001 : CEntity_Effect
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
                activateClass.SetHashString("Gain1Memory_EX6_001");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[Your Turn] [Once Per Turn] When an effect places a card with the [Legend-Arms] trait in this Digimon's digivolution cards, gain 1 memory.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                                    hashtable: hashtable,
                                    permanentCondition: permanent => permanent == card.PermanentOfThisCard(),
                                    cardEffectCondition: cardEffect => cardEffect.EffectSourceCard != null,
                                    cardCondition: cardSource => cardSource.ContainsTraits("Legend-Arms")))
                            {
                                return true;
                            }
                        }
                    }
                    
                    return false;
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
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