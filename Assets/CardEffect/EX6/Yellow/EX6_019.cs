using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX6
{
    public class EX6_019 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Barrier
            
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(
                    CardEffectFactory.BarrierSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            
            #endregion
            
            #region When Attacking - ESS
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Draw1_EX6_019");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[When Attacking] [Once Per Turn] If this Digimon has the [Angel]/[Archangel]/[Three Great Angels] trait, [Draw 1].";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            if (card.PermanentOfThisCard().TopCard.ContainsTraits("Angel") ||
                                card.PermanentOfThisCard().TopCard.ContainsTraits("Archangel") ||
                                card.PermanentOfThisCard().TopCard.ContainsTraits("Three Great Angels") ||
                                card.PermanentOfThisCard().TopCard.ContainsTraits("ThreeGreatAngels"))
                            {
                                return true;
                            }
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}