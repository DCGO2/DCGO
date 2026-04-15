using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Pagumon 
namespace DCGO.CardEffects.BT25
{
    public class BT25_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your turn 

            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a [Three Musketeers] in text or [TS] trait in hand for 2 reduced cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("EX11_074_OnAddDigivolutionCards");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When [Three Musketeers] trait cards are placed in this Digimon's digivolution cards, it may digivolve into a Digimon card with [Three Musketeers] in its text or the [TS] trait in the hand with the cost reduced by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAddDigivolutionCard(hashtable, PermamentCondition, null, CardCondition)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                        
                }

                bool PermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && card.PermanentOfThisCard() == permanent;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.HasThreeMusketeersTraits;
                }

                bool CanDigivolveCondition(CardSource cardSource)
                {
                    return cardSource.HasText("Three Musketeers") || cardSource.HasTSTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanDigivolveCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.PermanentOfThisCard(),
                            cardCondition: CanDigivolveCondition,
                            payCost: true,
                            reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}