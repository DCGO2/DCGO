using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

//Card-related operations
public class CardObjectController : MonoBehaviour
{
    #region Generate cards for each player's deck
    public static void CreatePlayerDecks(CardSource CardPrefab, GameContext gameContext, DeckData youDeck, DeckData opponentDeck)
    {
        #region カードを生成

        void AddCards(Player player, List<CardSource> deckLocation, List<CEntity_Base> Cards)
        {
            foreach (var card in Cards)
            {
                deckLocation.Add(CreateCardSource(player.PlayerID, card, false));
            }
        }

        GManager.instance.CardIndex = 0;

        DeckData player0 = gameContext.You.PlayerID == 0 ? youDeck : opponentDeck;
        DeckData player1 = gameContext.Opponent.PlayerID == 1 ? opponentDeck : youDeck;


        AddCards(gameContext.PlayerFromID(0), gameContext.PlayerFromID(0).LibraryCards, RandomUtility.ShuffledDeckCards(player0.DeckCards()));
        AddCards(gameContext.PlayerFromID(0), gameContext.PlayerFromID(0).DigitamaLibraryCards, RandomUtility.ShuffledDeckCards(player0.DigitamaDeckCards()));
        AddCards(gameContext.PlayerFromID(1), gameContext.PlayerFromID(1).LibraryCards, RandomUtility.ShuffledDeckCards(player1.DeckCards()));
        AddCards(gameContext.PlayerFromID(1), gameContext.PlayerFromID(1).DigitamaLibraryCards, RandomUtility.ShuffledDeckCards(player1.DigitamaDeckCards()));

        #endregion
    }
    #endregion

    #region Decks for AI match
    public static void GetAIDecks(out DeckData youDeck, out DeckData aiDeck)
    {
        List<DeckData> validDecks = new List<DeckData>();

        foreach (DeckData deckData in ContinuousController.instance.DeckDatas)
        {
            if (deckData.IsValidDeckData())
            {
                validDecks.Add(deckData);
            }
        }

        if (validDecks.Count > 0)
        {
            DeckData randomDeck = validDecks[UnityEngine.Random.Range(0, validDecks.Count)];

#if UNITY_EDITOR && !UNITY_WINDOWS
            randomDeck = validDecks[0];
#endif

            aiDeck = new DeckData(randomDeck.GetThisDeckCode(), randomDeck.DeckID);
        }
        else
        {
            aiDeck = CreateRandomCardsDeck();
        }

        DeckData battleDeck = ContinuousController.instance.BattleDeckData;
        youDeck = aiDeck;

        if (battleDeck == null && validDecks.Count > 0)
        {
            youDeck = validDecks[0];
        }
        else if (battleDeck != null && battleDeck.IsValidDeckData())
        {
            youDeck = battleDeck;
        }
    }

    static DeckData CreateRandomCardsDeck()
    {
        void AddRandomCards(List<CEntity_Base> deck, List<CEntity_Base> candidates, int count)
        {
            if (candidates.Count == 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                deck.Add(candidates[UnityEngine.Random.Range(0, candidates.Count)]);
            }
        }

        List<CEntity_Base> mainDeckCandidates = new List<CEntity_Base>();
        List<CEntity_Base> digitamaDeckCandidates = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in ContinuousController.instance.CardList)
        {
            if (cEntity_Base.cardKind.Contains(CardKind.DigiEgg))
            {
                digitamaDeckCandidates.Add(cEntity_Base);
            }
            else
            {
                mainDeckCandidates.Add(cEntity_Base);
            }
        }

        List<CEntity_Base> mainDeckCards = new List<CEntity_Base>();
        List<CEntity_Base> digitamaDeckCards = new List<CEntity_Base>();

        AddRandomCards(mainDeckCards, mainDeckCandidates, 50);
        AddRandomCards(digitamaDeckCards, digitamaDeckCandidates, 5);

