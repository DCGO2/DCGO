using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using Photon.Pun;
using System;
using UnityEngine.Events;

public class SelectDigiXrosClass : MonoBehaviourPunCallbacks
{
    public List<CardSource> selectedDigicrossCards { get; private set; } = new List<CardSource>();
    public List<AddDigivolutionCardsInfo> addDigivolutionCardInfos { get; private set; } = new List<AddDigivolutionCardsInfo>();
    public CardSource playCard { get; private set; } = null;

    public void ResetSelectDigiXrosClass()
    {
        selectedDigicrossCards = new List<CardSource>();
        addDigivolutionCardInfos = new List<AddDigivolutionCardsInfo>();
        playCard = null;
    }

    public void AddDigivolutionCardInfos(AddDigivolutionCardsInfo digivolutionCardsInfo)
    {
        addDigivolutionCardInfos.Add(digivolutionCardsInfo);
    }

    #region Max Trash Count
    int maxTrashCount(CardSource card)
    {
        int maxTrashCount = 0;

        if (card != null)
        {
            List<int> maxTrashCountList = new List<int>();

            #region 選択可能トラッシュ枚数を変更する効果
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                #region 場のパーマネントの効果
                foreach (Permanent permanent in player.GetBattleAreaPermanents())
                {
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IAddMaxTrashCountDigiXrosEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                int count = ((IAddMaxTrashCountDigiXrosEffect)cardEffect).GetMaxTrashCount(card);

                                if (count >= 1)
                                {
                                    maxTrashCountList.Add(count);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region プレイヤーの効果
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IAddMaxTrashCountDigiXrosEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            int count = ((IAddMaxTrashCountDigiXrosEffect)cardEffect).GetMaxTrashCount(card);

                            if (count >= 1)
                            {
                                maxTrashCountList.Add(count);
                            }
                        }
                    }
                }
                #endregion
            }

