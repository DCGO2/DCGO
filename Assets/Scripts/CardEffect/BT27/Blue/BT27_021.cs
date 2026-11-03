using System;
using System.Collections;
using System.Collections.Generic;

// Dracomon (X Antibody)
namespace DCGO.CardEffects.BT27
{
    public class BT27_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Dracomon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "Reveal the top 3 cards of deck, add 1 [Examon] text Digimon and 1 [Examon] text Option to hand";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Reveal the top 3 cards of your deck. Add 1 Digimon card with [Examon] in its text and 1 Option card with [Examon] in its text among them to the hand. Return the rest to the bottom of the deck.";

            bool CanSelectDigimonCondition(CardSource cardSource)
                => cardSource.IsDigimon && cardSource.HasText("Examon");

            bool CanSelectOptionCondition(CardSource cardSource)
                => cardSource.IsOption && cardSource.HasText("Examon");

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: CanSelectDigimonCondition,
                            message: "Select 1 Digimon card with [Examon] in its text.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: CanSelectOptionCondition,
                            message: "Select 1 Option card with [Examon] in its text.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass
                ));
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region End of Your Turn - ESS
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon and 1 other Digimon may DNA digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Your Turn] This Digimon and any of your other Digimon may DNA digivolve into a Digimon card in the hand.";

                bool CanSelectDNACardCondition(CardSource cardSource)
                    => cardSource != null
                        && cardSource.IsDigimon
                        && cardSource.Owner == card.Owner
                        && cardSource.CanPlayJogress(true)
                        && cardSource.CanJogressFromTargetPermanent(card.PermanentOfThisCard(), true);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDNACardCondition)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, permanent => permanent.IsDigimon && permanent != card.PermanentOfThisCard());

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                            CanSelectDNACardCondition,
                            payCost: true,
                            isHand: true,
                            activateClass,
                            permanentConditions: new Func<Permanent, bool>[] { permanent => permanent == card.PermanentOfThisCard() }
                        ));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
