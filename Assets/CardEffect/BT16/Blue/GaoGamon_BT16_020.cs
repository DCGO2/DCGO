using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT16
{
    public class GaoGamon_BT16_020 : CEntity_Effect
    {
        
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            var cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement, Inherited Effect
            if(timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLightFangNightClawTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
                
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1 card, then Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("GaoGamon_BT16_020_When_Digivolving");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Both players draw 1 card from their decks. Then, if your opponent has 8 " +
                           "or more cards in their hand or this Digimon has 3 or more digivolution cards, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer.Count(player => player.LibraryCards.Count >= 1) >= 1)
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                    {
                        if (player.LibraryCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(player, 1, activateClass).Draw());
                        }
                    }
                    
                    if (card.Owner.Enemy.HandCards.Count >= 8 || card.PermanentOfThisCard().DigivolutionCards.Count >= 3)
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