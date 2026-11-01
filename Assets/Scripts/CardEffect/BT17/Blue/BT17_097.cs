using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Return to the Primogenitor
namespace DCGO.CardEffects.BT17
{
    public class BT17_097 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main Effect
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into level 5 or higher [Free] trait Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] 1 of your Digimon may digivolve into a level 5 or higher Digimon card with the [Free] trait in your hand with the digivolution cost reduced by 4. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectHandCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.Level >= 5
                        && cardSource.CardTraits.Contains("Free");
                }

                bool CanSelectOwnPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && card.Owner.HandCards.Where(CanSelectHandCardCondition).Any(cardSource => cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOwnPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.",
                            "The opponent is selecting 1 Digimon that will digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: CanSelectHandCardCondition,
                                payCost: true,
                                reduceCostTuple: (reduceCost: 4, reduceCostCardCondition: null),
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region Delay Effect
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                List<Permanent> removedPermanents = null;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a Digimon card with [Imperialdramon] in its name in your hand to prevent deletion", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When one of your Digimon with the [Free] trait would be deleted other than by one of your effects, [Delay] • By digivolving that Digimon into a Digimon card with [Imperialdramon] in its name in your hand without paying the cost, prevent that deletion.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanDeclareOptionDelayEffect(card)
                    && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsOwnerPermanentCondition)
                    && !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card)))
                    {
                        removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(IsOwnerPermanentCondition);
                            return true;
                    }

                    return false;
                }

                bool CanSelectHandCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.ContainsCardName("Imperialdramon");
                }


                bool IsOwnerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.ContainsTraits("Free")
                        && card.Owner.HandCards.Where(CanSelectHandCardCondition).Any(cardSource => cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass, successProcess: permanents => SuccessProcess(),
                        failureProcess: null
                    ));

                    IEnumerator SuccessProcess()
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(removedPermanents.Contains))
                        {
                            Permanent selectedPermanent = null;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: removedPermanents.Contains,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to digivolve.", "The opponent is selecting 1 Digimon to digivolve.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: selectedPermanent,
                                    cardCondition: CanSelectHandCardCondition,
                                    payCost: false,
                                    reduceCostTuple: null,
                                    fixedCostTuple: null,
                                    ignoreDigivolutionRequirementFixedCost: -1,
                                    isHand: true,
                                    activateClass: activateClass,
                                    successProcess: null
                                ));

                                selectedPermanent.willBeRemoveField = false;
                                selectedPermanent.HideDeleteEffect();
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Davis Motomiya] or [Ken Ichijoji] from hand or trash and place this card in the battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] You may play 1 Tamer card with [Davis Motomiya]/[Ken Ichijoji] in its name from your hand or trash without paying the cost. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsTamer
                        && (cardSource.ContainsCardName("Davis Motomiya")
                            || cardSource.ContainsCardName("Ken Ichijoji"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        SelectCardEffect.Root root;

                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                            {
                                new(message: "From hand", value: 1, spriteIndex: 0),
                                new(message: "From trash", value: 2, spriteIndex: 0),
                                new(message: "Don't play", value: 3, spriteIndex: 1),
                            };

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: "From which area will you play a card?", notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                            root = GManager.instance.userSelectionManager.SelectedIntValue == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        }
                        else
                        {
                            root = canSelectHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanSelectCardCondition,
                            root: root,
                            cardEffect: activateClass,
                            payCost: false
                        ));
                    }

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}