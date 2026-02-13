using System;
using System.Collections;
using System.Collections.Generic;

// TigerVespamon
namespace DCGO.CardEffects.BT23
{
    public class BT23_045 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blast Digivolve

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }

            #endregion

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel5
                        && (targetPermanent.TopCard.HasCSTraits || targetPermanent.TopCard.HasRoyalBaseTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion Alternative Digivolution Condition

            #region OP/WD Shared

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && (cardSource.HasRoyalBaseTraits || cardSource.EqualsTraits("Zaxon"));
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
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

                    CardSource selectedCard = null;
                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (fromHand)
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

                        selectHandEffect.SetUpCustomMessage("Select 1 digimon to place in security.", "The opponent is selecting 1 digimon to place in security.");
                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
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
                            message: "Select 1 digimon to place in security.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 digimon to place in security.", "The opponent is selecting 1 digimon to place in security.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(selectedCard, toTop: false, faceUp: true));

                        bool CanSelectPermanentCondition(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                                && permanent.HasDP && permanent.DP <= card.PermanentOfThisCard().DP;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            Permanent selectedPermanent = null;
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to bounce to hand.", "The opponent is selecting 1 Digimon to bounce to hand.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BouncePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { selectedPermanent },
                                activateClass: activateClass,
                                successProcess: null,
                                failureProcess: null));
                        }
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Royal Base]/[Zaxon] as bottom security card, bounce 1 digimon to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By placing 1 [Royal Base] or [Zaxon] trait Digimon card from your hand or trash face up as the bottom security card, return 1 of your opponent's Digimon with as much or less DP as this Digimon to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.CanAddSecurity(activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition) || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition));
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Royal Base]/[Zaxon] as bottom security card, bounce 1 digimon to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By placing 1 [Royal Base] or [Zaxon] trait Digimon card from your hand or trash face up as the bottom security card, return 1 of your opponent's Digimon with as much or less DP as this Digimon to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.CanAddSecurity(activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition) || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition));
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By flipping your top face up security card face down, unsuspend 1 digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When this Digimon suspends, by flipping your top face-up security card face down, 1 of your Digimon unsuspends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenSelfPermanentSuspends(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersSecurity(card, _ => true, false);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    var flippedSecurity = false;
                    foreach (CardSource source in card.Owner.SecurityCards)
                    {
                        if (source.IsFlipped) continue;

                        source.SetReverse();
                        GManager.OnSecurityStackChanged?.Invoke(card.Owner.Enemy);
                        flippedSecurity = true;
                        break;
                    }

                    if (flippedSecurity && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.UnTap,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to unsuspend.", "The opponent is selecting 1 Digimon to unsuspend.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                }
            }

            #endregion

            return cardEffects;
        }
    }
}