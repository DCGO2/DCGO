using System.Collections;
using System.Collections.Generic;

// Akihiro Kurata — BT27-098
namespace DCGO.CardEffects.BT27
{
    public class BT27_098 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            // [All Turns] When any Digimon are deleted, by suspending this Tamer,
            // you may place the top 2 cards of your deck face down under it.
            // Trigger based on BT26_095; deck placement based on BT25_088.
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend: place top 2 deck cards face down under this Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    "[All Turns] When any Digimon are deleted, by suspending this Tamer, you may place the top 2 cards of your deck face down under this Tamer.");
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, p => p.IsDigimon, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent> { card.PermanentOfThisCard() },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));
                }

                IEnumerator SuccessProcess(List<Permanent> suspendedPermanents)
                {
                    // Guard the deck size; BT25_088 indexes both cards without a guard.
                    int count = card.Owner.LibraryCards.Count;
                    if (count == 0) yield break;
                    List<CardSource> topCards = new List<CardSource> { card.Owner.LibraryCards[0] };
                    if (count > 1) topCards.Add(card.Owner.LibraryCards[1]);
                    yield return ContinuousController.instance.StartCoroutine(
                        card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                            topCards, activateClass, isFacedown: true));
                }
            }

            // [End of All Turns] — NOT REGISTERED UNTIL THE ENTIRE ATOMIC COST +
            // hand/trash play + post-play Tamer attachment sequence is verified.
            // Requirements:
            // 1. At either player's end-of-turn timing, select/trash exactly 5 bottom
            //    face-down sources distributed among any of your Tamers.
            // 2. Select one Digimon card named Gizmon or Belphemon from either hand
            //    or trash; play without paying its cost.
            // 3. ONLY after successful play, place THIS Tamer under the newly played
            //    Digimon as its bottom digivolution card.
            // Do not implement as a free play without step 3, or charge sources
            // before confirming a legal play target. See README for references.
            // Closest verified references: BT23_099 (hand/trash chooser),
            // BT20_099 (placing this card beneath another permanent),
            // ST23_12 (trash bottom face-down Tamer sources),
            // BT18_061 (OnEndTurn + free play).
            // NOTE: EffectTiming.OnEndTurn needs both-player trigger validation.

            // BT27-098 has no Security effect in the supplied specification.

            return cardEffects;
        }
    }
}
