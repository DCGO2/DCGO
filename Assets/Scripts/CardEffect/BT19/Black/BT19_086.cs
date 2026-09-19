using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_086: CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place a Device option in the battle area to Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Start of Your Main Phase] By placing 1 Option card with the [Device] trait from your hand in your battle area, <Draw 1>.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsOption && cardSource.EqualsTraits("Device");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    CardSource selectedCard = null;

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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to place in the battle area.",
                        "The opponent is selecting 1 card to place in the battle area.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Battle Area");

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;

                        yield return null;
                    }

                    yield return StartCoroutine(selectHandEffect.Activate());

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: selectedCard, cardEffect: activateClass));

                        if(CardEffectCommons.IsExistOnBattleArea(selectedCard))
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
            }
            #endregion
            
            #region Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 4 Device options in battle area, play a Cyberdramon without paying the cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Main] By suspending this Tamer and trashing 4 Option cards with the [Device] trait in your battle area, you may play 1 [Cyberdramon] from your hand or trash without paying the cost.";
                }

                bool IsDeviceTrait(Permanent permanent)
                {
                    return permanent.IsOption &&
                           permanent.TopCard.EqualsTraits("Device") &&
                           CardEffectCommons.IsOwnerPermanent(permanent, card);
                }

                bool IsCyberdramon(CardSource source)
                {
                    return source.EqualsCardName("Cyberdramon") && CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return isExistOnField(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsDeviceTrait) >= 4;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    List<Permanent> selectedPermanents = new List<Permanent>();

                    SelectPermanentEffect selectPermanentEffect =
                        GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsDeviceTrait,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 4,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Option to trash.",
                        "The opponent is selecting 1 Option to trash.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanents.Add(permanent);
                        yield return null;
                    }

                    if (selectedPermanents.Count >= 4)
                    {
                        var deleted = 0;
                        
                        foreach (Permanent permanent in selectedPermanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                    targetPermanents: new List<Permanent>() { permanent },
                                    activateClass: activateClass,
                                    successProcess: permanents => SuccessProcess(),
                                    failureProcess: null));
                        }
                        
                        IEnumerator SuccessProcess()
                        {
                            deleted++;
                            yield return null;
                        }

                        if (deleted >= 4)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsCyberdramon))
                            {
                                
                            }

                            bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, IsCyberdramon);
                            bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsCyberdramon);

                            if (canSelectHand || canSelectTrash)
                            {
                                if (canSelectHand && canSelectTrash)
                                {
                                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                                    string selectPlayerMessage = "From which area do you play a card?";
                                    string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

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
                                    int maxCount = Math.Min(1, card.Owner.HandCards.Count(IsCyberdramon));

                                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                    selectHandEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: IsCyberdramon,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        isShowOpponent: false,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        mode: SelectHandEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectHandEffect.SetUpCustomMessage("Select [Cyberdramon] to play.", "The opponent is selecting an [Cyberdramon] to play.");

                                    yield return StartCoroutine(selectHandEffect.Activate());
                                }
                                else
                                {
                                    int maxCount = 1;

                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: IsCyberdramon,
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

                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                                }


                                SelectCardEffect.Root root = SelectCardEffect.Root.Hand;

                                if (!fromHand)
                                    root = SelectCardEffect.Root.Trash;

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: root,
                                    activateETB: true));
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