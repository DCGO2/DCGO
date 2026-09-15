using System.Collections;
using System.Collections.Generic;

// Veemon
namespace DCGO.CardEffects.EX13
{
    public class EX13_017 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.IsLevel2 && targetPermanent.TopCard.HasCSTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared When Moving / On Play

            string SharedEffectName = "Reveal the top 3 cards of the deck and add 1 [Veedramon] text or [Royal Knight] trait card to the hand";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Reveal the top 3 cards of your deck. Add 1 card with [Veedramon] in its text or the [Royal Knight] trait among them to the hand. Return the rest to the bottom of the deck.";

            bool CanSelectCardCondition(CardSource cardSource)
                => cardSource.HasText("Veedramon") || cardSource.CardTraits.Contains("Royal Knight");

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: CanSelectCardCondition,
                            message: "Select 1 card with [Veedramon] in its text or the [Royal Knight] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass));
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                whenMoving: true,
                onPlay: true);

            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Digimon with [Veedramon] in its name, it doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX13_017_ESS_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When this Digimon with [Veedramon] in its name would leave the battle area by your opponent's effects, by suspending it, it doesn't leave.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card)
                        && CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOpponentEffect(cardEffect, card));

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().TopCard.ContainsCardName("Veedramon")
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        new List<Permanent>() { thisPermanent },
                        activateClass,
                        SuccessProcess,
                        null));

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermanents)
                    {
                        thisPermanent.willBeRemoveField = false;

                        thisPermanent.HideDeleteEffect();
                        thisPermanent.HideHandBounceEffect();
                        thisPermanent.HideDeckBounceEffect();
                        thisPermanent.HideWillRemoveFieldEffect();

                        yield return null;
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
