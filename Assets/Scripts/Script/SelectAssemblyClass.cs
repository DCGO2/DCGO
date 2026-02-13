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
                                            if (element.CanTargetCondition_ByPreSelecetedList != null)
                                            {
                                                if (!element.CanTargetCondition_ByPreSelecetedList(selectedAssemblyCards, targetCard))
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

    #region CanFulfillConditions
    bool CanFulfillConditions(CardSource card)
    {
        if (card != null)
        {
            if (card.HasAssembly)
            {
                AssemblyCondition AssemblyCondition = card.assemblyCondition;

                if (AssemblyCondition.elements != null)
                {
                    if (AssemblyCondition.elements.Count == 1)
                        return CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, (cardSource) => CanSelectAssembly(AssemblyCondition.elements[0], cardSource, card)) >= AssemblyCondition.elementCount;
                    else
                        return CanFulfillEachElementCondition(card, AssemblyCondition);
                }
            }
        }
        return false;
    }

   bool CanFulfillEachElementCondition(CardSource card, AssemblyCondition AssemblyCondition, int index = 0, List<CardSource> usedCards = null, int currentElementCount = 0)
    {
        if (index < 0 || AssemblyCondition.elements == null)
            return false;
        if (index >= AssemblyCondition.elements.Count)
            return true;
            
        usedCards ??= new List<CardSource>();

        AssemblyConditionElement currentElement = AssemblyCondition.elements[index];

        List<CardSource> validCards = card.Owner.TrashCards.Filter(currentElement.CardCondition).Except(usedCards).ToList();

        foreach(CardSource validCard in validCards)
        {  
            List<CardSource> addedSoFar = new List<CardSource>() { validCard };
            addedSoFar.AddRange(usedCards);
            if (currentElementCount + 1 >= currentElement.ElementCount)
            {
                if(CanFulfillEachElementCondition(card, AssemblyCondition, index+1, addedSoFar, 0))
                    return true;
            } 
            else
            {
                if (CanFulfillEachElementCondition(card, AssemblyCondition, index, addedSoFar, currentElementCount+1))
                    return true;
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

        if (CanFulfillConditions(card))
        {
            AssemblyCondition AssemblyCondition = card.assemblyCondition;

            foreach(AssemblyConditionElement element in AssemblyCondition.elements)
            {
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

                if (CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, (cardSource) => CanSelectAssembly(element, cardSource, card)) >= element.ElementCount)
                {
                    canSelectTrash = true;
                }


                if (canSelectTrash)
                {
                    yield return ContinuousController.instance.StartCoroutine(SelectTrashCard(element, card));
                }
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
    IEnumerator SelectTrashCard(AssemblyConditionElement AssemblyConditionElement, CardSource card)
    {
        bool CanSelectCardCondition(CardSource cardSource) => CanSelectAssembly(AssemblyConditionElement, cardSource, card);

        if (CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition) >= AssemblyConditionElement.ElementCount)
        {
            int maxCount = AssemblyConditionElement.ElementCount;

            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

            selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: AssemblyConditionElement.CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: $"<color=#FF633E>Assembly</color>: Select {AssemblyConditionElement.selectMessage} from trash.",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: card.Owner.TrashCards.Except(selectedAssemblyCards).ToList(),//don't include cards already chosen
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: null);

            selectCardEffect.SetUpCustomMessage($"Select {AssemblyConditionElement.selectMessage}.", $"The opponent is selecting {AssemblyConditionElement.selectMessage}.");
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
                    if (AssemblyConditionElement != null)
                    {
                        if (AssemblyConditionElement.CanTargetCondition_ByPreSelecetedList != null || AssemblyConditionElement.skipAllIfNoSelect)
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
