using System.Collections;
using System.Collections.Generic;

// Kyokyomon
namespace DCGO.CardEffects.BT24
{
    public class BT24_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region ESS

            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 3 then return to top or bot.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT24_005_Reveal");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When Tamer cards are placed in this Digimon's digivolution cards, reveal the top 3 cards of your deck. Return the revealed cards to the top or bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                            CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                               hashtable: hashtable,
                               permanentCondition: permanent => permanent == card.PermanentOfThisCard(),
                               cardEffectCondition: null,
                               cardCondition: cardSource =>
                                   cardSource.IsTamer);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                                        revealCount: 3,
                                        simplifiedSelectCardCondition:
                                        new SimplifiedSelectCardConditionClass(
                                                canTargetCondition: (cardSource) => false,
                                                message: "",
                                                mode: SelectCardEffect.Mode.Custom,
                                                maxCount: -1,
                                                selectCardCoroutine: null),
                                        remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                                        activateClass: activateClass
                                    ));
                }
            }
            #endregion
            return cardEffects;
        }
    }
}
