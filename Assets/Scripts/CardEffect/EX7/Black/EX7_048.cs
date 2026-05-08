using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Gundramon
namespace DCGO.CardEffects.EX7
{
    public class EX7_048 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Three Musketeers");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
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

            #region Shared OP / WD
            string SharedEffectName = "Reveal the top 6 cards of deck, use an Option card";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] Reveal the top 6 cards of your deck. You may use 1 Option card with the [Three Musketeers] trait among them without paying the cost. Return the rest to the top or bottom of the deck.";

            bool CanSelectCardSharedCondition(CardSource cardSource)
            {
                return cardSource.IsOption &&
                       !cardSource.CanNotPlayThisOption &&
                       cardSource.ContainsTraits("Three Musketeers");
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 6,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: CanSelectCardSharedCondition,
                            message: "Select 1 Option card with the [Three Musketeers] trait.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                    activateClass: activateClass,
                    canNoSelect: true
                ));

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlayOptionCards(
                            cardSources: new List<CardSource>() { cardSource },
                            activateClass: activateClass,
                            payCost: false,
                            root: SelectCardEffect.Root.Library
                        )
                    );
                    yield return null;
                }
            }
            #endregion

            #region All Turns

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Trash 1 Option card in this Digimon's digivolution cards to prevent your Digimon from leaving Battle Area",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("Substitute_EX7_48");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] When your Digimon with the [Three Musketeers] trait would leave the battle area other than by your effects, by trashing 1 Option card in this Digimon's digivolution cards, prevent it.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsOwnerPermanentCondition) &&
                           !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));
                }

                bool IsOwnerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.ContainsTraits("Three Musketeers");
                }

                bool IsOwnerPermanentToBeDeletedCondition(Permanent permanent)
                {
                    return IsOwnerPermanentCondition(permanent) && permanent.willBeRemoveField;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsOption &&
                           !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOwnerPermanentToBeDeletedCondition))
                    {
                        Permanent thisCardPermanent = card.PermanentOfThisCard();

                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOwnerPermanentToBeDeletedCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon to prevent the deletion of.",
                            "The opponent is selecting 1 Digimon to prevent the deletion of.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null && thisCardPermanent.DigivolutionCards.Some(CanSelectCardCondition))
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 Digivolution card to trash.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: thisCardPermanent.DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetNotShowCard();
                            yield return StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count == 1)
                            {
                                selectedPermanent.willBeRemoveField = false;

                                selectedPermanent.HideDeleteEffect();
                                selectedPermanent.HideHandBounceEffect();
                                selectedPermanent.HideDeckBounceEffect();
                                selectedPermanent.HideWillRemoveFieldEffect();

                                yield return ContinuousController.instance.StartCoroutine(
                                    new ITrashDigivolutionCards(thisCardPermanent, selectedCards, activateClass)
                                        .TrashDigivolutionCards());
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
