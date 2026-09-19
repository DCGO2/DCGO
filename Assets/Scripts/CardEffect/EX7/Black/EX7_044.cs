using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX7
{
    public class EX7_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 && targetPermanent.TopCard.HasText("Three Musketeers");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region On Play/ When Digivolving Shared
            
            bool CanSelectCardSharedCondition(CardSource cardSource)
            {
                return cardSource.IsOption &&
                       cardSource.ContainsTraits("Three Musketeers");
            }

            bool CanSelectOpponentPermanentSharedCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card) &&
                       (permanent.IsDigimon || permanent.IsTamer) &&
                       permanent.TopCard.HasPlayCost &&
                       permanent.TopCard.GetCostItself <= 3;
            }

            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            #endregion
            
            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 4 cards of deck, then delete opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] Reveal the top 4 cards of your deck. Place 1 Option card with the [Three Musketeers] trait among them as this Digimon's bottom digivolution card. Return the rest to the top or bottom of the deck. If this effect placed, delete 1 of your opponent's Digimon or Tamers with a play cost of 3 or less.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool cardWasPlaced = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 4,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new(
                                canTargetCondition: CanSelectCardSharedCondition,
                                message: "Select 1 Option card with [Three Musketeers] in its traits.",
                                mode: SelectCardEffect.Mode.Custom,
                                maxCount: 1,
                                selectCardCoroutine: SelectCardCoroutine)
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                        activateClass: activateClass
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard()
                                .AddDigivolutionCardsBottom(new List<CardSource>() { cardSource }, activateClass));

                            cardWasPlaced = true;
                        }
                    }

                    if (cardWasPlaced && CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentSharedCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentPermanentSharedCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion
            
            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 4 cards of deck, then delete opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] Reveal the top 4 cards of your deck. Place 1 Option card with the [Three Musketeers] trait among them as this Digimon's bottom digivolution card. Return the rest to the top or bottom of the deck. If this effect placed, delete 1 of your opponent's Digimon or Tamers with a play cost of 3 or less.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool cardWasPlaced = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 4,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new(
                                canTargetCondition: CanSelectCardSharedCondition,
                                message: "Select 1 Option card with [Three Musketeers] in its traits.",
                                mode: SelectCardEffect.Mode.Custom,
                                maxCount: 1,
                                selectCardCoroutine: SelectCardCoroutine)
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                        activateClass: activateClass
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard()
                                .AddDigivolutionCardsBottom(new List<CardSource>() { cardSource }, activateClass));

                            cardWasPlaced = true;
                        }
                    }

                    if (cardWasPlaced && CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentSharedCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentPermanentSharedCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            #region Collision - ESS

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}