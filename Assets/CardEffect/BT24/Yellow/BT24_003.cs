using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tsunomon 
namespace DCGO.CardEffects.BT24
{
    public class BT24_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into Shaman trait", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT24_003_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When your security stack is removed from, this Digimon may digivolve into a [Shaman] trait Digimon card in the hand with the digivolution cost reduced by 1.";
                }

                bool SelectSourceCard(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("Shaman")
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, SelectSourceCard);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                       targetPermanent: card.PermanentOfThisCard(),
                       cardCondition: SelectSourceCard,
                       payCost: true,
                       reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                       fixedCostTuple: null,
                       ignoreDigivolutionRequirementFixedCost: -1,
                       isHand: true,
                       activateClass: activateClass,
                       successProcess: null));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
