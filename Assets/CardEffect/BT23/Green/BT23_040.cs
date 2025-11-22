using System;
using System.Collections;
using System.Collections.Generic;

// Wormmon
namespace DCGO.CardEffects.BT23
{
    public class BT23_040 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasCSTraits
                        && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel2;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Start Of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Erika Mishima] as bottom digivolution card, this digimon may digivolve into a [Hudie] digimon in hand/trash for 2 reduced cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By placing 1 of your [Erika Mishima] as this Digimon's bottom digivolution card, this Digimon may digivolve into [Hudiemon] in the hand or trash with the digivolution cost reduced by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionPermanent(IsErikaMishima);
                }

                bool IsErikaMishima(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.IsTamer
                        && permanent.TopCard.EqualsCardName("Erika Mishima");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermament = card.PermanentOfThisCard();
                    if (CardEffectCommons.HasMatchConditionPermanent(IsErikaMishima))
                    {
                        Permanent selectedErikaMishima = null;

                        #region Select Erika Mishima

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsErikaMishima));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsErikaMishima,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedErikaMishima = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Erika Mishima] to place as bottom digivolution source", "The opponent is selecting 1 [Erika Mishima] to place as bottom digivolution source.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        if (selectedErikaMishima != null)
                        {
                            #region Place Erika Mishima

                            var placeSources = new IPlacePermanentToDigivolutionCards(
                                permanentArrays: new List<Permanent[]>() { new Permanent[] { selectedErikaMishima, thisPermament } },
                                toTop: false,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(placeSources.PlacePermanentToDigivolutionCards());

                            #endregion

                            bool SelectSourceCard(CardSource cardSource)
                            {
                                return cardSource.IsDigimon
                                    && cardSource.EqualsCardName("Hudiemon")
                                    && cardSource.CanPlayCardTargetFrame(thisPermament.PermanentFrame, true, activateClass);
                            }

                            bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, SelectSourceCard);
                            bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, SelectSourceCard);

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

                                bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                                #endregion

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: thisPermament,
                                    cardCondition: SelectSourceCard,
                                    payCost: true,
                                    reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
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

            #region All Turns

            if (timing == EffectTiming.None)
            {
                string EffectDiscription()
                {
                    return "[All Turns] All of your Digimon with the [Hudie] trait get +1000 DP.";
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasHudieTraits;
                }

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 1000,
                isInheritedEffect: true,
                card: card,
                condition: Condition,
                effectName: EffectDiscription));
            }

            #endregion

            return cardEffects;
        }
    }
}