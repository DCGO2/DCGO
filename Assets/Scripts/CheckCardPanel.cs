using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;
using Photon.Pun;

public class CheckCardPanel : MonoBehaviour
{
    [Header("Message Text")]
    public TextMeshProUGUI MessageText;

    [Header("ScrollRect")]
    public ScrollRect scrollRect;

    [Header("Background")]
    public GameObject BackGround;

    [Header("Parent except background")]
    public GameObject Parent;

    public List<HandCard> handCards
    {
        get
        {
            List<HandCard> handCards = new List<HandCard>();

            if (GManager.instance.turnStateMachine.DoneStartGame)
            {
                for (int i = 0; i < scrollRect.content.childCount; i++)
                {
                    if (scrollRect.content.GetChild(i) != null)
                    {
                        if (scrollRect.content.GetChild(i).gameObject != null)
                        {
                            if (scrollRect.content.GetChild(i).GetComponent<HandCard>() != null)
                            {
                                handCards.Add(scrollRect.content.GetChild(i).GetComponent<HandCard>());
                            }
                        }
                    }
                }
            }

            return handCards;
        }
    }

    #region Check trash cards
    public void OnClickCheckTrashButton(bool IsYou)
    {
        string Message = "";

        Player player = null;

        if (IsYou)
        {
            player = GManager.instance.You;
            Message = $"Your trash({player.TrashCards.Count}card{Utils.PluralFormSuffix(player.TrashCards.Count)})";
        }

        else
        {
            player = GManager.instance.Opponent;
            Message = $"The opponent's trash({player.TrashCards.Count}card{Utils.PluralFormSuffix(player.TrashCards.Count)})";
        }

        OpenSelectCardPanel(Message, player.TrashCards, true, null, SelectCardEffect.Root.Trash, IsYou);
    }
    #endregion

    #region Check security cards
    public void OnClickCheckSideButton(bool IsYou)
    {
        string Message = "";

        Player player = null;

        if (IsYou)
        {
            player = GManager.instance.You;
            Message = $"Your security({player.SecurityCards.Count}card{Utils.PluralFormSuffix(player.SecurityCards.Count)})";
        }

        else
        {
            player = GManager.instance.Opponent;
            Message = $"The opponent's security({player.SecurityCards.Count}card{Utils.PluralFormSuffix(player.SecurityCards.Count)})";
        }

        OpenSelectCardPanel(Message, player.SecurityCards, false, null, SelectCardEffect.Root.Security, IsYou);
    }
    #endregion

    public void OpenSelectCardPanel(string Message, List<CardSource> RootCardSources, bool CanLookReverseCard, Func<UnityAction<HandCard>> OnClickAction, SelectCardEffect.Root root, bool IsYou)
    {
        #region Initialization
        this.gameObject.SetActive(true);

        BackGround.SetActive(true);

        Parent.SetActive(false);

        MessageText.text = Message;
        #endregion

        StartCoroutine(OpenSelectCardPanelCoroutine(RootCardSources, CanLookReverseCard, OnClickAction, root, IsYou));
    }

    IEnumerator OpenSelectCardPanelAnimation(SelectCardEffect.Root root, bool IsYou)
    {
        bool end = false;
        var sequence = DOTween.Sequence();
        float time = 0.4f;

        this.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

        if (root == SelectCardEffect.Root.Security)
        {
            if (IsYou)
            {
                this.transform.localPosition = new Vector3(-580f, -110f, 0f);
            }

            else
            {
                this.transform.localPosition = new Vector3(-580f, 165f, 0f);
            }
        }

        else if (root == SelectCardEffect.Root.Trash || root == SelectCardEffect.Root.DigivolutionCards)
        {
            if (IsYou)
            {
                this.transform.localPosition = new Vector3(-580f, -280f, 0f);
            }

            else
            {
                this.transform.localPosition = new Vector3(-580f, 340f, 0f);
            }
        }

        else
        {
            this.transform.localPosition = new Vector3(0f, 0f, 0f);
        }

        sequence
            .Append(this.transform.DOLocalMove(new Vector3(0f, 0f, 0f), time))
            .Join(this.transform.DOScale(new Vector3(1f, 1f, 1f), time))
            .AppendCallback(() => { end = true; });

        sequence.Play();

        yield return new WaitWhile(() => !end);
        end = false;
    }

