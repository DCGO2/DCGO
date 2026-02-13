using System.Collections;
using System.Collections.Generic;
using System;

//Unique Emblem: Melting Recital
namespace DCGO.CardEffects.P
{
    public class P_232 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 3, add 1 [Evil]/[Dark Dragon]/[Evil Dragon] trait and 1 [Liberator] trait, bot deck the rest, place in battle area.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Reveal the top 3 cards of your deck. Add 1 [Evil], [Dark Dragon] or [Evil Dragon] trait Digimon card and 1 [LIBERATOR] trait card among them to the hand. Return the rest to the bottom of the deck. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Evil")
                        || cardSource.EqualsTraits("Dark Dragon")
                        || cardSource.EqualsTraits("Evil Dragon");
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("LIBERATOR");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 [Evil]/[Dark Dragon]/[Evil Dragon] trait card.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 [LIBERATOR] trait card.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));

                }
            }

            #endregion

            #region Your turn - Delay

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a level 6 or lower [LIBERATOR] Digimon for 3 less.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your [Yuuki]s are played, <Delay>.\r\n 1 of your Digimon may digivolve into a level 6 or lower [LIBERATOR] trait card in the hand with the digivolution cost reduced by 3.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanDeclareOptionDelayEffect(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool PlayedPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsCardName("Yuuki");
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 6
                        && cardSource.HasLiberatorTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool delaySuccessful = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        delaySuccessful = true;
                        yield return null;
                    }

                    if (delaySuccessful)
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition))
                        {
                            Permanent selectedDigimon = null;

                            #region Select Permament

                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, PermanentCondition));
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: PermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedDigimon = permanent;
                                yield return null;
                            }

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to digivolve", "The opponent is selecting 1 Digimon to digivolve");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                            bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                            if (canSelectHand || canSelectTrash)
                            {
                                if (canSelectHand && canSelectTrash)
                                {
                                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                    {
                                        new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                        new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                                    };

                                    string selectPlayerMessage = "From which area do you digivolve a card?";
                                    string notSelectPlayerMessage = "The opponent is choosing from which area to digivolve a card.";

                                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                                }
                                else
                                {
                                    GManager.instance.userSelectionManager.SetBool(canSelectHand);
                                }

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: selectedDigimon,
                                        cardCondition: CanSelectCardCondition,
                                        payCost: true,
                                        reduceCostTuple: (reduceCost: 3, reduceCostCardCondition: null),
                                        fixedCostTuple: null,
                                        ignoreDigivolutionRequirementFixedCost: -1,
                                        isHand: fromHand,
                                        activateClass: activateClass,
                                        successProcess: null));
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(
                    card: card,
                    cardEffects: ref cardEffects,
                    effectName: "Reveal 3, add 1 [Evil]/[Dark Dragon]/[Evil Dragon] trait and 1 [Liberator] trait, bot deck the rest, place in battle area.");
            }

            #endregion

            return cardEffects;
        }
    }
}