            #region カード自身の効果
            if (card.PermanentOfThisCard() == null)
            {
                foreach (ICardEffect cardEffect in card.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IAddMaxTrashCountDigiXrosEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            int count = ((IAddMaxTrashCountDigiXrosEffect)cardEffect).GetMaxTrashCount(card);

                            if (count >= 1)
                            {
                                maxTrashCountList.Add(count);
                            }
                        }
                    }
                }
            }
            #endregion

            #endregion

            if (maxTrashCountList.Count >= 1)
            {
                maxTrashCount = maxTrashCountList.Max();
            }

            if (maxTrashCount > card.Owner.TrashCards.Count)
            {
                maxTrashCount = card.Owner.TrashCards.Count;
            }

            if (maxTrashCount < 0)
            {
                maxTrashCount = 0;
            }
        }

        return maxTrashCount;
    }
    #endregion

    #region Max Tamer Digivolution Cards Count
    int maxTamerDigivolutionCardsCount(CardSource card)
    {
        int maxTamerDigivolutionCardsCount = 0;

        if (card != null)
        {
            List<int> maxTamerDigivolutionCardsCountList = new List<int>();

            #region 選択可能テイマー進化元枚数を変更する効果
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                #region 場のパーマネントの効果
                foreach (Permanent permanent in player.GetBattleAreaPermanents())
                {
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IAddMaxUnderTamerCountDigiXrosEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                int count = ((IAddMaxUnderTamerCountDigiXrosEffect)cardEffect).getMaxUnderTamerCount(card);

                                if (count >= 1)
                                {
                                    maxTamerDigivolutionCardsCountList.Add(count);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region プレイヤーの効果
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IAddMaxUnderTamerCountDigiXrosEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            int count = ((IAddMaxUnderTamerCountDigiXrosEffect)cardEffect).getMaxUnderTamerCount(card);

                            if (count >= 1)
                            {
                                maxTamerDigivolutionCardsCountList.Add(count);
                            }
                        }
                    }
                }
                #endregion
            }

            #region カード自身の効果
            if (card.PermanentOfThisCard() == null)
            {
                foreach (ICardEffect cardEffect in card.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IAddMaxUnderTamerCountDigiXrosEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            int count = ((IAddMaxUnderTamerCountDigiXrosEffect)cardEffect).getMaxUnderTamerCount(card);

                            if (count >= 1)
                            {
                                maxTamerDigivolutionCardsCountList.Add(count);
                            }
                        }
                    }
                }
            }
            #endregion

            #endregion

            if (maxTamerDigivolutionCardsCountList.Count >= 1)
            {
                maxTamerDigivolutionCardsCount = maxTamerDigivolutionCardsCountList.Max();
            }

            if (maxTamerDigivolutionCardsCount < 0)
            {
                maxTamerDigivolutionCardsCount = 0;
            }
        }

        return maxTamerDigivolutionCardsCount;
    }
    #endregion

    #region Is Hand Card
    bool isHandCard(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (cardSource.Owner.HandCards.Contains(cardSource))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Is Battle Area Card
    bool isBattleAreaCard(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (cardSource.PermanentOfThisCard() != null)
            {
                if (cardSource.Owner.GetBattleAreaPermanents().Contains(cardSource.PermanentOfThisCard()))
                {
                    if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Is Trash Card
    bool isTrashCard(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (CardEffectCommons.IsExistOnTrash(cardSource))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Is Security Card
    bool isSecurityCard(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (CardEffectCommons.IsExistInSecurity(cardSource, false))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region is Tamer Digivolution Card
    bool isTamerDigivolutionCard(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (cardSource.PermanentOfThisCard() != null)
            {
                if (cardSource.PermanentOfThisCard().IsTamer)
                {
                    if (cardSource.PermanentOfThisCard().DigivolutionCards.Contains(cardSource))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Can Select DigiXros
    bool CanSelectDigiXros(DigiXrosConditionElement element, CardSource targetCard, CardSource card)
    {
        if (card != targetCard)
        {
            if (card != null)
            {
                if (element != null)
                {
                    if (element.CardCondition != null)
                    {
                        if (targetCard != null)
                        {
                            if (card.digiXrosCondition != null)
                            {
                                if (!targetCard.IsToken)
                                {
                                    if (!selectedDigicrossCards.Contains(targetCard))
                                    {
                                        if (addDigivolutionCardInfos.Count((addDigivolutionCardInfo) => addDigivolutionCardInfo.cardSources.Contains(targetCard)) == 0)
                                        {
                                            if (card.digiXrosCondition.CanTargetCondition_ByPreSelecetedList != null)
                                            {
                                                if (!card.digiXrosCondition.CanTargetCondition_ByPreSelecetedList(selectedDigicrossCards, targetCard))
                                                {
                                                    return false;
                                                }
                                            }

                                            if (element.CardCondition(targetCard))
                                            {
                                                return true;
                                            }

                                            if (targetCard.PermanentOfThisCard() != null)
                                            {
                                                if (targetCard == targetCard.PermanentOfThisCard().TopCard)
                                                {
                                                    if (targetCard.PermanentOfThisCard().CanSubstituteForDigiXrosCondition(card))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Select
    public IEnumerator Select(CardSource card)
    {
        GManager.instance.turnStateMachine.isSync = true;

        selectedDigicrossCards = new List<CardSource>();

        playCard = card;

        if (card != null)
        {
            if (card.HasDigiXros)
            {
                DigiXrosCondition digiXrosCondition = card.digiXrosCondition;

                foreach (DigiXrosConditionElement element in digiXrosCondition.elements)
                {
                    yield return GManager.instance.photonWaitController.StartWait("SelectDigiXross");

                    if (selectedDigicrossCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(selectedDigicrossCards, "DigiXros Cards", false, true));
                    }

                    if (_endSelectDigiXros)
                    {
                        _endSelectDigiXros = false;
                        //break; TODO: Removed for not triggering digixros in all situations
                    }

                    bool canSelectHand = false;

                    if (card.Owner.HandCards.Count((cardSource) => CanSelectDigiXros(element, cardSource, card)) >= 1)
                    {
                        canSelectHand = true;
                    }

                    bool canSelectField = false;

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) =>  CanSelectDigiXros(element, permanent.TopCard, card)))
                    {
                        canSelectField = true;
                    }

                    bool canSelectTrash = false;

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectDigiXros(element, cardSource, card)))
                    {
                        if (selectedDigicrossCards.Count(isTrashCard) < maxTrashCount(card))
                        {
                            canSelectTrash = true;
                        }
                    }

                    bool canSelectTamer = false;

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer && permanent.DigivolutionCards.Count((cardSource) => CanSelectDigiXros(element, cardSource, card)) >= 1))
                    {
                        if (selectedDigicrossCards.Count(isTamerDigivolutionCard) < maxTamerDigivolutionCardsCount(card))
                        {
                            canSelectTamer = true;
                        }
                    }

                    Func<IEnumerator> _SelectHandCard = () => SelectHandCard(digiXrosCondition, element, card);
                    Func<IEnumerator> _SelectBattleAreaDigimon = () => SelectBattleAreaPermanent(digiXrosCondition, element, card);
                    Func<IEnumerator> _SelectTrashCard = () => SelectTrashCard(digiXrosCondition, element, card);
                    Func<IEnumerator> _SelectTamerDigivolutionCard = () => SelectTamerDigivolutionCard(digiXrosCondition, element, card);
                    Func<IEnumerator> _EndSelectDigiXros = () => EndSelectDigiXros();

                    List<Func<IEnumerator>> actions = new List<Func<IEnumerator>>() { _SelectHandCard, _SelectBattleAreaDigimon, _SelectTrashCard, _SelectTamerDigivolutionCard, _EndSelectDigiXros };

                    List<Func<IEnumerator>> canSelectActions = new List<Func<IEnumerator>>();

                    if (canSelectHand)
                    {
                        canSelectActions.Add(_SelectHandCard);
                    }

                    if (canSelectField)
                    {
                        canSelectActions.Add(_SelectBattleAreaDigimon);
                    }

                    if (canSelectTrash)
                    {
                        canSelectActions.Add(_SelectTrashCard);
                    }

                    if (canSelectTamer)
                    {
                        canSelectActions.Add(_SelectTamerDigivolutionCard);
                    }

                    canSelectActions.Add(_EndSelectDigiXros);

                    if (canSelectActions.Count == 1)
                    {
                        if (digiXrosCondition.CanTargetCondition_ByPreSelecetedList != null || element.skipAllIfNoSelect)
                        {
                            break;
                        }

                        else
                        {
                            continue;
                        }
                    }

                    else if (canSelectActions.Count == 2 && digiXrosCondition.CanTargetCondition_ByPreSelecetedList == null && !element.skipAllIfNoSelect)
                    {
                        SetTargetDigiXrossIndex(actions.IndexOf(canSelectActions[0]));
                    }

                    else
                    {
                        if (card.Owner.isYou)
                        {
                            GManager.instance.commandText.OpenCommandText($"From which area will you select {element.selectMessage}?", digiXros: true);

                            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>();

                            for (int i = 0; i < canSelectActions.Count; i++)
                            {
                                int k = actions.IndexOf(canSelectActions[i]);
                                int spriteIndex = 0;

                                string message = "";

                                switch (k)
                                {
                                    case 0:
                                        message = "Hand";
                                        break;

                                    case 1:
                                        message = "Field Digimon";
                                        break;

                                    case 2:
                                        message = "Trash";
                                        break;

                                    case 3:
                                        message = "Tamer digivolution cards";
                                        break;

                                    case 4:
                                        message = "End Selection";
                                        spriteIndex = 1;
                                        break;
                                }

                                command_SelectCommands.Add(new Command_SelectCommand(message, () => photonView.RPC("SetTargetDigiXrossIndex", RpcTarget.All, k), spriteIndex));
                            }

                            GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                        }

                        else
                        {
                            GManager.instance.commandText.OpenCommandText($"The opponent is choosing from which area to select {element.selectMessage}.", digiXros: true);

                            #region AIモード
                            if (GManager.instance.IsAI)
                            {
                                List<int> indexes = new List<int>();

                                for (int i = 0; i < canSelectActions.Count; i++)
                                {
                                    int k = actions.IndexOf(canSelectActions[i]);

                                    indexes.Add(k);
                                }

                                SetTargetDigiXrossIndex(UnityEngine.Random.Range(0, indexes.Count));
                            }
                            #endregion
                        }
                    }

                    yield return new WaitWhile(() => !_endSelect);
                    _endSelect = false;

                    GManager.instance.commandText.CloseCommandText();
                    yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                    if (0 <= _targetIndex && _targetIndex <= actions.Count - 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(actions[_targetIndex]());

                        if (!card.Owner.isYou && GManager.instance.IsAI)
                        {
                            yield return new WaitForSeconds(0.3f);
                        }
                    }
                }
            }
        }

        if (selectedDigicrossCards.Count >= 1)
        {
            yield return new WaitForSeconds(0.4f);
        }

        GManager.instance.GetComponent<Effects>().OffShowCard2();

        GManager.instance.turnStateMachine.isSync = false;
    }
    #endregion

    #region Select Hand Card

    IEnumerator SelectHandCard(DigiXrosCondition digiXrosCondition, DigiXrosConditionElement element, CardSource card)
    {
        bool CanSelectCardCondition(CardSource cardSource) => CanSelectDigiXros(element, cardSource, card);

        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
        {
            int maxCount = 1;

            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

            selectHandEffect.SetUp(
                selectPlayer: card.Owner,
                canTargetCondition: CanSelectCardCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: maxCount,
                canNoSelect: true,
                canEndNotMax: false,
                isShowOpponent: true,
                selectCardCoroutine: SelectCardCoroutine,
                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                mode: SelectHandEffect.Mode.Custom,
                cardEffect: null);

            selectHandEffect.SetUpCustomMessage($"Select {element.selectMessage}.", $"The opponent is selecting {element.selectMessage}.");
            selectHandEffect.SetUpCustomMessage_ShowCard("Selected Hand Card");
            selectHandEffect.SetDigiXros();

            yield return StartCoroutine(selectHandEffect.Activate());

            IEnumerator SelectCardCoroutine(CardSource cardSource)
            {
                selectedDigicrossCards.Add(cardSource);

                yield return null;
            }

            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
            {
                if (cardSources.Count == 0)
                {
                    if (digiXrosCondition != null)
                    {
                        if (digiXrosCondition.CanTargetCondition_ByPreSelecetedList != null || element.skipAllIfNoSelect)
                        {
                            EndSelectDigiXros();
                        }
                    }
                }

                yield return null;
            }
        }
    }
    #endregion

    #region 輯elect Battle Area Permanent
    IEnumerator SelectBattleAreaPermanent(DigiXrosCondition digiXrosCondition, DigiXrosConditionElement element, CardSource card)
    {
        bool CanSelectPermanentCondition(Permanent permanent) => permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent) && CanSelectDigiXros(element, permanent.TopCard, card);

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("", null, card);

        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
        {
            int maxCount = 1;

            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

            selectPermanentEffect.SetUp(
                selectPlayer: card.Owner,
                canTargetCondition: CanSelectPermanentCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: maxCount,
                canNoSelect: true,
                canEndNotMax: false,
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: activateClass);

            selectPermanentEffect.SetUpCustomMessage($"Select {element.selectMessage}.", $"The opponent is selecting {element.selectMessage}.");
            selectPermanentEffect.SetDigiXros();

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
                selectedDigicrossCards.Add(permanent.TopCard);

                yield return null;
            }

            IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
            {
                if (permanents.Count == 0)
                {
                    if (digiXrosCondition != null)
                    {
                        if (digiXrosCondition.CanTargetCondition_ByPreSelecetedList != null || element.skipAllIfNoSelect)
                        {
                            EndSelectDigiXros();
                        }
                    }
                }

                yield return null;
            }
        }
    }
    #endregion

    #region Select Tamer Digivolution Card
    IEnumerator SelectTamerDigivolutionCard(DigiXrosCondition digiXrosCondition, DigiXrosConditionElement element, CardSource card)
    {
        bool CanSelectCardCondition(CardSource cardSource) => CanSelectDigiXros(element, cardSource, card);

        bool CanSelectPermanentCondition(Permanent permanent) => permanent.TopCard.Owner == card.Owner && permanent.IsTamer && permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("", null, card);

        if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
        {
            Permanent selectedPermanent = null;

            int maxCount = 1;

            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

            selectPermanentEffect.SetUp(
                selectPlayer: card.Owner,
                canTargetCondition: CanSelectPermanentCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: maxCount,
                canNoSelect: true,
                canEndNotMax: false,
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: activateClass);

            selectPermanentEffect.SetUpCustomMessage($"Select a Tamer that has {element.selectMessage} in digivolution cards.", $"The opponent is selecting a Tamer that has {element.selectMessage} in digivolution cards.");
            selectPermanentEffect.SetDigiXros();

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
                selectedPermanent = permanent;

                yield return null;
            }

            IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
            {
                if (permanents.Count == 0)
                {
                    if (digiXrosCondition != null)
                    {
                        if (digiXrosCondition.CanTargetCondition_ByPreSelecetedList != null || element.skipAllIfNoSelect)
                        {
                            EndSelectDigiXros();
                        }
                    }
                }

                yield return null;
            }

            if (selectedPermanent != null)
            {
                if (selectedPermanent.DigivolutionCards.Count >= 1)
                {
                    maxCount = 1;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message: $"<color=#FF633E>DigiXros</color>: Select {element.selectMessage}.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: selectedPermanent.DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: null);

                    selectCardEffect.SetUpCustomMessage($"Select {element.selectMessage}.", $"The opponent is selecting {element.selectMessage}.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Selected Digivolution Card");
                    selectCardEffect.SetDigiXros();

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedDigicrossCards.Add(cardSource);

                        yield return null;
                    }

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count == 0)
                        {
                            if (digiXrosCondition != null)
                            {
                                if (digiXrosCondition.CanTargetCondition_ByPreSelecetedList != null || element.skipAllIfNoSelect)
                                {
                                    EndSelectDigiXros();
                                }
                            }
                        }

                        yield return null;
                    }
                }
            }
        }

        yield return null;
    }
    #endregion

    #region Select Trash Card
    IEnumerator SelectTrashCard(DigiXrosCondition digiXrosCondition, DigiXrosConditionElement element, CardSource card)
    {
        bool CanSelectCardCondition(CardSource cardSource) => CanSelectDigiXros(element, cardSource, card);

        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource)))
        {
            int maxCount = 1;

            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

            selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: $"<color=#FF633E>DigiXros</color>: Select {element.selectMessage} from trash.",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: null);

            selectCardEffect.SetUpCustomMessage($"Select {element.selectMessage}.", $"The opponent is selecting {element.selectMessage}.");
            selectCardEffect.SetUpCustomMessage_ShowCard("Selected Trash Card");
            selectCardEffect.SetDigiXros();

            yield return StartCoroutine(selectCardEffect.Activate());

            IEnumerator SelectCardCoroutine(CardSource cardSource)
            {
                selectedDigicrossCards.Add(cardSource);

                yield return null;
            }

            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
            {
                if (cardSources.Count == 0)
                {
                    if (digiXrosCondition != null)
                    {
                        if (digiXrosCondition.CanTargetCondition_ByPreSelecetedList != null || element.skipAllIfNoSelect)
                        {
                            EndSelectDigiXros();
                        }
                    }
                }

                yield return null;
            }
        }
    }
    #endregion

    #region End Select DigiXros
    IEnumerator EndSelectDigiXros()
    {
        _endSelectDigiXros = true;
        yield return null;
    }
    #endregion

    #region Add Digivolution Cards
    public IEnumerator AddDigivolutiuonCards(CardSource card)
    {
        if (selectedDigicrossCards.Count >= 1)
        {
            if (card != null)
            {
                if (card == playCard)
                {
                    if (card.PermanentOfThisCard() != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(selectedDigicrossCards, "Digixros Cards", true, true));

                        foreach (CardSource cardSource in selectedDigicrossCards)
                        {
                            if (isHandCard(cardSource))
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { cardSource }, null));
                            }

                            else if (isBattleAreaCard(cardSource))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDigiXrosSelectCardEffect(cardSource.PermanentOfThisCard()));

                                IPlacePermanentToDigivolutionCards placePermanentToDigivolutionCards = new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { cardSource.PermanentOfThisCard(), card.PermanentOfThisCard() } }, false, null);
                                placePermanentToDigivolutionCards.SetNotShowCards();
                                yield return ContinuousController.instance.StartCoroutine(placePermanentToDigivolutionCards.PlacePermanentToDigivolutionCards());
                            }

                            else if (isTrashCard(cardSource))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDigiXrosSelectCardEffect(null, player: cardSource.Owner));

                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { cardSource }, null));
                            }

                            else if (isTamerDigivolutionCard(cardSource))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(cardSource, cardSource.PermanentOfThisCard()));
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { cardSource }, null));
                            }

                            cardSource.cEntity_EffectController.InitUseCountThisTurn();
                        }
                    }
                }
            }
        }

        ResetSelectDigiXrosClass();

        yield return null;
    }
    #endregion

    #region 効果によって進化元を付与(天野ユウ(BT10),シャウトモンX7スペリオルモード(BT12))
    public IEnumerator AddDigivolutiuonCardsByEffect(CardSource card)
    {
        if (addDigivolutionCardInfos.Count >= 1)
        {
            if (card != null)
            {
                if (card.PermanentOfThisCard() != null)
                {
                    List<CardSource> addedCards = new List<CardSource>();

                    foreach (AddDigivolutionCardsInfo info in addDigivolutionCardInfos)
                    {
                        List<CardSource> underTamerCards = new List<CardSource>();
                        List<Permanent> digimonPermanents = new List<Permanent>();
                        List<CardSource> trashCards = new List<CardSource>();
                        List<CardSource> secuirtyCards = new List<CardSource>();

                        foreach (CardSource cardSource in info.cardSources)
                        {
                            if (isTamerDigivolutionCard(cardSource))
                            {
                                underTamerCards.Add(cardSource);
                                addedCards.Add(cardSource);
                            }

                            else if (isBattleAreaCard(cardSource))
                            {
                                digimonPermanents.Add(cardSource.PermanentOfThisCard());
                                addedCards.Add(cardSource);
                            }
                            else if (isTrashCard(cardSource))
                            {
                                trashCards.Add(cardSource);
                                addedCards.Add(cardSource);
                            }

                            else if (isSecurityCard(cardSource))
                            {
                                secuirtyCards.Add(cardSource);
                                addedCards.Add(cardSource);
                            }
                        }

                        if (underTamerCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(underTamerCards, info.cardEffect));
                        }

                        if (digimonPermanents.Count >= 1)
                        {
                            foreach (Permanent digimonPermanent in digimonPermanents)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { digimonPermanent, card.PermanentOfThisCard() } }, false, info.cardEffect).PlacePermanentToDigivolutionCards());
                            }
                        }

                        if (trashCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(trashCards, info.cardEffect));
                        }

                        if (secuirtyCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(secuirtyCards, info.cardEffect));
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(addedCards, "Digivolution Cards", true, true));
                }
            }
        }

        addDigivolutionCardInfos = new List<AddDigivolutionCardsInfo>();

        yield return null;
    }
    #endregion

    int _targetIndex = 0;
    bool _endSelect = false;

    bool _endSelectDigiXros = false;

    [PunRPC]
    public void SetTargetDigiXrossIndex(int targetIndex)
    {
        this._targetIndex = targetIndex;
        _endSelect = true;
    }
}

public class AddDigivolutionCardsInfo
{
    public AddDigivolutionCardsInfo(ICardEffect cardEffect, List<CardSource> cardSources)
    {
        this.cardEffect = cardEffect;
        this.cardSources = new List<CardSource>();

        foreach (CardSource cardSource in cardSources)
        {
            this.cardSources.Add(cardSource);
        }
    }

    public ICardEffect cardEffect { get; private set; } = null;
    public List<CardSource> cardSources { get; private set; } = new List<CardSource>();
}
