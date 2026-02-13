using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX9
{
    public class EX9_058 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel2 &&
                        targetPermanent.TopCard.EqualsTraits("DM");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 3, Add 1 [DM] trait, and place 1 [Ver.5] trait under a [DM] digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[On Play] Reveal the top 3 cards of your deck. Among them, add 1 card with the [DM] trait to the hand and place 1 card with the [Ver.5] trait face down as the bottom digivolution card of any of your Digimon with the [DM] trait . Return the rest to the bottom of the deck.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        card.Owner.LibraryCards.Count > 0;
                }

                bool HadDMTrait(CardSource source)
                {
                    return source.EqualsTraits("DM");
                }

                bool HasVer5Trait(CardSource source)
                {
                    return source.EqualsTraits("Ver.5");
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                        permanent.TopCard.EqualsTraits("DM");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                            new SimplifiedSelectCardConditionClass[]
                            {
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition:HadDMTrait,
                                    message: "Select 1 [DM] card.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition:HasVer5Trait,
                                    message: "Select 1 [Ver.5] card.",
                                    mode: SelectCardEffect.Mode.Custom,
                                    maxCount: 1,
                                    selectCardCoroutine: SelectCardCoroutine),
                            },
                            remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                            activateClass: activateClass));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null && CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(selectedCard));
                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass, isFacedown: true));
                        }
                    }
                }
            }

            #endregion

            #region Retaliation - ESS

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}