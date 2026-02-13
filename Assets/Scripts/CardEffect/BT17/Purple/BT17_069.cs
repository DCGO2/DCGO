using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_069 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("SoC") &&
                           targetPermanent.TopCard.HasLevel &&
                           targetPermanent.TopCard.Level == 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition,
                    digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] If [Eiji Nagasumi] is in this Digimon's digivolution cards, you may play 1 [Fenriloogamon]/[Kazuchimon] from your trash without paying the cost. At the next end of your opponent's turn, return that Digimon to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                    }

                    return false;
                }

                bool CanSelectTrashCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass) &&
                           (cardSource.EqualsCardName("Fenriloogamon") || cardSource.EqualsCardName("Kazuchimon"));
                }
                
                bool IsEijiCardCondition(CardSource cardSource)
                {
                    return cardSource.IsTamer &&
                           (cardSource.EqualsCardName("Eiji Nagasumi") ||
                            cardSource.EqualsCardName("EijiNagasumi"));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Some(IsEijiCardCondition) &&
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectTrashCardCondition));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectTrashCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to play.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false,
                            root: SelectCardEffect.Root.Trash, activateETB: true));

                        if (selectedCards.Count > 0)
                        {
                            Permanent selectedPermanent = selectedCards[0].PermanentOfThisCard();

                            ActivateClass activateDebuffClass = new ActivateClass();
                            activateDebuffClass.SetUpICardEffect("Return this Digimon to the hand", CanUseDebuffCondition,
                                selectedPermanent.TopCard);
                            activateDebuffClass.SetUpActivateClass(CanActivateDebuffCondition, ActivateDebuffCoroutine, -1, false,
                                EffectDebuffDescription());
                            activateDebuffClass.SetEffectSourcePermanent(selectedPermanent);
                            selectedPermanent.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                    .CreateDebuffEffect(selectedPermanent));
                            }

                            string EffectDebuffDescription()
                            {
                                return "[End of Opponent's Turn] Return this Digimon to the hand.";
                            }

                            bool CanUseDebuffCondition(Hashtable debuffHashtable)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) &&
                                       GManager.instance.turnStateMachine.gameContext.TurnPlayer != selectedPermanent.TopCard.Owner;
                            }

                            bool CanActivateDebuffCondition(Hashtable debuffHashtable)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) &&
                                       !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                            }

                            IEnumerator ActivateDebuffCoroutine(Hashtable debuffHashtable)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                        CardEffectCommons.BouncePeremanentAndProcessAccordingToResult(
                                            targetPermanents: new List<Permanent>() { selectedPermanent },
                                            activateClass: activateDebuffClass,
                                            successProcess: null,
                                            failureProcess: null));
                                }
                            }

                            ICardEffect GetCardEffect(EffectTiming debuffTiming)
                            {
                                if (debuffTiming == EffectTiming.OnEndTurn)
                                {
                                    return activateDebuffClass;
                                }

                                return null;
                            }
                        }
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Delete 1 of your opponent's Digimon with 10000 DP or less", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    EffectDescription());
                activateClass.SetHashString("Delete10000_BT17_069");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Your Turn] [Once Per Turn] When one of your Digimon or Tamers that have the [SoC] trait or [Pulsemon] in its text is played, delete 1 of your opponent's Digimon with 10000 DP or less.";
                }

                bool PlayedPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card) &&
                           (permanent.IsDigimon || permanent.IsTamer) &&
                           (permanent.TopCard.ContainsTraits("SoC") || permanent.TopCard.HasText("Pulsemon"));
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.DP <= card.Owner.MaxDP_DeleteEffect(10000, activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                    {
                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentPermanentCondition,
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
            }

            #endregion

            #region Your Turn - ESS

            if (timing == EffectTiming.None)
            {
                ChangeEndTurnMinMemoryClass changeEndTurnMinMemoryClass = new ChangeEndTurnMinMemoryClass();
                changeEndTurnMinMemoryClass.SetUpICardEffect("The turn end condition is the opponent having 3 or more memory.",
                    CanUseCondition, card);
                changeEndTurnMinMemoryClass.SetUpChangeEndTurnMinMemoryClass(_ => 3);
                changeEndTurnMinMemoryClass.SetIsInheritedEffect(true);
                cardEffects.Add(changeEndTurnMinMemoryClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           card.PermanentOfThisCard().TopCard.ContainsCardName("Fenriloogamon");
                }
            }

            #endregion

            return cardEffects;
        }
    }
}