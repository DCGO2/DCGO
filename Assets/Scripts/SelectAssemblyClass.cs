using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using Photon.Pun;
using System;
using UnityEngine.Events;

public class SelectAssemblyClass : MonoBehaviourPunCallbacks
{
    public List<CardSource> selectedAssemblyCards { get; private set; } = new List<CardSource>();
    public List<AddDigivolutionCardsInfo> addDigivolutionCardInfos { get; private set; } = new List<AddDigivolutionCardsInfo>();
    public CardSource playCard { get; private set; } = null;

    public void ResetSelectAssemblyClass()
    {
        selectedAssemblyCards = new List<CardSource>();
        addDigivolutionCardInfos = new List<AddDigivolutionCardsInfo>();
        playCard = null;
    }

    public void AddDigivolutionCardInfos(AddDigivolutionCardsInfo digivolutionCardsInfo)
    {
        addDigivolutionCardInfos.Add(digivolutionCardsInfo);
    }

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

    #region Can Select Assembly
    bool CanSelectAssembly(AssemblyConditionElement element, CardSource targetCard, CardSource card)
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
                            if (card.assemblyCondition != null)
                            {
                                if (!targetCard.IsToken)
                                {
                                    if (!selectedAssemblyCards.Contains(targetCard))
                                    {
                                        if (addDigivolutionCardInfos.Count((addDigivolutionCardInfo) => addDigivolutionCardInfo.cardSources.Contains(targetCard)) == 0)
                                        {
                                            if (card.assemblyCondition.CanTargetCondition_ByPreSelecetedList != null)
                                            {
                                                if (!card.assemblyCondition.CanTargetCondition_ByPreSelecetedList(selectedAssemblyCards, targetCard))
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
                                                    if (targetCard.PermanentOfThisCard().CanSubstituteForAssemblyCondition(card))
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

        selectedAssemblyCards = new List<CardSource>();

        playCard = card;

        if (card != null)
        {
            if (card.HasAssembly)
            {
                AssemblyCondition AssemblyCondition = card.assemblyCondition;

                yield return GManager.instance.photonWaitController.StartWait("SelectAssemblys");

                if (selectedAssemblyCards.Count >= AssemblyCondition.elementCount)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(selectedAssemblyCards, "Assembly Cards", false, true));
                }

                if (_endSelectAssembly)
                {
                    _endSelectAssembly = false;
                    //break; TODO: Removed for not triggering Assembly in all situations
                }

                bool canSelectTrash = false;

                if (CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, (cardSource) => CanSelectAssembly(AssemblyCondition.element, cardSource, card)) >= AssemblyCondition.elementCount)
                {
                    canSelectTrash = true;
                }


                if (canSelectTrash)
                {
                    yield return ContinuousController.instance.StartCoroutine(SelectTrashCard(AssemblyCondition, card));
                }

                /*foreach (AssemblyConditionElement element in AssemblyCondition.elements)
                {
                    yield return GManager.instance.photonWaitController.StartWait("SelectAssemblys");

                    if (selectedAssemblyCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(selectedAssemblyCards, "Assembly Cards", false, true));
                    }

                    if (_endSelectAssembly)
                    {
                        _endSelectAssembly = false;
                        //break; TODO: Removed for not triggering Assembly in all situations
                    }

                    bool canSelectTrash = false;

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectAssembly(element, cardSource, card)))
                    {
                        canSelectTrash = true;
                    }

                    Func<IEnumerator> _SelectTrashCard = () => SelectTrashCard(AssemblyCondition, element, card);
                    Func<IEnumerator> _EndSelectAssembly = () => EndSelectAssembly();

                    List<Func<IEnumerator>> actions = new List<Func<IEnumerator>>() { _SelectTrashCard, _EndSelectAssembly };

                    List<Func<IEnumerator>> canSelectActions = new List<Func<IEnumerator>>();

                    if (canSelectTrash)
                    {
                        canSelectActions.Add(_SelectTrashCard);
                    }

                    canSelectActions.Add(_EndSelectAssembly);

                    if (canSelectActions.Count == 1)
                    {
                        if (AssemblyCondition.CanTargetCondition_ByPreSelecetedList != null || element.skipAllIfNoSelect)
                        {
                            break;
                        }

                        else
                        {
                            continue;
                        }
                    }

                    else if (canSelectActions.Count == 2 && AssemblyCondition.CanTargetCondition_ByPreSelecetedList == null && !element.skipAllIfNoSelect)
                    {
                        SetTargetAssemblysIndex(actions.IndexOf(canSelectActions[0]));
                    }

                    else
                    {
                        if (card.Owner.isYou)
                        {
                            GManager.instance.commandText.OpenCommandText($"From which area will you select {element.selectMessage}?", assembly: true);

                            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>();

                            for (int i = 0; i < canSelectActions.Count; i++)
                            {
                                int k = actions.IndexOf(canSelectActions[i]);
                                int spriteIndex = 0;

                                string message = "";

                                switch (k)
                                {
                                    case 0:
                                        message = "Trash";
                                        break;

                                    case 1:
                                        message = "End Selection";
                                        spriteIndex = 1;
                                        break;
                                }

                                command_SelectCommands.Add(new Command_SelectCommand(message, () => photonView.RPC("SetTargetAssemblysIndex", RpcTarget.All, k), spriteIndex));
                            }

                            GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                        }

                        else
                        {
                            GManager.instance.commandText.OpenCommandText($"The opponent is choosing from which area to select {element.selectMessage}.", assembly: true);

                            #region AI
                            if (GManager.instance.IsAI)
                            {
                                List<int> indexes = new List<int>();

                                for (int i = 0; i < canSelectActions.Count; i++)
                                {
                                    int k = actions.IndexOf(canSelectActions[i]);

                                    indexes.Add(k);
                                }

                                SetTargetAssemblysIndex(UnityEngine.Random.Range(0, indexes.Count));
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
                }*/
            }
        }

        if (selectedAssemblyCards.Count >= 1)
        {
            yield return new WaitForSeconds(0.4f);
        }

        GManager.instance.GetComponent<Effects>().OffShowCard2();

        GManager.instance.turnStateMachine.isSync = false;
    }
    #endregion

    #region Select Trash Card
    IEnumerator SelectTrashCard(AssemblyCondition AssemblyCondition, CardSource card)
    {
        bool CanSelectCardCondition(CardSource cardSource) => CanSelectAssembly(AssemblyCondition.element, cardSource, card);

        if (CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition) >= AssemblyCondition.elementCount)
        {
            int maxCount = AssemblyCondition.elementCount;

            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

            selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: AssemblyCondition.CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: $"<color=#FF633E>Assembly</color>: Select {AssemblyCondition.selectMessage} from trash.",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: null);

            selectCardEffect.SetUpCustomMessage($"Select {AssemblyCondition.selectMessage}.", $"The opponent is selecting {AssemblyCondition.selectMessage}.");
            selectCardEffect.SetUpCustomMessage_ShowCard("Selected Trash Card");
            selectCardEffect.SetAssembly();

            yield return StartCoroutine(selectCardEffect.Activate());

            IEnumerator SelectCardCoroutine(CardSource cardSource)
            {
                selectedAssemblyCards.Add(cardSource);

                yield return null;
            }

            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
            {
                if (cardSources.Count == 0)
                {
                    if (AssemblyCondition != null)
                    {
                        if (AssemblyCondition.CanTargetCondition_ByPreSelecetedList != null || AssemblyCondition.element.skipAllIfNoSelect)
                        {
                            EndSelectAssembly();
                        }
                    }
                }

                yield return null;
            }
        }
    }
    #endregion

    #region End Select Assembly
    IEnumerator EndSelectAssembly()
    {
        _endSelectAssembly = true;
        yield return null;
    }
    #endregion

    #region Add Digivolution Cards
    public IEnumerator AddDigivolutiuonCards(CardSource card)
    {
        if (card != null)
        {
            if (card == playCard)
            {
                if (card.PermanentOfThisCard() != null)
                {
                    if (selectedAssemblyCards.Count == playCard.assemblyCondition.elementCount)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(selectedAssemblyCards, "Assembly Cards", true, true));

                        foreach (CardSource cardSource in selectedAssemblyCards)
                        {
                            if (isTrashCard(cardSource))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateAssemblySelectCardEffect(null, player: cardSource.Owner));

                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { cardSource }, null));
                            }
                        }
                    }
                }
            }
        }

        ResetSelectAssemblyClass();

        yield return null;
    }
    #endregion

    #region Add Digivolution Card
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

                        foreach (CardSource cardSource in info.cardSources)
                        {
                            if (isTrashCard(cardSource))
                            {
                                trashCards.Add(cardSource);
                                addedCards.Add(cardSource);
                            }
                        }

                        if (trashCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(trashCards, info.cardEffect));
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

    bool _endSelectAssembly = false;

    [PunRPC]
    public void SetTargetAssemblysIndex(int targetIndex)
    {
        this._targetIndex = targetIndex;
        _endSelect = true;
    }
}
