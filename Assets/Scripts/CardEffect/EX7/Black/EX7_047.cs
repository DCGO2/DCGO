using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Eldradimon
namespace DCGO.CardEffects.EX7
{
    public class EX7_047 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("NSp");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5)
                );
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region OP/WD Shared
            string SharedEffectName = "Reveal the top 4 cards of your deck and play Digimon from them";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] Reveal the top 4 cards of your deck. You may play up to 7 play cost's total worth of Digimon cards with the [NSp] trait among them without paying the costs. Return the rest to the bottom of the deck.";
          
            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 7
                        && cardSource.EqualsTraits("NSp")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                int maxCount = card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame());

                List<CardSource> selectedCards = new List<CardSource>();

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 4,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                    new(
                        canTargetCondition: CanSelectCardCondition,
                        message: "Select cards with the [NSp] whose play costs add up to 7 or less.",
                        mode: SelectCardEffect.Mode.Custom,
                        maxCount: maxCount,
                        selectCardCoroutine: SelectCardCoroutine),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass,
                    canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                    canEndSelectCondition: CanEndSelectCondition,
                    canNoSelect: true,
                    canEndNotMax: true
                ));

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);

                    yield return null;
                }

                bool CanTargetConditionByPreSelectedList(List<CardSource> cardSources, CardSource cardSource)
                {
                    return cardSources.Sum(source => source.GetCostItself) + cardSource.GetCostItself <= 7;
                }

                bool CanEndSelectCondition(List<CardSource> cardSources)
                {
                    return cardSources.Count > 0 &&
                           cardSources.Sum(source => source.GetCostItself) <= 7;
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                    cardSources: selectedCards,
                    activateClass: activateClass,
                    payCost: false,
                    isTapped: false,
                    root: SelectCardEffect.Root.Library,
                    activateETB: true));
            }
            #endregion

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA digivolve into a Digimon card with the [NSp] trait", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("DNA_EX7_047");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Your Turn] (Once Per Turn) 2 of your Digimon may DNA digivolve into a Digimon card with the [NSp] trait in your hand.";
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.CanPlayJogress(true) &&
                           cardSource.EqualsTraits("NSp");
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDNACardCondition)
                        && card.Owner.GetBattleAreaDigimons().Count >= 2;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                            CanSelectDNACardCondition,
                            payCost: true,
                            isHand: true,
                            activateClass
                        ));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}