using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Gundramon // Gewalt Schwarmer
namespace DCGO.CardEffects.LM
{
    public class LM_067 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Three Musketeers")
                            || targetPermanent.TopCard.EqualsTraits("TS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Fragment
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                string EffectDescription()
                {
                    return "<Fragment <3>> (When this Digimon would be deleted, by trashing any 3 of its digivolution cards, it isn’t deleted.)";
                }

                cardEffects.Add(CardEffectFactory.FragmentSelfEffect(isInheritedEffect: false, card: card, condition: null, trashValue: 3, effectName: "Fragment <3>", effectDiscription: EffectDescription()));
            }
            #endregion

            #region Shared WD / C
            string SharedEffectName1 = "Reveal 6, play/use 1 6 cost or lower [Three Musketeer] text for free, bot deck the rest";
            string SharedEffectDescription1(string tag) => $"[{tag}] [Once Per Turn] Reveal the top 6 cards of your deck. You may play or use 1 play or use cost of 6 or lower [Three Musketeers] text card among them without paying the cost. Return the rest to the bottom of the deck.";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName1,
                    SharedActivateCoroutine1,
                    SharedEffectDescription1,
                    optional: false,
                    maxCountPerTurn: 1,
                    hashValue: "LM_067_WD_C",
                    whenDigivolving: true,
                    counter: true);

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                return cardSource.HasText("Three Musketeers")
                    && cardSource.GetCostItself <= 6;
            }

            IEnumerator SharedActivateCoroutine1(Hashtable hashtable, ActivateClass activateClass)
            {
                CardSource selectedCard = null;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 6,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:CanSelectCardCondition1,
                                message: "Select 1 Digimon card to Play or Use.",
                                mode: SelectCardEffect.Mode.Custom,
                                maxCount: 1,
                                selectCardCoroutine: SelectCardCoroutine),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass,
                    canNoSelect: true
                ));

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;

                    if (selectedCard.IsOption)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            root: SelectCardEffect.Root.Library));

                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Library,
                            activateETB: true));
                    }
                }
            }
            #endregion

            #region Shared WD / WA
            string SharedEffectName2 = "Trash up to 3 [Three Musketeers] text from sources, to delete as many enemy 7 cost or lower Digimon";
            string SharedEffectDescription2(string tag) => $"[{tag}] By trashing up to 3 [Three Musketeers] text cards from any of your Digimon's digivolution cards, for each one trashed, delete 1 of your opponent's play cost 7 or lower Digimon.";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName2,
                    SharedActivateCoroutine2,
                    SharedEffectDescription2,
                    optional: false,
                    isSkippable: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            bool CanSelectCardCondition2(CardSource cardSource)
            {
                return cardSource.HasText("Three Musketeers");
            }
            
            bool CanSelectOwnerPermanentCondition2(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Any(CanSelectCardCondition2);
            }

            bool CanSelectEnemyPermanentCondition2(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasPlayCost
                    && permanent.TopCard.GetCostItself <= 7;
            }

            IEnumerator SharedActivateCoroutine2(Hashtable hashtable, ActivateClass activateClass)
            {
                int trashedCount = 0;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                    permanentCondition: CanSelectOwnerPermanentCondition2,
                    cardCondition: CanSelectCardCondition2,
                    maxCount: 3,
                    canNoTrash: true,
                    isFromOnly1Permanent: false,
                    activateClass: activateClass,
                    afterSelectionCoroutine: AfterTrashedCards
                ));

                IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                {
                    trashedCount = cards.Count;

                    yield return null;
                }

                if (trashedCount > 0)
                {
                    int maxCount = Math.Min(trashedCount, CardEffectCommons.MatchConditionPermanentCount(CanSelectEnemyPermanentCondition2));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectEnemyPermanentCondition2,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion
            #endregion

            #region Option Effects
            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                    => cardSource.HasText("Three Musketeers");
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-Digivolve 3> 3 enemy Digimon, then may place [Three Musketeers] trait under your digimon from hand/trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] <De-Digivolve 3> 3 of your opponent's Digimon. Then, you may place 1 [Three Musketeers] trait card from your hand or trash as any of your Digimon's bottom digivolution card.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectEnemyPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Three Musketeers");
                }

                bool CanSelectOwnerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int maxCount = Math.Min(3, CardEffectCommons.MatchConditionPermanentCount(CanSelectEnemyPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectEnemyPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Degenerate,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetDegenerationCount(3);

                    selectPermanentEffect1.SetUpCustomMessage("Select 3 Digimon to De-Digivolve.", "The opponent is selecting 3 Digimon to De-Digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        bool selectedLocation = false;

                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                            {
                                new(message: "From hand", value: 1, spriteIndex: 0),
                                new(message: "From trash", value: 2, spriteIndex: 0),
                                new(message: "Don't select", value: 3, spriteIndex: 1),
                            };

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: "From which area will you select a card?", notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                            selectedLocation = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                        }
                        else
                        {
                            selectedLocation = canSelectHand;
                        }

                        CardSource selectedCard = null;

                        if (selectedLocation)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place under 1 of your Digimon.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: null);
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        if (selectedCard != null)
                        {
                            SelectPermanentEffect selectPermanentEffect2 = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect2.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOwnerPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect2.SetUpCustomMessage("Select 1 Digimon to place card under.", "The opponent is selecting 1 Digimon to place card under.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect2.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                yield return ContinuousController.instance.StartCoroutine(permanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