    IEnumerator OpenSelectCardPanelCoroutine(List<CardSource> RootCardSources, bool CanLookReverseCard, Func<UnityAction<HandCard>> OnClickAction, SelectCardEffect.Root root1, bool IsYou)
    {
        yield return StartCoroutine(OpenSelectCardPanelAnimation(root1, IsYou));

        List<CardSource> root = new List<CardSource>();

        foreach (CardSource cardSource in RootCardSources)
        {
            root.Add(cardSource);
        }

        yield return new WaitForSeconds(Time.deltaTime);

        #region Initialize card list
        if (scrollRect.content.childCount > 0)
        {
            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                if (scrollRect.content.GetChild(i) != null)
                {
                    if (scrollRect.content.GetChild(i).gameObject != null)
                    {
                        Destroy(scrollRect.content.GetChild(i).gameObject);
                    }
                }
            }

            yield return new WaitWhile(() => scrollRect.content.childCount > 0);
        }
        #endregion

        #region card generation
        foreach (CardSource cardSource in root)
        {
            HandCard handCard = Instantiate(GManager.instance.handCardPrefab, scrollRect.content);
            handCard.gameObject.name = $"checkCardPanel_{cardSource.Owner.PlayerName}";

            handCard.GetComponent<Draggable_HandCard>().startScale = new Vector3(2.7f, 2.7f, 1);

            handCard.GetComponent<Draggable_HandCard>().DefaultY = -292;

            EventTrigger eventTrigger = handCard.CardImage.GetComponent<EventTrigger>();

            eventTrigger.triggers.Clear();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((x) => { PointerClick(cardSource); });

            #region Processing on click
            void PointerClick(CardSource cardSource1)
            {
                #region right click
                if (Input.GetMouseButtonUp(1))
                {
                    if (cardSource1 != null)
                    {
                        //If you can't see the face down card
                        if (!CanLookReverseCard)
                        {
                            if (!cardSource1.IsFlipped)
                            {
                                GManager.instance.cardDetail.OpenCardDetail(cardSource1, true);

                                if (GManager.instance != null)
                                {
                                    GManager.instance.PlayDecisionSE();
                                }
                            }
                        }

                        //If you can see the card face down
                        else
                        {
                            GManager.instance.cardDetail.OpenCardDetail(cardSource1, true);

                            GManager.instance.PlayDecisionSE();
                        }
                    }
                }
                #endregion

                #region left click
                else if (Input.GetMouseButtonUp(0))
                {
                    handCard.OnClickAction?.Invoke(handCard);
                }
                #endregion
            }
            #endregion

            eventTrigger.triggers.Add(entry);

            handCard.SetUpHandCard(cardSource, showCardImage: false);

            if (!cardSource.IsFlipped)
            {
                handCard.SetUpHandCardImage();
            }

            else
            {
                handCard.SetUpReverseCard();
            }
        }

        yield return new WaitWhile(() => scrollRect.content.childCount < root.Count);

        yield return new WaitForSeconds(Time.deltaTime * 2);

        for (int i = 0; i < scrollRect.content.childCount; i++)
        {
            scrollRect.content.GetChild(i).localScale = new Vector3(2.7f, 2.7f, 1);
        }
        #endregion

        Parent.SetActive(true);

        scrollRect.horizontalNormalizedPosition = 0;

        yield return new WaitForSeconds(Time.deltaTime * 0.2f);

