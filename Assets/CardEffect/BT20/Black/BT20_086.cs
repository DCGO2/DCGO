using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT20
{
    public class BT20_086 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region Start of Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing a card as bottom digivolution source, flip opponents top security face up", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By placing 1 play cost 4 or lower black Digimon card with the [Cyborg] or [Machine] trait from your hand or trash as any of your [Cyborg] or [Machine] trait Digimon's bottom digivolution card, flip your opponent's top face-down security card face up.";
                }

                bool SelectPermanentCard(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           (permanent.TopCard.EqualsTraits("Cyborg") || permanent.TopCard.EqualsTraits("Machine"));
                }

                bool SelectSourceCard(CardSource source)
                {
                    return source.IsDigimon &&
                           source.HasPlayCost && source.GetCostItself <= 4 &&
                           source.CardColors.Contains(CardColor.Black) &&
                           (source.EqualsTraits("Cyborg") || source.EqualsTraits("Machine"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(SelectPermanentCard) &&
                           (CardEffectCommons.HasMatchConditionOwnersHand(card, SelectSourceCard) || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, SelectSourceCard));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(SelectPermanentCard));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SelectPermanentCard,
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

                    if(selectedPermanent != null)
                    {
                        bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, SelectSourceCard);
                        bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, SelectSourceCard);

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

                            List<CardSource> selectedCards = new List<CardSource>();

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (fromHand)
                            {
                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: SelectSourceCard,
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

                                selectHandEffect.SetUpCustomMessage("Select 1 card to add as source.", "The opponent is selecting 1 card to add as source.");

                                yield return StartCoroutine(selectHandEffect.Activate());
                            }
                            else
                            {
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: SelectSourceCard,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to add as source.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 card to add as source.", "The opponent is selecting 1 card to add as source.");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }

                            if(selectedCards.Count > 0)
                            {
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(selectedCards, activateClass));

                                foreach (CardSource source in card.Owner.Enemy.SecurityCards)
                                {
                                    if (!source.IsFlipped)
                                        continue;

                                    source.SetFace();
                                    GManager.OnSecurityStackChanged?.Invoke(card.Owner.Enemy);

                                    break;
                                }
                            }
                            
                        }
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion
            

            return cardEffects;
        }
    }
}