using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_048 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardColors.Contains(CardColor.Black) &&
                           targetPermanent.TopCard.IsLevel2 &&
                           targetPermanent.TopCard.HasXAntibodyTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] Reveal the top 3 cards of your deck. Add 1 card with [X Antibody] trait and 1 Tamer card or Option card with the [Chronicle] trait among them to the hand. Return the rest to the bottom of the deck.";
                }

                bool CanSelectXAntiCondition(CardSource cardSource)
                {
                    return cardSource.HasXAntibodyTraits;
                }

                bool CanSelectChronicleCondition(CardSource cardSource)
                {
                    return (cardSource.IsTamer || cardSource.IsOption) &&
                           cardSource.ContainsTraits("Chronicle");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.Owner.LibraryCards.Count >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new SimplifiedSelectCardConditionClass[]
                            {
                                new(
                                    canTargetCondition: CanSelectXAntiCondition,
                                    message: "Select 1 card with the [X Antibody] trait.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                                new(
                                    canTargetCondition: CanSelectChronicleCondition,
                                    message: "Select 1 Tamer or Option card with the [Chronicle] trait.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                            },
                            remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                            activateClass: activateClass
                        ));
                }
            }

            #endregion

            #region Opponent's Turn - ESS

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOpponentTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}