        return new DeckData(DeckData.GetDeckCode("サンプルデッキ", mainDeckCards, digitamaDeckCards, null));
    }
    #endregion

    #region generate card
    public static CardSource CreateCardSource(int _PlayerID, CEntity_Base cEntity_Base, bool isToken)
    {
        Player player = GManager.instance.turnStateMachine.gameContext.PlayerFromID(_PlayerID);

        CardSource cardSource = Instantiate(GManager.instance.CardPrefab, player.CardSorcesParent);

        cardSource.SetBaseData(cEntity_Base, player);

        cardSource.cEntity_EffectController.AddCardEffect(cEntity_Base.CardID, cEntity_Base.CardEffectClassName);

        cardSource.SetUpCardIndex(GManager.instance.CardIndex);

        cardSource.SetIsToken(isToken);

        GManager.instance.turnStateMachine.gameContext.ActiveCardList.Add(cardSource);

        GManager.instance.CardIndex++;

        return cardSource;
    }
    #endregion

    #region Remove cards from all areas
    public static IEnumerator RemoveFromAllArea(CardSource cardSource)
    {
        //TODO: This might not even be necessary at all? - MB
        if(!cardSource.IsFlipped || cardSource.IsBeingRevealed || GManager.instance.turnStateMachine.gameContext.IsSecurityLooking)
            cardSource.SetFace();

        if (cardSource.Owner.HandCards.Contains(cardSource))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().DeleteHandCardEffectCoroutine(cardSource));
        }

        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            List<Permanent> permanents = new List<Permanent>();

            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                permanents.Add(permanent);
            }

            foreach (Permanent permanent in permanents)
            {
                if (permanent.cardSources.Contains(cardSource))
                {
                    if (permanent.LinkedCards.Contains(cardSource))
                    {
                        yield return ContinuousController.instance.StartCoroutine(permanent.RemoveLinkedCard(cardSource, trashCard: false));
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(permanent.RemoveCardSource(cardSource));
                    }
                }
            }
        }

        //手札から取り除く
        while (cardSource.Owner.HandCards.Contains(cardSource))
        {
            cardSource.Owner.HandCards.Remove(cardSource);
        }

        //デッキから取り除く
        while (cardSource.Owner.LibraryCards.Contains(cardSource))
        {
            cardSource.Owner.LibraryCards.Remove(cardSource);
        }

        //デジタマデッキから取り除く
        while (cardSource.Owner.DigitamaLibraryCards.Contains(cardSource))
        {
            cardSource.Owner.DigitamaLibraryCards.Remove(cardSource);
        }

        //トラッシュから取り除く
        while (CardEffectCommons.IsExistOnTrash(cardSource))
        {
            cardSource.Owner.TrashCards.Remove(cardSource);
        }

        //思い出から取り除く
        while (cardSource.Owner.LostCards.Contains(cardSource))
        {
            cardSource.Owner.LostCards.Remove(cardSource);
        }

        //ライフから取り除く
        while (cardSource.Owner.SecurityCards.Contains(cardSource))
        {
            cardSource.Owner.SecurityCards.Remove(cardSource);
        }

        //処理領域から取り除く
        while (cardSource.Owner.ExecutingCards.Contains(cardSource))
        {
            cardSource.Owner.ExecutingCards.Remove(cardSource);
        }
    }
    #endregion

    #region 手札のカードを整列する
    public static void AlignHand(Player player)
    {
        int n = player.HandTransform.childCount;
        float scale = player.HandTransform.transform.localScale.x;

        if (n > 0)
        {
            float CardWidth = player.HandTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;

            //If it fits within the area as it is.
            if (n * scale * CardWidth < player.HandWidth)
            {
                player.HandTransform.GetComponent<GridLayoutGroup>().spacing = new Vector2(0, 0);
                //player.HandTransform.GetComponent<GridLayoutGroup>().spacing = new Vector2(7, 0);
            }

            //If it sticks out as it is
            else
            {
                float delta = (n * scale * CardWidth - player.HandWidth) / ((n - 1) * scale);

                player.HandTransform.GetComponent<GridLayoutGroup>().spacing = new Vector2(-delta, 0);
            }
        }
    }
    #endregion

    #region 新しいパーマネントを生成する
    public static IEnumerator CreateNewPermanent(Permanent permanent, int frameID)
    {
        if (permanent.TopCard != null)
        {
            if (permanent.TopCard.Owner != null)
            {
                yield return ContinuousController.instance.StartCoroutine(RemoveFromAllArea(permanent.TopCard));

                if (0 <= frameID && frameID <= permanent.TopCard.Owner.FieldPermanents.Length)
                {
                    permanent.TopCard.Owner.FieldPermanents[frameID] = permanent;

                    permanent.TopCard.Owner.fieldCardFrames[frameID].SetFramePermanent(permanent);

                    permanent.TopCard.SetFace();

                    FieldPermanentCard fieldPermanentCard = Instantiate(GManager.instance.fieldCardPrefab, permanent.TopCard.Owner.PermanentTransform);

                    fieldPermanentCard.SetPermanentData(permanent, false);

                    fieldPermanentCard.StartScale = fieldPermanentCard.transform.localScale;

                    permanent.ShowingPermanentCard = fieldPermanentCard;

                    fieldPermanentCard.transform.localPosition = permanent.TopCard.Owner.fieldCardFrames[frameID].GetLocalCanvasPosition();

                    fieldPermanentCard.gameObject.SetActive(false);
                }
            }
        }
    }
    #endregion

    #region leave the place
    public static IEnumerator RemoveField(Permanent permanent, bool ignoreOverflow = false)
    {
        if (permanent == null) yield break;
        if (permanent.TopCard == null) yield break;

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
            CardEffectCommons.WhenPermanentWouldRemoveFieldCheckHashtable(
                new List<Permanent> { permanent },
                null,
                null
            ),
            EffectTiming.OnRemovedField));

        if (!ignoreOverflow)
        {
            yield return ContinuousController.instance.StartCoroutine(new AceOverflowClass(permanent.cardSources).Overflow());
        }

        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            while (player.GetFieldPermanents().Contains(permanent))
            {
                for (int i = 0; i < permanent.TopCard.Owner.FieldPermanents.Length; i++)
                {
                    if (permanent.TopCard.Owner.FieldPermanents[i] == permanent)
                    {
                        permanent.TopCard.Owner.FieldPermanents[i] = null;
                        permanent.TopCard.Owner.fieldCardFrames[i].SetFramePermanent(null);
                    }
                }
            }
        }

        if (permanent.TopCard != null)
        {
            foreach (CardSource cardSource in permanent.cardSources)
            {
                cardSource.Init();
            }

            permanent.cardSources = new List<CardSource>();
        }
    }
    #endregion

    #region add card to hand
    public static IEnumerator AddHandCards(List<CardSource> cardSources, bool isDraw, ICardEffect cardEffect)
    {
        if (cardSources.Count == 0) yield break;

        bool isFromTrash = cardSources.Some(cardSource => CardEffectCommons.IsExistOnTrash(cardSource));

        cardSources = cardSources.Filter(cardSource => cardSource != null && !cardSource.Owner.HandCards.Contains(cardSource));

        if (isFromTrash)
        {
            #region "Effect of "When a card is returned from the trash to the hand"

            #region Hashtable Setting
            System.Collections.Hashtable hashtable = new System.Collections.Hashtable()
            {
                {"CardSources", cardSources}
            };
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnReturnCardsToHandFromTrash));
            #endregion
        }

        foreach (CardSource cardSource in cardSources)
        {
            if (cardSource.IsToken)
            {
                yield return ContinuousController.instance.StartCoroutine(RemoveFromAllArea(cardSource));
            }
        }

        List<CardSource> eggCards = cardSources.Filter(cardSource => cardSource.IsDigiEgg);
        List<CardSource> addedCards = cardSources.Filter(cardSource => !cardSource.IsDigiEgg && !cardSource.IsToken);

        if (eggCards.Count <= 0)
            yield return ContinuousController.instance.StartCoroutine(AddLibraryBottomCards(eggCards));

        if (addedCards.Count <= 0) yield break;

        yield return ContinuousController.instance.StartCoroutine(new AceOverflowClass(addedCards).Overflow());

        foreach (CardSource cardSource in addedCards)
        {
            yield return ContinuousController.instance.StartCoroutine(AddHandCard(cardSource, isDraw));
        }

        if (GManager.instance.turnStateMachine.DoneStartGame && addedCards.Count > 0)
        {
            List<Player> Players = addedCards.Map(cardSource => cardSource.Owner).Distinct().ToList();

            #region Effect of "When the number of cards in hand increases"

            #region Hashtable Setting
            System.Collections.Hashtable _hashtable = new System.Collections.Hashtable()
                {
                    {"Players", Players},
                    {"CardEffect", cardEffect},
                    {"CardSources", addedCards},
                };
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(_hashtable, EffectTiming.OnAddHand));
            #endregion
        }
    }

    public static IEnumerator AddHandCard(CardSource cardSource, bool isDraw)
    {
        cardSource.Init();
        cardSource.SetFace();

        if (!cardSource.Owner.HandCards.Contains(cardSource))
        {
            yield return ContinuousController.instance.StartCoroutine(RemoveFromAllArea(cardSource));

            if (!cardSource.IsToken)
            {
                cardSource.Owner.HandCards.Add(cardSource);

                if (isDraw)
                {
                    if (GManager.instance.turnStateMachine.DoneStartGame)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().AddHandCardEffect(cardSource));
                    }
                }

                else
                {
                    ContinuousController.instance.PlaySE(GManager.instance.DrawSE);
                }

                HandCard handCard = Instantiate(GManager.instance.handCardPrefab, cardSource.Owner.HandTransform);

                handCard.gameObject.name = $"handCard_{cardSource.Owner.PlayerName}";

                handCard.GetComponent<Draggable_HandCard>().startScale = handCard.transform.localScale;

                handCard.GetComponent<Draggable_HandCard>().DefaultY = -9;

                handCard.SetUpHandCard(cardSource);

                AlignHand(cardSource.Owner);

                cardSource.SetShowingHandCard(handCard);

                if (cardSource.Owner.isYou)
                {
                    handCard.SetUpHandCardImage();
                    handCard.ShowCostLevel();
                }

                yield return new WaitForSeconds(Time.deltaTime);

                #region アニメーション
                if (GManager.instance.turnStateMachine.DoneStartGame)
                {
                    cardSource.Owner.HandTransform.GetComponent<GridLayoutGroup>().enabled = false;

                    Vector3 startPosition = Vector3.zero;
                    Vector3 targetPositon = handCard.transform.localPosition;

                    if (cardSource.Owner.isYou)
                    {
                        startPosition = handCard.transform.localPosition + new Vector3(0, 70, 0);
                    }

                    else
                    {
                        startPosition = handCard.transform.localPosition - new Vector3(0, 70, 0);
                    }

                    handCard.transform.localPosition = startPosition;

                    bool end = false;
                    var sequence = DOTween.Sequence();
                    sequence
                        .Append(handCard.transform.DOLocalMove(targetPositon, 0.08f).SetEase(Ease.OutBack))
                        .AppendCallback(() => { end = true; });

                    yield return new WaitWhile(() => !end);
                    end = false;

                    cardSource.Owner.HandTransform.GetComponent<GridLayoutGroup>().enabled = true;
                }
                #endregion

                if (cardSource.Owner.isYou)
                {
                    handCard.SetUpHandCardImage();
                    handCard.ShowCostLevel();
                }
            }
        }
    }
    #endregion

    #region put the card in the trash
    public static IEnumerator AddTrashCard(CardSource cardSource)
    {
        if (!CardEffectCommons.IsExistOnTrash(cardSource))
        {
            yield return ContinuousController.instance.StartCoroutine(RemoveFromAllArea(cardSource));

            if (!cardSource.IsToken)
            {
                cardSource.SetFace();

                if (!CardEffectCommons.IsExistOnTrash(cardSource))
                {
                    cardSource.Owner.TrashCards.Insert(0, cardSource);
                }

                cardSource.Init();
            }
        }
    }
    #endregion

    #region Put the cards together in the trash
    public static IEnumerator AddTrashCards(List<CardSource> cardSources)
    {
        List<CardSource> handCards = new List<CardSource>();

        foreach (CardSource cardSource in cardSources)
        {
            if (cardSource.Owner.HandCards.Contains(cardSource))
            {
                handCards.Add(cardSource);
            }
        }

        foreach (CardSource cardSource in handCards)
        {
            ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().DeleteHandCardEffectCoroutine(cardSource));
        }

        yield return new WaitForSeconds(GManager.instance.GetComponent<Effects>().waitTime_DeleteHandEffect);

        foreach (CardSource cardSource in cardSources)
        {
            if (!CardEffectCommons.IsExistOnTrash(cardSource))
            {
                yield return ContinuousController.instance.StartCoroutine(RemoveFromAllArea(cardSource));

                if (!cardSource.IsToken)
                {
                    cardSource.SetFace();

                    if (!CardEffectCommons.IsExistOnTrash(cardSource))
                    {
                        cardSource.Owner.TrashCards.Insert(0, cardSource);
                    }

                    cardSource.Init();
                }
            }
        }
    }
    #endregion

    #region place a card on top of the deck
    public static IEnumerator AddLibraryTopCards(List<CardSource> cardSources, bool notAddLog = false)
    {
        if (cardSources.Count <= 0) yield break;

        bool isFromTrash = cardSources.Some(cardSource => CardEffectCommons.IsExistOnTrash(cardSource));

        yield return ContinuousController.instance.StartCoroutine(new AceOverflowClass(cardSources).Overflow());

        if (isFromTrash)
        {
            #region "Effect of "When a card is returned from the trash to the deck"

            #region Hashtable Setting
            System.Collections.Hashtable hashtable = new System.Collections.Hashtable()
            {
                {"CardSources", cardSources}
            };
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnReturnCardsToLibraryFromTrash));
            #endregion
        }

        foreach (CardSource cardSource in cardSources)
        {
            bool isFromHand = CardEffectCommons.IsExistOnHand(cardSource);

            yield return ContinuousController.instance.StartCoroutine(RemoveFromAllArea(cardSource));

            if (isFromHand && GManager.instance.turnStateMachine.DoneStartGame)
            {
                yield return new WaitForSeconds(GManager.instance.GetComponent<Effects>().waitTime_DeleteHandEffect);
            }

            if (!cardSource.IsToken)
            {
                cardSource.SetFace();

                if (!cardSource.IsDigiEgg)
                {
                    if (!cardSource.Owner.LibraryCards.Contains(cardSource))
                    {
                        cardSource.Owner.LibraryCards.Insert(0, cardSource);
                    }
                }

                else
                {
                    if (!cardSource.Owner.DigitamaLibraryCards.Contains(cardSource))
                    {
                        cardSource.Owner.DigitamaLibraryCards.Insert(0, cardSource);
                    }
                }

                cardSource.Init();
            }
        }

        #region Log
        if (!notAddLog)
        {
            if (cardSources.Count >= 1)
            {
                string log = "";

                log += $"\nDeck Top card{Utils.PluralFormSuffix(cardSources.Count)}:";

                foreach (CardSource cardSource in cardSources)
                {
                    log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
                }

                log += "\n";

                PlayLog.OnAddLog?.Invoke(log);
            }
        }
        #endregion
    }
    #endregion

    #region put a card at the bottom of the deck
    public static IEnumerator AddLibraryBottomCards(List<CardSource> cardSources, bool notAddLog = false)
    {
        if (cardSources.Count <= 0) yield break;

        bool isFromTrash = cardSources.Some(cardSource => CardEffectCommons.IsExistOnTrash(cardSource));

        yield return ContinuousController.instance.StartCoroutine(new AceOverflowClass(cardSources).Overflow());

        if (isFromTrash)
        {
            #region Effect of "When a card returns from the trash to the deck"

            #region Hashtable Setting
            System.Collections.Hashtable hashtable = new System.Collections.Hashtable()
            {
                {"CardSources", cardSources}
            };
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnReturnCardsToLibraryFromTrash));
            #endregion
        }

        foreach (CardSource cardSource in cardSources)
        {
            if (cardSource.PermanentOfThisCard() != null)
            {
                if (cardSource.PermanentOfThisCard().DigivolutionCards.Contains(cardSource))
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                    .RemoveDigivolveRootEffect(cardSource, cardSource.PermanentOfThisCard()));
                }
            }
        }

        foreach (CardSource cardSource in cardSources.Clone())
        {
            bool isFromHand = CardEffectCommons.IsExistOnHand(cardSource);

            yield return ContinuousController.instance.StartCoroutine(RemoveFromAllArea(cardSource));

            if (isFromHand && GManager.instance.turnStateMachine.DoneStartGame)
            {
                yield return new WaitForSeconds(GManager.instance.GetComponent<Effects>().waitTime_DeleteHandEffect);
            }

            if (!cardSource.IsToken)
            {
                cardSource.SetFace();

                if (!cardSource.IsDigiEgg)
                {
                    if (!cardSource.Owner.LibraryCards.Contains(cardSource))
                    {
                        cardSource.Owner.LibraryCards.Add(cardSource);
                    }
                }

                else
                {
                    if (!cardSource.Owner.DigitamaLibraryCards.Contains(cardSource))
                    {
                        cardSource.Owner.DigitamaLibraryCards.Add(cardSource);
                    }
                }

                cardSource.Init();
            }
        }

        #region Log
        if (!notAddLog)
        {
            if (cardSources.Count >= 1)
            {
                string log = "";

                log += $"\nDeck Bottom card{Utils.PluralFormSuffix(cardSources.Count)}:";

                foreach (CardSource cardSource in cardSources)
                {
                    log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
                }

                log += "\n";

                PlayLog.OnAddLog?.Invoke(log);
            }
        }
        #endregion
    }
    #endregion

    #region Place the card in the processing area
    public static IEnumerator AddExecutingCard(CardSource cardSource)
    {
        if (!cardSource.Owner.ExecutingCards.Contains(cardSource))
        {
            yield return ContinuousController.instance.StartCoroutine(RemoveFromAllArea(cardSource));

            if (!cardSource.IsToken)
            {
                cardSource.SetFace();

                cardSource.Owner.ExecutingCards.Insert(0, cardSource);

                cardSource.Init();
            }
        }
    }
    #endregion

    #region Add card to security
    public static IEnumerator AddSecurityCard(CardSource cardSource, bool toTop = true, bool faceUp = false, bool useEffect = true)
    {
        if (!cardSource.Owner.SecurityCards.Contains(cardSource))
        {
            yield return ContinuousController.instance.StartCoroutine(RemoveFromAllArea(cardSource));

            if (cardSource.IsDigiEgg)
            {
                cardSource.Owner.DigitamaLibraryCards.Add(cardSource);
            }
            else if (!cardSource.IsToken)
            {
                if (!faceUp)
                    cardSource.SetReverse();
                else
                    cardSource.SetFace();

                cardSource.Owner.SecurityCards.Insert(0, cardSource);

                if (!toTop)
                {
                    cardSource.Owner.SecurityCards.Remove(cardSource);
                    cardSource.Owner.SecurityCards.Add(cardSource);
                }

                if(useEffect)
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateRecoveryEffect(cardSource.Owner));

                yield return ContinuousController.instance.StartCoroutine(new IAddSecurity(cardSource).AddSecurity());
            }
        }
    }
    #endregion

    #region Shuffle
    public static IEnumerator Shuffle(Player player)
    {
        ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);

        player.LibraryCards = RandomUtility.ShuffledDeckCards(player.LibraryCards);

        yield return ContinuousController.instance.StartCoroutine(player.ShuffleAnimation());
    }
    #endregion

    #region move permanent
    public static IEnumerator MovePermanent(FieldCardFrame movingPermanentFrame, bool toBreeding = false, ICardEffect effect = null)
    {
        if (movingPermanentFrame != null)
        {
            Player player = movingPermanentFrame.player;

            Permanent movingPermanent = movingPermanentFrame.GetFramePermanent();

            if (movingPermanent != null)
            {
                bool prevIsFrontLine = movingPermanentFrame.IsBattleAreaFrame();
                FieldCardFrame moveTargetFrame = movingPermanent.TopCard.PreferredFrame();

                if (toBreeding)
                {
                    moveTargetFrame = player.fieldCardFrames.Filter(frame => frame.isBreedingAreaFrame()).ToList()[0];

                    #region cut in effect

                    // "When permanents would remove field" effect
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
                        CardEffectCommons.WhenPermanentWouldRemoveFieldCheckHashtable(
                            new List<Permanent> { movingPermanent },
                            effect,
                            null
                        ),
                        EffectTiming.WhenRemoveField));

                    if (GManager.instance.autoProcessing_CutIn.HasAwaitingActivateEffects())
                    {
                        // effect
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.ShrinkSecurityDigimonDisplay());

                        // cut in effect process
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(false, null));
                    }
                    #endregion
                }

                if (moveTargetFrame != null && moveTargetFrame != null)
                {
                    if (moveTargetFrame.GetFramePermanent() == null)
                    {
                        int oldFrameID = movingPermanentFrame.FrameID;
                        int newFrameID = moveTargetFrame.FrameID;

                        player.FieldPermanents[oldFrameID] = null;
                        player.fieldCardFrames[oldFrameID].SetFramePermanent(null);
                        player.FieldPermanents[newFrameID] = movingPermanent;
                        player.fieldCardFrames[newFrameID].SetFramePermanent(movingPermanent);

                        #region animation
                        bool end = false;
                        var sequence = DOTween.Sequence();

                        FieldPermanentCard movingPermanentCard = movingPermanent.ShowingPermanentCard;

                        Vector3 TargetPosition = moveTargetFrame.GetLocalCanvasPosition();

                        Vector3 oldPosition = movingPermanentFrame.GetLocalCanvasPosition();

                        float GoTime = 0.2f;

                        sequence
                            .Append(movingPermanentCard.transform.DOLocalMove(TargetPosition, GoTime).SetEase(Ease.OutCubic))
                            .AppendCallback(() =>
                            {
                                end = true;
                            });

                        sequence.Play();

                        yield return new WaitWhile(() => !end);
                        end = false;
                        #endregion

                        if (!prevIsFrontLine)
                        {
                            movingPermanent.TopCard.SetChangedLocationTime();//register that the card moved for the purpose of the start of timestamped continuous effects

                            #region Effect of "when moving from the training area"

                            #region Hashtable Setting
                            System.Collections.Hashtable hashtable = new System.Collections.Hashtable()
                            {
                                {"Permanent", movingPermanent}
                            };
                            #endregion

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnMove));
                            #endregion
                        }
                        if (toBreeding)
                        {
                            // "When permanents leave the battle area" effect
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(
                                CardEffectCommons.OnDeletionHashtable(
                                    new List<Permanent> { movingPermanent },
                                    effect,
                                    null,
                                    false
                                ),
                                EffectTiming.OnLeaveFieldAnyone));
                        }
                    }
                }

            }
        }
    }
    #endregion
}
