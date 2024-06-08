using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX6
{
    public class Xiquemon_EX6_013 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Rule Text, Inherited Effect
            
            if (timing == EffectTiming.None)
            {
                // Trait Rule Aquatic Type
                ChangeTraitsClass changeTraitsClass = new ChangeTraitsClass();
                changeTraitsClass.SetUpICardEffect("Trait: Has [Aquatic] Type", CanUseCondition, card);
                changeTraitsClass.SetUpChangeTraitsClass(changeeTraits: ChangeTraits);
                cardEffects.Add(changeTraitsClass);
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }
                
                List<string> ChangeTraits(CardSource cardSource, List<string> cardTraits)
                {
                    if (cardSource == card)
                    {
                        cardTraits.Add("Aquatic");
                    }
                    
                    return cardTraits;
                }
                
                // Jamming Inherited Effect
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }
            
            #endregion
            
            #region On Play
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1 and gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[On Play] <Draw 1>. If played from digivolution cards, gain 1 memory.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                        
                        if (CardEffectCommons.IsFromDigimonDigivolutionCards(hashtable))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    // Draw 1
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    
                    // Gain 1 memory if played from digivolution cards
                    if (CardEffectCommons.IsFromDigimonDigivolutionCards(hashtable))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}