        if (GManager.instance.turnStateMachine.gameContext.TurnPlayer.isYou)
        {
            foreach (HandCard handCard in handCards)
            {
                if (handCard.cardSource.Owner != GManager.instance.turnStateMachine.gameContext.TurnPlayer)
                    continue;

                if (handCard.cardSource.CanDeclareSkill)
                {
                    handCard.SetOrangeOutline();

                    handCard.AddClickTarget(OnClickHandCard);
                }
            }
        }        
    }

    public void OnClickHandCard(HandCard handCard)
    {
        if (handCard != null)
        {
            if (handCard.cardSource != null)
            {
                List<CardCommand> FieldUnitCommands = new List<CardCommand>();

                #region Card Effects
                List<ICardEffect> cardEffects = new List<ICardEffect>();
                List<ICardEffect> cardEffects1 = new List<ICardEffect>();

                foreach (ICardEffect cardEffect in handCard.cardSource.EffectList(EffectTiming.OnDeclaration))
                {
                    cardEffects1.Add(cardEffect);
                    cardEffects.Add(cardEffect);
                }

                cardEffects.Reverse();

                foreach (ICardEffect cardEffect in cardEffects)
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        CardCommand SkillCommand = new CardCommand(cardEffect.EffectName, OnClick_SetUseSkillUnit_RPC, cardEffect.CanUse(null), DataBase.CommandColor_Skill);
                        FieldUnitCommands.Add(SkillCommand);

                        void OnClick_SetUseSkillUnit_RPC()
                        {
                            #region Reset the card on the field
                            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                            {
                                foreach (FieldPermanentCard fieldPermanentCard in player.FieldPermanentObjects)
                                {
                                    fieldPermanentCard.RemoveSelectEffect();
                                    fieldPermanentCard.RemoveClickTarget();
                                    fieldPermanentCard.RemoveDragTarget();
                                    fieldPermanentCard.Outline_Select.gameObject.SetActive(false);
                                    fieldPermanentCard.CloseCommandPanel();
                                }
                            }
                            #endregion

                            #region Reset the card in hand
                            foreach (HandCard handCard1 in GManager.instance.turnStateMachine.gameContext.TurnPlayer.HandCardObjects)
                            {
                                handCard.Outline_Select.gameObject.SetActive(false);
                                handCard1.RemoveSelectEffect();
                                handCard1.RemoveClickTarget();
                                handCard1.RemoveDragTarget();
                            }
                            #endregion

                            #region Reset the card in trash
                            foreach (HandCard trashCard in handCards)
                            {
                                trashCard.Outline_Select.gameObject.SetActive(false);
                                trashCard.RemoveSelectEffect();
                                trashCard.RemoveClickTarget();
                                trashCard.RemoveDragTarget();
                            }
                            #endregion

                            handCard.GetComponent<Draggable_HandCard>().CanPointerEnterExitAction = true;

                            GManager.instance.turnStateMachine.photonView.RPC("SetActCardSkill", RpcTarget.All, handCard.cardSource.CardIndex, cardEffects1.IndexOf(cardEffect));

                            CloseSelectCardPanel();
                        }
                    }
                }
                #endregion

                handCard.handCardCommandPanel.SetUpCommandPanel(FieldUnitCommands, null, handCard);

                handCard.AddClickTarget((_fieldUnitCard) => StartCoroutine(GManager.instance.turnStateMachine.SetMainPhase()));

                handCard.Outline_Select.gameObject.SetActive(true);
                handCard.SetOrangeOutline();
            }
        }
    }

    bool CanClose = true;
    public void CloseSelectCardPanel()
    {
        if (!CanClose)
            return;

        CanClose = false;
        ContinuousController.instance.StartCoroutine(CloseSelectCardPanelCoroutine());
    }

    bool first = false;
    public IEnumerator CloseSelectCardPanelCoroutine()
    {
        if (first)
        {
            if (Opening.instance != null)
            {
                Opening.instance.PlayCancelSE();
            }
        }

        first = true;

        this.gameObject.SetActive(false);

        if (scrollRect.content.childCount > 0)
        {
            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                if (scrollRect.content.GetChild(i) != null)
                {
                    if (scrollRect.content.GetChild(i).gameObject != null)
                    {
                        Destroy(scrollRect.content.GetChild(i).gameObject);
                    }
                }
            }

            yield return new WaitWhile(() => scrollRect.content.childCount > 0);
        }

        CanClose = true;
    }
}
