using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Unique Emblem: Ragnarok Attainer
namespace DCGO.CardEffects.P
{
    public class P_244 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Vemmon]/[Zenith] from hand or trash for free, then place in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] You may play 1 [Vemmon] or [Zenith] from your hand or trash without paying the cost. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return (cardSource.EqualsCardName("Vemmon")
                            || cardSource.EqualsCardName("Zenith"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition)
                    || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                        if (canSelectHand || canSelectTrash)
                        {
                            #region Setup Location Selection
                            if (canSelectHand && canSelectTrash)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                {
                                    new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                    new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                                };

                                string selectPlayerMessage = "From which area do you select a card?";
                                string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            }
                            else
                            {
                                GManager.instance.userSelectionManager.SetBool(canSelectHand);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                            #endregion

                            SelectCardEffect.Root root = GManager.instance.userSelectionManager.SelectedBoolValue ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;

                            #region Hand/Trash Card Selection & Play
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanSelectCardCondition,
                                root,
                                activateClass,
                                payCost: false
                            ));
                            #endregion
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region Your turn - Delay
            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a Digimon w/[Vemmon] in text for 3 less.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] When effects place [Vemmon] as any of your Digimon's digivolution cards, <Delay>.\r\n 1 of your Digimon with [Vemmon] in its text may digivolve into a Digimon card with [Vemmon] in its text in the hand or trash with the cost reduced by 3.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanDeclareOptionDelayEffect(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                            hashtable: hashtable,
                            permanentCondition: TriggerPermanentCondition,
                            cardEffectCondition: cardEffect => cardEffect.EffectSourceCard != null,
                            cardCondition: cardSource => cardSource.EqualsCardName("Vemmon"));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool TriggerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasText("Vemmon");
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasText("Vemmon");
                }

                bool ValidTarget(CardSource cardSource, SelectCardEffect.Root root, Permanent permanent)
                {
                    return CanSelectCardCondition(cardSource)
                        && cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass, root);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition))
                        {
                            Permanent selectedPermanent = null;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to digivolve.", "The opponent is selecting 1 Digimon to return to digivolve.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                bool ValidCardInHand = card.Owner.HandCards.Any(cardSource => ValidTarget(cardSource, SelectCardEffect.Root.Hand, selectedPermanent));
                                bool ValidCardInTrash = card.Owner.TrashCards.Any(cardSource => ValidTarget(cardSource, SelectCardEffect.Root.Trash, selectedPermanent));

                                if (ValidCardInHand && ValidCardInTrash)
                                {
                                    List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                                    {
                                        new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                        new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                                    };

                                    string selectPlayerMessage1 = "From which area do you select a card?";
                                    string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                                }
                                else
                                {
                                    GManager.instance.userSelectionManager.SetBool(ValidCardInHand);
                                }

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                var fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    selectedPermanent,
                                    CanSelectCardCondition,
                                    payCost: true,
                                    reduceCostTuple: (3, null),
                                    fixedCostTuple: null,
                                    ignoreDigivolutionRequirementFixedCost: -1,
                                    isHand: fromHand,
                                    activateClass,
                                    successProcess: null
                                ));
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
                    effectName: "Play 1 [Vemmon]/[Zenith] from hand or trash for free, then place in battle area");
            }
            #endregion

            return cardEffects;
        }
    }
}
