using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// Asuna Shiroki
namespace DCGO.CardEffects.BT25
{
    public class BT25_092 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared Condition
            bool Valid3MOrTSCard(CardSource cardSource)
            {
                return cardSource.HasText("Three Musketeers")
                    || cardSource.HasTSTraits;
            }
            #endregion

            #region Start of your Main Phase
            if(timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.StartOfYourMainPhaseClass(
                    card,
                    "By trashing a [Three Musketeers] in test or [TS] trait card form hand, <Draw 1> and gain 1 memory",
                    ActivateCoroutine,
                    EffectDescription(),
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true
                    ));

                string EffectDescription() => "[Start of Your Main Phase] By trashing 1 card with [Three Musketeers] in its text or [TS] trait from your hand, <Draw 1> and gain 1 memory.";

                bool AdditionalActivateCondition(Hashtable hashtable) => CardEffectCommons.HasMatchConditionOwnersHand(card, Valid3MOrTSCard);

                IEnumerator ActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
                {
                    bool discarded = false;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: Valid3MOrTSCard,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 1)
                        {
                            discarded = true;

                            yield return null;
                        }
                    }

                    if (discarded)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                        
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 card from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() 
                    => "[Main] By suspending this Tamer and trashing 1 Option card from your hand or your Digimon's digivolution cards, 1 of your Digimon may digivolve into a Digimon card with [Three Musketeers] in its text or the [TS] trait in the hand or trash with the cost reduced by 1.";

                bool ValidTrashCard(CardSource cardSource) => cardSource.IsOption;

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Any(ValidTrashCard);
                }

                bool IsOwnDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateSuspendCostEffect(card)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, ValidTrashCard)
                            || CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass,
                        SuccessProcess,
                        null));

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermaments)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: ValidTrashCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());
                        
                        if(CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsOwnDigimon))
                        {
                            Permanent selectedPermanent = null;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsOwnDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to Digivolve", "The opponent is selecting 1 Digimon to return to digivolve");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                bool ValidTarget(CardSource cardSource, SelectCardEffect.Root root)
                                {
                                    return Valid3MOrTSCard(cardSource)
                                        && cardSource.CanPlayCardTargetFrame(selectedPermanent.PermanentFrame, false, activateClass, root);
                                }

                                bool ValidCardInHand = card.Owner.HandCards.Any(cardSource => ValidTarget(cardSource, SelectCardEffect.Root.Hand));
                                bool ValidCardInTrash = card.Owner.TrashCards.Any(cardSource => ValidTarget(cardSource, SelectCardEffect.Root.Trash));

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
                                    Valid3MOrTSCard,
                                    payCost: true,
                                    reduceCostTuple: (1, null),
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

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
