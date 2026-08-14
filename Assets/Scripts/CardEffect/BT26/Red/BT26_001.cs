using System.Collections;
using System.Collections.Generic;

// Yokomon
namespace DCGO.CardEffects.BT26
{
    public class BT26_001 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            if (timing == EffectTiming.OnAddLibraryAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Chronomon] card in hand, cost -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT26_001_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When your effects add to decks, this Digimon may digivolve into a Digimon card with [Chronomon] in its text in the hand with the cost reduced by 1.";

                // Approximates "your effects" by checking who owns the returned card, since
                // AddLibraryTopCards/BottomCards don't carry which player's effect caused the
                // move (same limitation the existing OnReturnCardsToLibraryFromTrash timing has).
                bool CardSourceCondition(CardSource cardSource)
                    => cardSource.Owner == card.Owner;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnAddLibrary(hashtable, CardSourceCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                bool CardCondition(CardSource cardSource)
                    => cardSource.IsDigimon && cardSource.HasText("Chronomon");

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CardCondition,
                        payCost: true,
                        reduceCostTuple: (1, CardCondition),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null,
                        isOptional: true));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
