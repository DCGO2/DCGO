using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tankdramon
namespace DCGO.CardEffects.BT25
{
    public class BT25_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("D-Brigade")
                            || targetPermanent.TopCard.EqualsTraits("ACCEL");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 4));
            }
            #endregion

            #region WD/WA Shared
            string SharedEffectName = "Reveal 3, may play 1 12 cost or lower [D-Brigade]/[ACCEL] trait with cost reduced by 5, trash rest";

            string SharedHashString = "BT25_074_WD_WA";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: SharedHashString,
                whenDigivolving: true,
                whenAttacking: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] [Once Per Turn] Reveal the top 3 cards of your deck. You may play 1 cost 12 or lower [D-Brigade] or [ACCEL] trait card among them with the cost reduced by 5. Trash the rest.";
            }

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 12
                        && (cardSource.ContainsCardName("Chaosmon")
                            || cardSource.EqualsTraits("D-Brigade")
                            || cardSource.EqualsTraits("ACCEL"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 5);
                }

                List<CardSource> selectedCards = new List<CardSource>();

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 play cost 12 or lower [D-Brigade] or [ACCEL] trait Digimon card to play with the cost reduced by 5.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                    },
                    remainingCardsPlace: RemainingCardsPlace.Trash,
                    activateClass: activateClass,
                    canNoSelect: true
                ));

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);
                    yield return null;
                }

                #region reduce play cost
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Play Cost -5", CanUseCondition1, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                ICardEffect GetCardEffect(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.None)
                    {
                        return changeCostClass;
                    }

                    return null;
                }

                bool CanUseCondition1(Hashtable hashtable) => true;

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource)
                        && RootCondition(root)
                        && PermanentsCondition(targetPermanents))
                    {
                        Cost -= 5;
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null || targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 12
                        && (cardSource.ContainsCardName("Chaosmon")
                            || cardSource.EqualsTraits("D-Brigade")
                            || cardSource.EqualsTraits("ACCEL"));
                }

                bool RootCondition(SelectCardEffect.Root root) => true;

                bool isUpDown() => true;
                #endregion

                #region Play the card
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                    cardSources: selectedCards,
                    activateClass: activateClass,
                    payCost: false,
                    isTapped: false,
                    root: SelectCardEffect.Root.Library,
                    activateETB: true));
                #endregion

                #region release effect reducing play cost
                card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                #endregion
            }
            #endregion

            #region Inherited - Opponent's turn Reboot and Blocker
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOpponentTurn(card)
                        && ( card.PermanentOfThisCard().TopCard.ContainsCardName("Chaosmon")
                            || card.PermanentOfThisCard().TopCard.EqualsTraits("D-Brigade")
                            || card.PermanentOfThisCard().TopCard.EqualsTraits("ACCEL"));
                }

                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: true, card: card, condition: Condition));
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
