using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT20
{
    public class BT20_093 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main Effect

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Main] You may play 1 Digimon card with [Dracomon]/[Examon] in its text from your hand with the play cost reduced by 3. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           (cardSource.HasText("Dracomon") || cardSource.HasText("Examon")) &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        #region Reduce Play Cost

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Play Cost -3", CanUseReduceCondition, card);
                        changeCostClass.SetUpChangeCostClass(
                            changeCostFunc: ChangeCost,
                            cardSourceCondition: CardSourceCondition,
                            rootCondition: RootCondition,
                            isUpDown: IsUpDown,
                            isCheckAvailability: () => false,
                            isChangePayingCost: () => true);
                        Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                        card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                        ICardEffect GetCardEffect(EffectTiming effectTiming)
                        {
                            if (effectTiming == EffectTiming.None)
                            {
                                return changeCostClass;
                            }

                            return null;
                        }

                        bool CanUseReduceCondition(Hashtable reduceHashtable)
                        {
                            return true;
                        }

                        int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                        {
                            if (CardSourceCondition(cardSource))
                            {
                                if (RootCondition(root))
                                {
                                    if (PermanentsCondition(targetPermanents))
                                    {
                                        cost -= 3;
                                    }
                                }
                            }

                            return cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            if (targetPermanents == null)
                            {
                                return true;
                            }

                            if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                            {
                                return true;
                            }

                            return false;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            return cardSource.HasPlayCost && CanSelectCardCondition(cardSource);
                        }

                        bool RootCondition(SelectCardEffect.Root root)
                        {
                            return true;
                        }

                        bool IsUpDown()
                        {
                            return true;
                        }

                        #endregion

                        List<CardSource> selectedCards = new List<CardSource>();

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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: true,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                    }

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            #region Delay Effect

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("2 of your Digimon may DNA digivolve into [Examon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("DNAExamon_BT20_093");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] When any of your Digimon with [Dracomon]/[Examon] in their texts would leave the battle area other than in battle, [Delay].・ 2 of your Digimon may DNA digivolve into [Examon] in the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanDeclareOptionDelayEffect(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition) &&
                           !CardEffectCommons.IsByBattle(hashtable);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           (permanent.TopCard.HasText("Dracomon") || permanent.TopCard.HasText("Examon"));
                }

                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.CanPlayJogress(true) &&
                           cardSource.ContainsCardName("Examon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass: activateClass, successProcess: _ => SuccessProcess(),
                            failureProcess: null));
                }

                IEnumerator SuccessProcess()
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDNACardCondition,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to DNA digivolve.",
                        "The opponent is selecting 1 card to DNA digivolve.");
                    selectHandEffect.SetNotShowCard();

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        foreach (CardSource selectedCard in selectedCards)
                        {
                            if (selectedCard.CanPlayJogress(true))
                            {
                                _jogressEvoRootsFrameIDs = Array.Empty<int>();

                                yield return GManager.instance.photonWaitController.StartWait("Eldradimon_EX7_047");

                                if (card.Owner.isYou || GManager.instance.IsAI)
                                {
                                    GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                    (card: selectedCard,
                                        isLocal: true,
                                        isPayCost: true,
                                        canNoSelect: true,
                                        endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutineSelectDigivolutionRoots,
                                        noSelectCoroutine: null);

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectJogressEffect
                                        .SelectDigivolutionRoots());

                                    IEnumerator EndSelectCoroutineSelectDigivolutionRoots(List<Permanent> permanents)
                                    {
                                        if (permanents.Count == 2)
                                        {
                                            _jogressEvoRootsFrameIDs = permanents.Distinct().ToArray()
                                                .Map(permanent => permanent.PermanentFrame.FrameID);
                                        }

                                        yield return null;
                                    }

                                    photonView.RPC("SetJogressEvoRootsFrameIDs", RpcTarget.All, _jogressEvoRootsFrameIDs);
                                }

                                else
                                {
                                    GManager.instance.commandText.OpenCommandText("The opponent is choosing a card to DNA digivolve.");
                                }

                                yield return new WaitWhile(() => !_endSelect);
                                _endSelect = false;

                                GManager.instance.commandText.CloseCommandText();
                                yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                                if (_jogressEvoRootsFrameIDs.Length == 2)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                        .ShowCardEffect(new List<CardSource>() { selectedCard }, "Played Card", true, true));

                                    PlayCardClass playCard = new PlayCardClass(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                        payCost: true,
                                        targetPermanent: null,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Hand,
                                        activateETB: true);

                                    playCard.SetJogress(_jogressEvoRootsFrameIDs);

                                    yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
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
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Dracomon] card from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Security] You may play 1 Digimon card with [Dracomon] in its name from your hand or trash without paying the cost. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.ContainsCardName("Dracomon") &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                            {
                                new(message: "From hand", value: true, spriteIndex: 0),
                                new(message: "From trash", value: false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "From which area do you play a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                                selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                                notSelectPlayerMessage: notSelectPlayerMessage);
                        }

                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

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

                            selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

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
                                message: "Select 1 card to play.",
                                maxCount: 1,
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

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false,
                            root: fromHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash, activateETB: true));
                    }

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            return cardEffects;
        }

        #region Photon DNA

        bool _endSelect;
        int[] _jogressEvoRootsFrameIDs = Array.Empty<int>();

        [PunRPC]
        public void SetJogressEvoRootsFrameIDs(int[] jogressEvoRootsFrameIDs)
        {
            this._jogressEvoRootsFrameIDs = jogressEvoRootsFrameIDs;
            _endSelect = true;
        }

        #endregion
    }
}