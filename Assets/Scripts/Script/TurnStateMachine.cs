using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;
using static UnityEngine.UIElements.UxmlAttributeDescription;
public class TurnStateMachine : MonoBehaviourPunCallbacks
{
    //Class to manage battle status
    public GameContext gameContext;

    //Wheteher Selecting some card
    public bool IsSelecting = false;

    //Synchronization
    public bool isSync;

    //Effects in use
    public bool isExecuting;

    //Whether it is the first turn of the first player
    public bool isFirstPlayerFirstTurn { get; set; } = true;

    //Turn Count
    public int TurnCount { get; set; } = 0;
    public bool isSecurityCehck { get; set; } = false;

    #region Initialization

    public IEnumerator Init()
    {
        yield return StartCoroutine(GManager.instance.LoadingObject.StartLoading("Now Loading"));

        yield return StartCoroutine(ContinuousController.LoadCoroutine());

        ContinuousController.instance.PlaySE(GManager.instance.StartBattleSE);

        #region initialize parameter
        _canPlayTargetFrames = new bool[GManager.instance.You.fieldCardFrames.Count];
        _canDigivolves = new bool[GManager.instance.You.fieldCardFrames.Count];
        _canJogresses = new bool[GManager.instance.You.fieldCardFrames.Count];
        _canBursts = new bool[GManager.instance.You.fieldCardFrames.Count];
        _canAppFusions = new bool[GManager.instance.You.fieldCardFrames.Count];
        _payingCosts = new int[GManager.instance.You.fieldCardFrames.Count];
        #endregion

        #region AIモード
        if (GManager.instance.IsAI)
        {
            ContinuousController.instance.isRandomMatch = true;

            if (!PhotonNetwork.IsConnected)
            {
                yield return ContinuousController.instance.StartCoroutine(PhotonUtility.ConnectToMasterServerCoroutine());
            }

            yield return new WaitWhile(() => !PhotonNetwork.IsConnectedAndReady);

            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }

            yield return new WaitWhile(() => !PhotonNetwork.InLobby);

            if (!PhotonNetwork.InRoom)
            {
                //Setting up the room to be created
                RoomOptions roomOptions = new RoomOptions();
                roomOptions.IsVisible = false;   //Make the room invisible in the lobby.
                roomOptions.IsOpen = false;      //Not Allow other players to enter the room
                roomOptions.PublishUserId = true;

                roomOptions.MaxPlayers = 1;

                string RoomName = StringUtils.GeneratePassword_AlpahabetNum(50);

                //Create Room
                PhotonNetwork.CreateRoom(RoomName, roomOptions, null);
            }

            yield return new WaitWhile(() => !PhotonNetwork.InRoom);
        }
        #endregion

        #region gameContextの設定
        gameContext = new GameContext(GManager.instance.You, GManager.instance.Opponent);
        #endregion

        #region プレイヤー名設定

        #region Extract master and non-master clients among Photon clients
        Photon.Realtime.Player MasterPlayer = null;
        Photon.Realtime.Player nonMasterPlayer = null;

        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber != PhotonNetwork.CurrentRoom.MasterClientId)
            {
                nonMasterPlayer = player;
            }

            else
            {
                MasterPlayer = player;
            }
        }
        #endregion

        #region Save each player name
        SetPlayerName(0, PlayerName(MasterPlayer));
        SetPlayerName(1, PlayerName(nonMasterPlayer));
        #endregion

        #region Player name for that Photon client
        string PlayerName(Photon.Realtime.Player player)
        {
            #region 対人戦
            if (!GManager.instance.IsAI)
            {
                ExitGames.Client.Photon.Hashtable hashtable = player.CustomProperties;

                if (HasPlayerName(player))
                {
                    if (hashtable.TryGetValue(ContinuousController.PlayerNameKey, out object value))
                    {
                        string playerName = (string)value;

                        Debug.Log($"playername:{playerName}");

                        if (!string.IsNullOrEmpty(playerName) && playerName != "Player")
                        {
                            return playerName;
                        }

                        if (player == PhotonNetwork.LocalPlayer)
                        {
                            return "You";
                        }

                        else
                        {
                            return "Opponent";
                        }

                        return playerName;
                    }
                }

                Debug.Log($"playername:None");
                return "";
            }
            #endregion

            #region AI mode
            else
            {
                #region Player Name
                if (player == MasterPlayer)
                {
                    return ContinuousController.instance.PlayerName;
                }
                #endregion

                #region AI Player Name
                else
                {
                    return "Bot";
                }
                #endregion

            }
            #endregion

            #region Determine if that Photon client has custom properties for player names
            bool HasPlayerName(Photon.Realtime.Player _player)
            {
                ExitGames.Client.Photon.Hashtable _hashtable = _player.CustomProperties;

                if (_hashtable.TryGetValue(ContinuousController.PlayerNameKey, out object value))
                {
                    string playerName = (string)value;

                    if (!string.IsNullOrEmpty(playerName))
                    {
                        if (playerName.Length <= ContinuousController.instance.PlayerNameMaxLength)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            #endregion
        }
        #endregion

        #region Store player names in Player class and display UI
        void SetPlayerName(int _PlayerID, string _PlayerName)
        {
            Player player = gameContext.PlayerFromID(_PlayerID);
            player.PlayerName = _PlayerName;

            if (player.PlayerNameText != null)
            {
                player.PlayerNameText.transform.parent.gameObject.SetActive(true);
                player.PlayerNameText.gameObject.SetActive(true);

                if(player.isYou || GManager.instance.IsAI)
                    player.PlayerNameText.text = player.PlayerName;
            }
        }
        #endregion
        #endregion

        #region 乱数列初期化
        if (PhotonNetwork.IsMasterClient)
        {
            ContinuousController.instance.GetComponent<PhotonView>().RPC("SetRandom", RpcTarget.All, RandomUtility.getRamdom());
        }

        yield return new WaitWhile(() => !ContinuousController.instance.DoneSetRandom);
        ContinuousController.instance.DoneSetRandom = false;
        #endregion

        yield return GManager.instance.photonWaitController.StartWait("EndSetRandom");

        #region デッキカード生成
        yield return StartCoroutine(CardObjectController.CreatePlayerDecks(GManager.instance.CardPrefab, gameContext));
        yield return new WaitForSeconds(0.2f);
        #endregion

        /*#region ログのクリック処理を追加
        foreach (CardSource cardSource in gameContext.ActiveCardList)
        {
            GManager.instance.playLog.AddOnClick_ShowCard(cardSource);
        }
        #endregion*/

        #region メモリーを初期化
        GManager.instance.memoryObject.Init();
        #endregion


        foreach (Player player in gameContext.Players)
        {
            yield return StartCoroutine(player.brainStormObject.Init());
        }

        #region Deciding whether to attack first or last
        gameContext.TurnPlayer = gameContext.PlayerFromID(UnityEngine.Random.Range(0, 2));

        #region get first player from room custom property
        int firstPlayerId = -1;

        ExitGames.Client.Photon.Hashtable roomHash = PhotonNetwork.CurrentRoom.CustomProperties;

        if (roomHash != null)
        {
            if (roomHash.TryGetValue(DataBase.FirstPlayerKey, out object value))
            {
                if (value is int)
                {
                    firstPlayerId = (int)value;
                }
            }
        }

        if (firstPlayerId >= 0)
        {
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if (player.ActorNumber == firstPlayerId)
                {
                    int playerID = player.ActorNumber == PhotonNetwork.CurrentRoom.MasterClientId ? 0 : 1;
                    gameContext.TurnPlayer = gameContext.PlayerFromID(playerID).Enemy;
                }
            }
        }
        #endregion

        #endregion


        yield return StartCoroutine(GManager.instance.LoadingObject.EndLoading());

        StartCoroutine(GameStateMachine());

        if (GManager.instance.bgms.Count >= 1)
        {
            GManager.instance.BattleBGM.StartPlayBGM(GManager.instance.bgms[UnityEngine.Random.Range(0, GManager.instance.bgms.Count)]);
        }
    }
    #endregion

    #region Manage turn progress
    public IEnumerator GameStateMachine()
    {
        yield return StartCoroutine(StartGame());

        while (true)
        {
            if (endGame)
            {
                yield break;
            }

            gameContext.SwitchTurnPlayer();

            yield return StartCoroutine(ActivePhase());

            yield return StartCoroutine(DrawPhase());

            if (endGame)
            {
                yield break;
            }

            yield return StartCoroutine(BreedingPhase());

            yield return StartCoroutine(MainPhase());

            if (endGame)
            {
                yield break;
            }

            yield return StartCoroutine(EndPhase());
        }
    }
    #endregion

    #region At the start of the game
    public bool DoneStartGame { get; set; } = false;

    bool _isRedraw = false;
    bool _endSelect = false;
    IEnumerator StartGame()
    {
#if UNITY_EDITOR
        //gameContext.TurnPlayer = GManager.instance.Opponent;
#endif
        yield return GManager.instance.photonWaitController.StartWait("StartGame");

        #region 先攻・後攻の決定
        if (gameContext.NonTurnPlayer.isYou)
        {
            //GManager.instance.commandText.OpenCommandText("You are the FIRST player.");
        }

        else
        {
            //GManager.instance.commandText.OpenCommandText("You are the SECOND player.");
        }

        gameContext.FirstPlayer = gameContext.NonTurnPlayer;
        gameContext.NonTurnPlayer.FirstObject.SetActive(true);

        //yield return new WaitForSeconds(0.6f);

        GManager.instance.commandText.CloseCommandText();
        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);


        #endregion

        yield return GManager.instance.photonWaitController.StartWait("EndSelectStartPlayer");

        foreach (Player player in gameContext.Players_ForNonTurnPlayer)
        {
            yield return StartCoroutine(new DrawClass(player, 5, null).Draw());
        }

        yield return GManager.instance.photonWaitController.StartWait("EndDrawStartGame");

        #region マリガン
        foreach (Player player in gameContext.Players_ForNonTurnPlayer)
        {
            _isRedraw = false;
            _endSelect = false;

            yield return GManager.instance.photonWaitController.StartWait($"Mulligan");

            if (!player.isYou)
            {
                GManager.instance.commandText.OpenCommandText("The opponent is selecting mulligan.");
            }

            if (player.isYou)
            {
                if (GManager.instance.isAuto && GManager.instance.IsAI)
                {
                    SetRedraw(RandomUtility.IsSucceedProbability(0.5f));
                }

                else
                {
                    string message = "Will you mulligan your hand?";

                    if (player == gameContext.NonTurnPlayer)
                    {
                        message += "\n(You are <color=#FF633E>FIRST</color>)";
                    }

                    else
                    {
                        message += "\n(You are <color=#FF633E>SECOND</color>)";
                    }

                    yield return StartCoroutine(GManager.instance.selectCardPanel.OpenSelectCardPanel(
                        Message: message,
                        NotSelectButtonMessage: "Keep Hand",
                        EndSelectButtonMessage: "Mulligan",
                        _OnClickNotSelectButtonAction: () => SetRedraw_RPC(false),
                        _OnClickEndSelectButtonAction: () => SetRedraw_RPC(true),
                        RootCardSources: player.HandCards,
                        _CanTargetCondition: (cardSource) => false,
                        _CanTargetCondition_ByPreSelecetedList: null,
                        _CanEndSelectCondition: null,
                        _MaxCount: 6,
                        _CanEndNotMax: true,
                        _CanNoSelect: () => true,
                        CanLookReverseCard: true,
                        skillInfos: null,
                        root: SelectCardEffect.Root.None));

                    void SetRedraw_RPC(bool _isDraw)
                    {
                        photonView.RPC("SetRedraw", RpcTarget.All, _isDraw);
                    }
                }
            }

            else
            {
                #region AI
                if (GManager.instance.IsAI)
                {
                    bool doRedraw = false;

                    if (player.HandCards.Count((cardSource) => cardSource.IsDigimon && cardSource.Level == 3) == 0)
                    {
                        doRedraw = true;
                    }

                    SetRedraw(doRedraw);
                }
                #endregion
            }

            yield return new WaitWhile(() => !_endSelect);
            _endSelect = false;

            GManager.instance.selectCardPanel.CloseSelectCardPanel();

            GManager.instance.commandText.CloseCommandText();
            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

            if (_isRedraw)
            {
                #region マリガンを行う

                #region 手札のカードを削除
                List<HandCard> HandCardObjects = new List<HandCard>();

                foreach (HandCard handCard in player.HandCardObjects)
                {
                    HandCardObjects.Add(handCard);
                }

                for (int i = 0; i < HandCardObjects.Count; i++)
                {
                    Destroy(HandCardObjects[i].gameObject);
                }
                #endregion

                #region 手札のカードを山札の下に加える
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(player.HandCards.Clone(),true));

                #region Log
                if (_isRedraw)
                    PlayLog.OnAddLog?.Invoke($"\nMulligan Hand\n{player.PlayerName}\n");
                #endregion

                #endregion

                #region シャッフル
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.Shuffle(player));
                #endregion

                #region 5枚引き直す
                yield return StartCoroutine(new DrawClass(player, 5, null).Draw());
                #endregion

                #endregion
            }
        }
        #endregion

        #region set up security
        foreach (Player player in gameContext.Players_ForNonTurnPlayer)
        {
            yield return StartCoroutine(new IAddSecurityFromLibrary(player, 5).AddSecurity());
        }
        #endregion

        DoneStartGame = true;
    }

    [PunRPC]
    public void SetStartPlayer(bool doChange)
    {
        if (doChange)
        {
            gameContext.TurnPlayer = gameContext.NonTurnPlayer;
        }
    }

    [PunRPC]
    void SetRedraw(bool isRedraw)
    {
        _isRedraw = isRedraw;
        _endSelect = true;
    }
    #endregion

    #region Active Phase
    IEnumerator ActivePhase()
    {
        foreach (Permanent permanent in gameContext.TurnPlayer.GetFieldPermanents())
        {
            permanent.UntilOwnerTurnStartEffects = new List<Func<EffectTiming, ICardEffect>>();
        }

        ContinuousController.instance.PlaySE(GManager.instance.StartBattleSE);

        foreach (Player player in gameContext.Players_ForTurnPlayer)
        {
            player.FirstObject.SetActive(false);
        }

        #region ログ追加
        PlayLog.OnAddLog?.Invoke($"\n---------------------------------\nActive Phase:\n{gameContext.TurnPlayer.PlayerName}\n");
        #endregion

        TurnCount++;

        gameContext.TurnPlayer.TurnCount++;

        isSync = true;
        gameContext.TurnPhase = GameContext.phase.Active;
        Debug.Log($"{gameContext.TurnPlayer}:Start Turn({TurnCount}th Turn)");
        yield return GManager.instance.photonWaitController.StartWait("StartTrun");
        isSync = false;

        GManager.instance.showTurnPlayerObject.ShowTurnPlayer(gameContext.TurnPlayer);

        GManager.instance.nextPhaseButton.SwitchTurnSprite();

        yield return new WaitWhile(() => !GManager.instance.showTurnPlayerObject.isClose);

        // ターン開始時の効果
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(null, EffectTiming.OnStartTurn));

        // 自動処理チェックタイミング
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

        //ターン終了チェック
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnCheck());

        if (gameContext.TurnPhase == GameContext.phase.End)
        {
            yield break;
        }

        isSync = true;

        #region Unsuspend

        #region Untap permanents in play

        List<Permanent> unsuspendPermanents = new List<Permanent>();

        //Add field Permanents to unsuspend list
        foreach (Permanent permanent in gameContext.PermanentsForTurnPlayer)
        {
            if (permanent.IsSuspended && permanent.CanUnsuspend)
            {
                if (permanent.TopCard.Owner == gameContext.TurnPlayer || permanent.HasReboot)
                {
                    unsuspendPermanents.Add(permanent);
                }
            }
        }

        //Unsuspend raising area Permanents
        foreach (Permanent permanent in gameContext.TurnPlayer.GetBreedingAreaPermanents())
        {
            if (permanent.IsSuspended)
            {
                permanent.IsSuspended = false;

                if (permanent.ShowingPermanentCard != null)
                {
                    permanent.ShowingPermanentCard.ShowPermanentData(true);
                }
                /*if (permanent.TopCard.Owner == gameContext.TurnPlayer)
                {
                    unsuspendPermanents.Add(permanent);
                }*/
            }
        }

        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(unsuspendPermanents, null).Unsuspend());

        #endregion

        #endregion

        //自動処理チェックタイミング
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

        #region アクティブフェイズ終了時までの効果をリセット
        gameContext.TurnPlayer.UntilOwnerActivePhaseEffects = new List<Func<EffectTiming, ICardEffect>>();
        foreach(Permanent permanent in gameContext.TurnPlayer.GetBattleAreaDigimons())
            permanent.UntilNextUntapEffects = new List<Func<EffectTiming, ICardEffect>>();
        #endregion
    }
    #endregion

    #region Draw Phase
    IEnumerator DrawPhase()
    {
        //ターン終了チェック
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnCheck());

        if (gameContext.TurnPhase == GameContext.phase.End)
        {
            yield break;
        }

        #region ログ追加
        PlayLog.OnAddLog?.Invoke($"\nDraw Phase:\n{gameContext.TurnPlayer.PlayerName}\n");
        #endregion

        isSync = true;
        gameContext.TurnPhase = GameContext.phase.Draw;
        yield return GManager.instance.photonWaitController.StartWait("DrawPhase");
        isSync = false;

        isSync = true;

        #region ドロー
        if (TurnCount != 1)
        {
            #region 引けなかったら負け
            if (gameContext.TurnPlayer.LibraryCards.Count == 0)
            {
                EndGame(gameContext.NonTurnPlayer, false);

                yield break;
            }
            #endregion

            yield return StartCoroutine(new DrawClass(gameContext.TurnPlayer, 1, null).Draw());
        }
        #endregion

        //自動処理チェックタイミング
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());
        //ターン終了チェック
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnCheck());
    }
    #endregion

    #region Breeding Phase
    IEnumerator BreedingPhase()
    {
        //ターン終了チェック
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnCheck());

        if (gameContext.TurnPhase == GameContext.phase.End)
        {
            yield break;
        }

        #region ログ追加
        PlayLog.OnAddLog?.Invoke($"\nBreeding Phase:\n{gameContext.TurnPlayer.PlayerName}\n");
        #endregion

        isSync = true;
        gameContext.TurnPhase = GameContext.phase.Breeding;
        yield return GManager.instance.photonWaitController.StartWait("BreedingPhase");
        isSync = false;
        IsSelecting = false;

        if (gameContext.TurnPlayer.CanHatch || gameContext.TurnPlayer.CanMove)
        {
            GManager.instance.showPhaseNotificationObject.ShowPhase(GameContext.phase.Breeding);

            yield return new WaitWhile(() => !GManager.instance.showPhaseNotificationObject.isClose);

            if (gameContext.TurnPlayer.isYou)
            {
                #region If hatching is possible
                if (gameContext.TurnPlayer.CanHatch || !gameContext.TurnPlayer.CanMove)
                {
                    GManager.instance.hideCannotSelectObject.SetUpHideCannotSelectObject(new List<FieldPermanentCard>() { null }, true);

                    //GManager.instance.commandText.OpenCommandText("BreedingPhase : Will you hatch Digiegg?");

                    if (ContinuousController.instance.autoHatch)
                    {
                        OnClickHatchObject();
                    }

                    else
                    {
                        gameContext.TurnPlayer.SetUpHatchObject(OnClickHatchObject);
                    }
                }
                #endregion
                #region If you can move
                else if (!gameContext.TurnPlayer.CanHatch || gameContext.TurnPlayer.CanMove)
                {
                    //GManager.instance.commandText.OpenCommandText("BreedingPhase : Will you move your Digimon to Battle Area?");

                    FieldPermanentCard fieldPermanentCard = gameContext.TurnPlayer.GetBreedingAreaPermanents()[0].ShowingPermanentCard;

                    if (fieldPermanentCard != null)
                    {
                        GManager.instance.hideCannotSelectObject.SetUpHideCannotSelectObject(new List<FieldPermanentCard>() { fieldPermanentCard }, false);

                        fieldPermanentCard.AddClickTarget((fieldPermanentCard1) => OnClickHatchObject());
                        fieldPermanentCard.Outline_Select.gameObject.SetActive(true);
                        fieldPermanentCard.SetOrangeOutline();
                    }
                }
                #endregion

                if (gameContext.TurnPlayer.isYou && GManager.instance.isAuto && GManager.instance.IsAI)
                {
                    gameContext.TurnPhase = GameContext.phase.Main;
                }
            }

            else
            {
                //GManager.instance.commandText.OpenCommandText("BreedingPhase : The opponent is selecting the action.");

                #region AI
                if (GManager.instance.IsAI)
                {
                    bool doHatch = RandomUtility.IsSucceedProbability(0.85f);

                    if (gameContext.TurnPlayer.CanHatch)
                    {
                        doHatch = true;
                    }

                    SetBreedingPhase(doHatch);
                }
                #endregion
            }

            yield return new WaitWhile(() => !endSelect_BreedingPhase && gameContext.TurnPhase == GameContext.phase.Breeding);
            GManager.instance.hideCannotSelectObject.Close();
            endSelect_BreedingPhase = false;
            gameContext.TurnPlayer.OffHatchObject();
            OffFieldCardTarget(gameContext.TurnPlayer);
            IsSelecting = true;

            if (gameContext.TurnPhase == GameContext.phase.Breeding)
            {
                if (doAction_BreedingPhase)
                {
                    //hatching
                    if (gameContext.TurnPlayer.CanHatch || !gameContext.TurnPlayer.CanMove)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new HatchDigiEggClass(player: gameContext.TurnPlayer, hashtable: null).Hatch());
                    }

                    //move
                    else if (!gameContext.TurnPlayer.CanHatch || gameContext.TurnPlayer.CanMove)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.MovePermanent(gameContext.TurnPlayer.GetBreedingAreaPermanents()[0].PermanentFrame));
                    }
                }
            }

            gameContext.TurnPhase = GameContext.phase.Breeding;
        }

        //自動処理チェックタイミング
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

        //ターン終了チェック
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnCheck());

        if (gameContext.TurnPhase == GameContext.phase.End)
        {
            yield break;
        }
    }

    void OnClickHatchObject()
    {
        gameContext.TurnPlayer.OffHatchObject();
        photonView.RPC("SetBreedingPhase", RpcTarget.All, true);
    }

    bool endSelect_BreedingPhase = false;
    bool doAction_BreedingPhase = false;
    [PunRPC]
    public void SetBreedingPhase(bool doAction_BreedingPhase)
    {
        this.doAction_BreedingPhase = doAction_BreedingPhase;
        endSelect_BreedingPhase = true;
    }
    #endregion

    #region main phase
    CardSource PlayCard { get; set; } = null;
    int TargetFrameID { get; set; } = 0;
    int[] JogressEvoRootsFrameIDs { get; set; } = new int[0];
    int BurstTamerFrameID { get; set; } = 0;
    int[] AppFusionFrameIDs { get; set; } = new int[0];
    ICardEffect UseCardEffect { get; set; } = null;
    Permanent AttackingPermanent { get; set; } = null;
    Permanent DefendingPermanent { get; set; } = null;
    float _timer = 0f;
    bool _canPlayEmptyFrame = true;
    bool[] _canPlayTargetFrames = new bool[GManager.instance.You.fieldCardFrames.Count];
    bool[] _canDigivolves = new bool[GManager.instance.You.fieldCardFrames.Count];
    bool[] _canJogresses = new bool[GManager.instance.You.fieldCardFrames.Count];
    bool[] _canBursts = new bool[GManager.instance.You.fieldCardFrames.Count];
    bool[] _canAppFusions = new bool[GManager.instance.You.fieldCardFrames.Count];
    int[] _payingCosts = new int[GManager.instance.You.fieldCardFrames.Count];
    IEnumerator MainPhase()
    {
        //ターン終了チェック
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnCheck());

        if (gameContext.TurnPhase == GameContext.phase.End)
        {
            goto EndMainPhase;
        }

        #region Add log
        PlayLog.OnAddLog?.Invoke($"\nMain Phase:\n{gameContext.TurnPlayer.PlayerName}\n");

        #endregion

        #region Reset the selection status of cards in your hand and cards on the field.
        OffHandCardTarget(gameContext.TurnPlayer);
        OffFieldCardTarget(gameContext.TurnPlayer);
        #endregion

        isSync = true;
        gameContext.TurnPhase = GameContext.phase.Main;
        Debug.Log($"{gameContext.TurnPlayer}:Main Phase");
        yield return GManager.instance.photonWaitController.StartWait("MainPhase");

        GManager.instance.showPhaseNotificationObject.ShowPhase(GameContext.phase.Main);

        yield return new WaitWhile(() => !GManager.instance.showPhaseNotificationObject.isClose);

        // Effects at the start of main phase
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(null, EffectTiming.OnStartMainPhase));

        // Automatic processing check timing
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

        isSync = false;

        bool CanSelect()
        {
            //You can play cards from your hand.
            if (gameContext.TurnPlayer.HandCards.Some((_card) => _card.CanPlayFromHandDuringMainPhase))
                return true;

            //There is a permanent in play that can use the effect.
            if (gameContext.TurnPlayer.GetFieldPermanents().Count((permanent) => permanent.CanDeclareSkill()) > 0)
                return true;

            //There is a permanent in play that can attack.
            if (gameContext.TurnPlayer.GetFieldPermanents().Count((permanent) => permanent.CanAttack(null)) > 0)
                return true;

            //I have a card in my hand that can use an effect.
            if (gameContext.TurnPlayer.HandCards.Count((_card) => _card.CanDeclareSkill) > 0)
                return true;

            //I have a card in my trash that can use an effect.
            if (gameContext.TurnPlayer.TrashCards.Count(_card => _card.CanDeclareSkill) > 0)
                return true;

            return false;
        }

        #region Repeat until turn player selects
        while (!endGame)
        {
            yield return GManager.instance.photonWaitController.StartWait("SetHandCardPlayablity");

            //自動処理チェックタイミング
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());
            //ターン終了チェック
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnCheck());

            #region パラメータリセット
            ResetMainPhaseParameter();
            #endregion

            yield return GManager.instance.photonWaitController.StartWait("SetMainPhase");

            if (gameContext.TurnPhase == GameContext.phase.Main)
            {
                if (!CanSelect())
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnProcess());
                }
            }

            if (gameContext.TurnPhase != GameContext.phase.Main)
            {
                goto EndMainPhase;
            }

            StartCoroutine(SetMainPhase());

            #region Wait until selection is complete
            while (PlayCard == null && UseCardEffect == null && AttackingPermanent == null)
            {
                yield return null;

                if (endGame)
                {
                    yield break;
                }

                #region AIモード
                if (GManager.instance.IsAI && !gameContext.TurnPlayer.isYou)
                {
                    if (RandomUtility.IsSucceedProbability(0.99f))
                    {
                        if (gameContext.TurnPlayer.GetFieldPermanents().Count((permanent) => permanent.CanAttack(null)) > 0)
                        {
                            #region アタック
                            List<Permanent> CanAttackPermenents = new List<Permanent>();

                            foreach (Permanent permanent in gameContext.TurnPlayer.GetFieldPermanents())
                            {
                                if (permanent.CanAttack(null))
                                {
                                    CanAttackPermenents.Add(permanent);
                                }
                            }

                            if (CanAttackPermenents.Count >= 1)
                            {
                                AttackingPermanent = CanAttackPermenents[UnityEngine.Random.Range(0, CanAttackPermenents.Count)];

                                bool isSecurityAttack = true;

                                List<Permanent> DefendingPermanentCandidates = new List<Permanent>();

                                foreach (Permanent permanent in gameContext.NonTurnPlayer.GetFieldPermanents())
                                {
                                    if (AttackingPermanent.CanAttackTargetDigimon(permanent, null) && AttackingPermanent.DP >= permanent.DP)
                                    {
                                        DefendingPermanentCandidates.Add(permanent);
                                    }
                                }

                                if (DefendingPermanentCandidates.Count >= 1)
                                {
                                    if (RandomUtility.IsSucceedProbability(0.5f))
                                    {
                                        isSecurityAttack = false;
                                    }
                                }

                                if (gameContext.NonTurnPlayer.SecurityCards.Count <= 1)
                                {
                                    isSecurityAttack = true;
                                }

                                if (!isSecurityAttack)
                                {
                                    if (DefendingPermanentCandidates.Count >= 1)
                                    {
                                        DefendingPermanent = DefendingPermanentCandidates[UnityEngine.Random.Range(0, DefendingPermanentCandidates.Count)];
                                    }

                                    else
                                    {
                                        AttackingPermanent = null;
                                    }
                                }

                                else
                                {
                                    if (!AttackingPermanent.CanAttackTargetDigimon(null, null))
                                    {
                                        AttackingPermanent = null;
                                    }
                                }
                            }
                            #endregion
                        }

#if UNITY_EDITOR
                        AttackingPermanent = null;
#endif

                        if (AttackingPermanent == null)
                        {
                            if (gameContext.TurnPlayer.HandCards.Some((cardSource) => cardSource.CanPlayFromHandDuringMainPhase))
                            {
                                #region カードをプレイ
                                List<CardSource> CanPlayCards = new List<CardSource>();

                                foreach (CardSource cardSource in gameContext.TurnPlayer.HandCards)
                                {
                                    if (cardSource.CanPlayFromHandDuringMainPhase)
                                    {
                                        CanPlayCards.Add(cardSource);
                                    }
                                }

                                CanPlayCards = CanPlayCards
                                    .OrderBy((value) => Array.IndexOf(DataBase.cardKinds, value.CardKind))
                                    .ThenBy((value) => Array.IndexOf(new bool[] { true, false }, value.Owner.fieldCardFrames.Count((frame) => value.CanPlayCardTargetFrame(frame, true, null) && !frame.IsEmptyFrame()) >= 1))
                                    .ToList();

                                if (CanPlayCards.Count > 0)
                                {
                                    foreach (CardSource cardSource in CanPlayCards)
                                    {
                                        if (RandomUtility.IsSucceedProbability(0.99f))
                                        {
                                            PlayCard = cardSource;

                                            if (PlayCard.IsPermanent)
                                            {
                                                List<int> frameIDCandidates = new List<int>();

                                                for (int i = 0; i < gameContext.TurnPlayer.fieldCardFrames.Count; i++)
                                                {
                                                    if (PlayCard.CanPlayCardTargetFrame(gameContext.TurnPlayer.fieldCardFrames[i], true, null))
                                                    {
                                                        int k = i;

                                                        int count = 4;

                                                        if (gameContext.TurnPlayer.fieldCardFrames[k].IsEmptyFrame())
                                                        {
                                                            if (frameIDCandidates.Count((id) => gameContext.TurnPlayer.fieldCardFrames[id].IsEmptyFrame()) >= 1)
                                                            {
                                                                continue;
                                                            }

                                                            count = 1;
                                                        }

                                                        for (int j = 0; j < count; j++)
                                                        {
                                                            frameIDCandidates.Add(k);
                                                        }
                                                    }
                                                }

                                                if (frameIDCandidates.Count >= 1)
                                                {
                                                    TargetFrameID = frameIDCandidates[UnityEngine.Random.Range(0, frameIDCandidates.Count)];

                                                    if (gameContext.TurnPlayer.fieldCardFrames[TargetFrameID].IsEmptyFrame())
                                                    {
                                                        TargetFrameID = PlayCard.PreferredFrame().FrameID;
                                                    }

                                                    break;
                                                }
                                            }

                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                #endregion

                            }
                        }
                    }

                    if (PlayCard == null && UseCardEffect == null && UseCardEffect == null && AttackingPermanent == null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnProcess());
                    }
                }
                #endregion

                else
                {
                    if (gameContext.TurnPlayer.isYou && GManager.instance.isAuto && GManager.instance.IsAI)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnProcess());
                    }
                }

                //EndTurnProcess

                if (gameContext.TurnPhase != GameContext.phase.Main)
                {
                    goto EndMainPhase;
                }
            }

            ResetUI();

            _timer = 0f;

            while (true)
            {
                yield return null;
                _timer += Time.deltaTime;

                if (!GManager.instance.commandText.gameObject.activeSelf)
                {
                    break;
                }

                if (_timer >= 0.6f)
                {
                    GManager.instance.commandText.gameObject.SetActive(false);
                }
            }

            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);
            #endregion

            #region Use activation effect
            if (UseCardEffect != null)
            {
                if (UseCardEffect is ActivateICardEffect)
                {
                    if (UseCardEffect.CanUse(null))
                    {
                        UseCardEffect.SetIsDeclarative(true);

                        //Count up the number of uses
                        if (UseCardEffect.MaxCountPerTurn < 100)
                        {
                            UseCardEffect.EffectSourceCard.cEntity_EffectController.RegisterUseEfffectThisTurn(UseCardEffect);
                        }

                        // yield return StartCoroutine(((ActivateICardEffect)UseCardEffect).Activate_Optional_Effect_Execute(null));
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.ActivateEffectProcess(
                                UseCardEffect,
                                null));
                    }
                }
            }
            #endregion

            #region play cards
            else if (PlayCard != null)
            {
                isSync = true;

                yield return StartCoroutine(GManager.instance.GetComponent<Effects>().DeleteHandCardEffectCoroutine(PlayCard));

                yield return StartCoroutine(GManager.instance.GetComponent<Effects>().ShowUseHandCardEffect_PlayCard(PlayCard));

                Permanent targetPermanent = null;

                if (0 <= TargetFrameID && TargetFrameID <= gameContext.TurnPlayer.fieldCardFrames.Count - 1)
                {
                    targetPermanent = gameContext.TurnPlayer.fieldCardFrames[TargetFrameID].GetFramePermanent();
                }

                //ログ追加
                PlayLog.OnAddLog?.Invoke($"\nPlay Card:\n{PlayCard.BaseENGCardNameFromEntity}({PlayCard.CardID})\n");

                PlayCardClass playCard = new PlayCardClass(
                    cardSources: new List<CardSource>() { PlayCard },
                    hashtable: null,
                    payCost: true,
                    targetPermanent: targetPermanent,
                    isTapped: false,
                    root: SelectCardEffect.Root.Hand,
                    activateETB: true);

                if (JogressEvoRootsFrameIDs != null)
                {
                    if (JogressEvoRootsFrameIDs.Length == 2)
                    {
                        playCard.SetJogress(JogressEvoRootsFrameIDs);
                    }
                }

                if (0 <= BurstTamerFrameID && BurstTamerFrameID <= gameContext.TurnPlayer.fieldCardFrames.Count - 1)
                {
                    playCard.SetBurst(BurstTamerFrameID, PlayCard);
                }

                if (AppFusionFrameIDs != null && AppFusionFrameIDs.Length == 2)
                {
                    playCard.SetAppFusion(AppFusionFrameIDs);
                }


                yield return StartCoroutine(playCard.PlayCard());

                isSync = false;
            }
            #endregion

            #region attack
            else if (AttackingPermanent != null)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.Attack(AttackingPermanent, DefendingPermanent, null));
            }
            #endregion
        }
    #endregion

    EndMainPhase:;

        #region Main phase ends
        GManager.instance.selectCommandPanel.CloseSelectCommandPanel();
        GManager.instance.commandText.CloseCommandText();

        _timer = 0f;

        while (true)
        {
            yield return null;
            _timer += Time.deltaTime;

            if (!GManager.instance.commandText.gameObject.activeSelf)
            {
                break;
            }

            if (_timer >= 0.6f)
            {
                GManager.instance.commandText.gameObject.SetActive(false);
            }
        }

        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);
        GManager.instance.OffTargetArrow();

        ResetMainPhaseParameter();
        #endregion

        //Automatic processing check timing
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

        //ターン終了

        #region Parameter reset
        void ResetMainPhaseParameter()
        {
            GManager.instance.OffTargetArrow();

            GManager.instance.memoryObject.OffMemoryPredictionLine();

            #region Reset the display of cards in hand, cards on the field, and frames
            foreach (Player player in gameContext.Players)
            {
                foreach (FieldCardFrame fieldCardFrame in player.fieldCardFrames)
                {
                    fieldCardFrame.OffFrame_Select();
                }

                player.playMatCardFrame.OffFrame_Select();

                foreach (FieldPermanentCard fieldCharaCard in player.FieldPermanentObjects)
                {
                    fieldCharaCard.RemoveSelectEffect();
                    fieldCharaCard.RemoveClickTarget();
                    fieldCharaCard.RemoveDragTarget();
                    fieldCharaCard.CloseCommandPanel();
                }

                foreach (HandCard handCard in player.HandCardObjects)
                {
                    handCard.RemoveSelectEffect();
                    handCard.RemoveClickTarget();
                    handCard.RemoveDragTarget();
                }

                player.securityObject.securityBreakGlass.gameObject.SetActive(false);
                player.securityObject.OffShowSecurityAttackObject();
            }
            #endregion

            #region Reset cards in hand and field
            OffHandCardTarget(gameContext.TurnPlayer);
            OffFieldCardTarget(gameContext.TurnPlayer);

            foreach (HandCard handCard in gameContext.TurnPlayer.HandCardObjects)
            {
                handCard.GetComponent<Draggable_HandCard>().CanPointerEnterExitAction = true;
            }
            #endregion

            IsSelecting = false;
            isSync = false;
            PlayCard = null;
            TargetFrameID = -1;
            JogressEvoRootsFrameIDs = new int[0];
            BurstTamerFrameID = -1;
            AppFusionFrameIDs = new int[0];
            UseCardEffect = null;
            AttackingPermanent = null;
            DefendingPermanent = null;
        }
        #endregion
    }

    #region Added main phase operations
    public IEnumerator SetMainPhase()
    {
        if (isSync)
        {
            yield break;
        }

        yield return new WaitWhile(() => isSync);

        if (gameContext.TurnPhase != GameContext.phase.Main)
        {
            yield break;
        }

        IsSelecting = false;

        #region reset

        OffFieldCardTarget(gameContext.TurnPlayer);
        OffHandCardTarget(gameContext.TurnPlayer);

        #region rearrange cards in hand
        GManager.instance.You.HandTransform.GetComponent<GridLayoutGroup>().enabled = false;

        foreach (HandCard handCard in GManager.instance.You.HandCards.Map(cardSource => cardSource.ShowingHandCard).Filter(handCard => handCard != null)
        .Clone())
        {
            if (handCard != null)
            {
                handCard.transform.SetParent(GManager.instance.You.HandTransform);
                handCard.GetComponent<Draggable_HandCard>().CanPointerEnterExitAction = true;
            }
        }

        GManager.instance.You.HandTransform.GetComponent<GridLayoutGroup>().enabled = true;
        #endregion

        foreach (Player player in gameContext.Players)
        {
            #region Reset permanents in play
            foreach (FieldPermanentCard fieldPermanentCard in player.FieldPermanentObjects)
            {
                fieldPermanentCard.RemoveSelectEffect();
                fieldPermanentCard.RemoveDragTarget();
                fieldPermanentCard.RemoveClickTarget();
                fieldPermanentCard.fieldUnitCommandPanel.CloseCommandPanel();
            }
            #endregion

            #region Reset cards in hand
            foreach (HandCard handCard in player.HandCardObjects)
            {
                handCard.RemoveSelectEffect();
                handCard.RemoveDragTarget();
                handCard.RemoveClickTarget();
            }
            #endregion

            player.securityObject.RemoveClickTarget();

            player.securityObject.securityBreakGlass.gameObject.SetActive(false);

            player.securityObject.OffShowSecurityAttackObject();
        }

        GManager.instance.selectCommandPanel.CloseSelectCommandPanel();
        GManager.instance.BackButton.CloseSelectCommandButton();

        GManager.instance.You.playMatCardFrame.Frame.transform.parent.gameObject.SetActive(false);

        GManager.instance.memoryObject.OffMemoryPredictionLine();

        GManager.instance.commandText.CloseCommandText();
        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);
        #endregion

        // added
        yield return new WaitWhile(() => isSync);

        #region Click/drag operation
        if (gameContext.TurnPlayer.isYou)
        {
            #region permanent of the place
            foreach (FieldPermanentCard fieldPermanentCard in gameContext.TurnPlayer.FieldPermanentObjects)
            {
                if (fieldPermanentCard.gameObject.activeSelf && fieldPermanentCard.ThisPermanent.CanDeclareSkill() || fieldPermanentCard.ThisPermanent.CanAttack(null))
                {
                    fieldPermanentCard.OnSelectEffect(1.1f);

                    #region Add click operation
                    fieldPermanentCard.AddClickTarget((_fieldUnitCard) => StartCoroutine(OnClick_Select()));

                    IEnumerator OnClick_Select()
                    {
                        if (isSync)
                        {
                            yield break;
                        }

                        IsSelecting = true;

                        #region Reset cards in other places
                        foreach (FieldPermanentCard _fieldPermanentCard in gameContext.TurnPlayer.FieldPermanentObjects)
                        {
                            if (_fieldPermanentCard != fieldPermanentCard)
                            {
                                _fieldPermanentCard.RemoveSelectEffect();
                            }
                        }

                        foreach (Player player in gameContext.Players)
                        {
                            OffFieldCardTarget(player);
                        }
                        #endregion

                        #region Reset cards in hand
                        foreach (HandCard handCard in gameContext.TurnPlayer.HandCardObjects)
                        {
                            handCard.RemoveSelectEffect();
                            handCard.RemoveClickTarget();
                            handCard.RemoveDragTarget();
                        }
                        #endregion

                        List<CardCommand> FieldUnitCommands = new List<CardCommand>();

                        #region activated effect
                        if (fieldPermanentCard.ThisPermanent.CanDeclareSkill())
                        {
                            List<ICardEffect> cardEffects = new List<ICardEffect>();
                            List<ICardEffect> cardEffects1 = new List<ICardEffect>();

                            foreach (ICardEffect cardEffect in fieldPermanentCard.ThisPermanent.EffectList(EffectTiming.OnDeclaration))
                            {
                                cardEffects1.Add(cardEffect);
                                cardEffects.Add(cardEffect);
                            }

                            cardEffects.Reverse();

                            foreach (ICardEffect cardEffect in cardEffects)
                            {
                                if (cardEffect is ActivateICardEffect)
                                {
                                    CardCommand SkillCommand = new CardCommand(cardEffect.EffectName, OnClick_SetUseSkillPermanent_RPC, cardEffect.CanUse(null), DataBase.CommandColor_Skill);
                                    FieldUnitCommands.Add(SkillCommand);

                                    void OnClick_SetUseSkillPermanent_RPC()
                                    {
                                        #region Reset cards on the field
                                        foreach (Player player in gameContext.Players)
                                        {
                                            foreach (FieldPermanentCard _fieldUnitCard in player.FieldPermanentObjects)
                                            {
                                                _fieldUnitCard.CloseCommandPanel();
                                                _fieldUnitCard.RemoveClickTarget();
                                                _fieldUnitCard.RemoveDragTarget();
                                                _fieldUnitCard.RemoveSelectEffect();
                                            }
                                        }
                                        #endregion

                                        photonView.RPC("SetActSkill", RpcTarget.All, fieldPermanentCard.ThisPermanent.TopCard.Owner.GetFieldPermanents().IndexOf(fieldPermanentCard.ThisPermanent), cardEffects1.IndexOf(cardEffect));
                                    }
                                }
                            }
                        }
                        #endregion

                        #region attack
                        if (fieldPermanentCard.ThisPermanent.IsDigimon)
                        {
                            CardCommand SkillCommand = new CardCommand("Attack", () => StartCoroutine(OnClick_SetAttack_RPC()), fieldPermanentCard.ThisPermanent.CanAttack(null), DataBase.CommandColor_Attack);
                            FieldUnitCommands.Add(SkillCommand);

                            IEnumerator OnClick_SetAttack_RPC()
                            {
                                if (fieldPermanentCard.ThisPermanent.CanAttack(null))
                                {
                                    #region Reset cards on the field
                                    foreach (Player player in gameContext.Players)
                                    {
                                        foreach (FieldPermanentCard _fieldUnitCard in player.FieldPermanentObjects)
                                        {
                                            if (_fieldUnitCard != fieldPermanentCard)
                                            {
                                                _fieldUnitCard.RemoveSelectEffect();
                                                _fieldUnitCard.RemoveDragTarget();
                                            }

                                            _fieldUnitCard.CloseCommandPanel();
                                            _fieldUnitCard.RemoveClickTarget();
                                        }
                                    }
                                    #endregion

                                    #region When only direct attack is available
                                    if (fieldPermanentCard.ThisPermanent.CanAttackTargetDigimon(null, null) && gameContext.NonTurnPlayer.GetBattleAreaDigimons().Count((permanent) => fieldPermanentCard.ThisPermanent.CanAttackTargetDigimon(permanent, null)) == 0)
                                    {
                                        if (fieldPermanentCard.ThisPermanent.CanAttackTargetDigimon(null, null))
                                        {
                                            bool doAttack = false;
                                            bool endSelect_doAttack = false;

                                            if (gameContext.NonTurnPlayer.SecurityCards.Count >= 1)
                                            {
                                                gameContext.NonTurnPlayer.securityObject.securityBreakGlass.ShowBlueMatarial();
                                            }

                                            gameContext.NonTurnPlayer.securityObject.SetSecurityAttackObject();
                                            gameContext.NonTurnPlayer.securityObject.SetSecurityOutline(true);

                                            gameContext.NonTurnPlayer.securityObject.AddClickTarget(() =>
                                            {
                                                doAttack = true;
                                                endSelect_doAttack = true;
                                            });

                                            GManager.instance.commandText.OpenCommandText($"Will you attack with {fieldPermanentCard.ThisPermanent.TopCard.BaseENGCardNameFromEntity}?");

                                            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                                    {
                                                        new Command_SelectCommand("Attack",() =>
                                                        {
                                                            doAttack = true;

                                                            GManager.instance.selectCommandPanel.CloseSelectCommandPanel();
                                                            GManager.instance.BackButton.CloseSelectCommandButton();

                                                            endSelect_doAttack = true;
                                                        },0),
                                                    };

                                            GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);

                                            GManager.instance.BackButton.OpenSelectCommandButton("Return", () =>
                                            {
                                                doAttack = false;

                                                GManager.instance.selectCommandPanel.CloseSelectCommandPanel();
                                                GManager.instance.BackButton.CloseSelectCommandButton();

                                                endSelect_doAttack = true;

                                            }, 0);

                                            yield return new WaitWhile(() => !endSelect_doAttack);
                                            endSelect_doAttack = false;

                                            GManager.instance.selectCommandPanel.Off();

                                            GManager.instance.commandText.CloseCommandText();
                                            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                                            gameContext.NonTurnPlayer.securityObject.RemoveClickTarget();
                                            GManager.instance.BackButton.CloseSelectCommandButton();

                                            if (doAttack)
                                            {
                                                photonView.RPC("SetAttackingPermaent", RpcTarget.All, fieldPermanentCard.ThisPermanent.TopCard.Owner.GetFieldPermanents().IndexOf(fieldPermanentCard.ThisPermanent), -1);
                                            }

                                            else
                                            {
                                                StartCoroutine(SetMainPhase());
                                            }
                                        }

                                    }
                                    #endregion

                                    #region When it is possible to attack characters on the field
                                    else if (gameContext.NonTurnPlayer.GetBattleAreaDigimons().Count((permanent) => fieldPermanentCard.ThisPermanent.CanAttackTargetDigimon(permanent, null)) >= 1)
                                    {
                                        bool doAttack = false;
                                        int attackTargetID = -1;
                                        bool endSelect_doAttack = false;

                                        GManager.instance.commandText.OpenCommandText($"Which target will you attack?");

                                        if (fieldPermanentCard.ThisPermanent.CanAttackTargetDigimon(null, null))
                                        {
                                            if (gameContext.NonTurnPlayer.SecurityCards.Count >= 1)
                                            {
                                                gameContext.NonTurnPlayer.securityObject.securityBreakGlass.ShowBlueMatarial();
                                            }

                                            gameContext.NonTurnPlayer.securityObject.SetSecurityAttackObject();
                                            gameContext.NonTurnPlayer.securityObject.SetSecurityOutline(true);

                                            gameContext.NonTurnPlayer.securityObject.AddClickTarget(() =>
                                            {
                                                doAttack = true;
                                                attackTargetID = -1;
                                                endSelect_doAttack = true;
                                            });
                                        }

                                        foreach (FieldPermanentCard enemyFieldPermanentCard in gameContext.NonTurnPlayer.FieldPermanentObjects)
                                        {
                                            if (fieldPermanentCard.ThisPermanent.CanAttackTargetDigimon(enemyFieldPermanentCard.ThisPermanent, null))
                                            {
                                                enemyFieldPermanentCard.AddClickTarget((_fieldUnitCard) => StartCoroutine(SelectDefender()));

                                                enemyFieldPermanentCard.OnSelectEffect(1.1f);

                                                IEnumerator SelectDefender()
                                                {
                                                    foreach (Player player in gameContext.Players_ForTurnPlayer)
                                                    {
                                                        foreach (FieldPermanentCard _fieldPermanentCard in gameContext.TurnPlayer.FieldPermanentObjects)
                                                        {
                                                            _fieldPermanentCard.RemoveSelectEffect();
                                                            _fieldPermanentCard.RemoveDragTarget();
                                                            _fieldPermanentCard.RemoveClickTarget();
                                                        }
                                                    }

                                                    GManager.instance.selectCommandPanel.CloseSelectCommandPanel();
                                                    GManager.instance.BackButton.CloseSelectCommandButton();

                                                    doAttack = true;
                                                    attackTargetID = enemyFieldPermanentCard.ThisPermanent.TopCard.Owner.GetFieldPermanents().IndexOf(enemyFieldPermanentCard.ThisPermanent);
                                                    endSelect_doAttack = true;

                                                    yield return null;
                                                }
                                            }
                                        }

                                        GManager.instance.BackButton.OpenSelectCommandButton("Return", () =>
                                        {
                                            doAttack = false;

                                            GManager.instance.selectCommandPanel.CloseSelectCommandPanel();
                                            GManager.instance.BackButton.CloseSelectCommandButton();

                                            endSelect_doAttack = true;

                                        }, 0);

                                        yield return new WaitWhile(() => !endSelect_doAttack);
                                        endSelect_doAttack = false;

                                        GManager.instance.selectCommandPanel.Off();
                                        gameContext.NonTurnPlayer.securityObject.OffShowSecurityAttackObject();
                                        GManager.instance.commandText.CloseCommandText();
                                        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                                        GManager.instance.BackButton.CloseSelectCommandButton();

                                        if (doAttack)
                                        {
                                            photonView.RPC("SetAttackingPermaent", RpcTarget.All, fieldPermanentCard.ThisPermanent.TopCard.Owner.GetFieldPermanents().IndexOf(fieldPermanentCard.ThisPermanent), attackTargetID);
                                        }

                                        else
                                        {
                                            StartCoroutine(SetMainPhase());
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                        #endregion

                        fieldPermanentCard.fieldUnitCommandPanel.SetUpCommandPanel(FieldUnitCommands, fieldPermanentCard, null);

                        fieldPermanentCard.AddClickTarget((_fieldUnitCard) => StartCoroutine(SetMainPhase()));

                        fieldPermanentCard.Outline_Select.gameObject.SetActive(true);
                        fieldPermanentCard.SetOrangeOutline();

                        yield return null;
                    }
                    #endregion

                    AddDragProcess(fieldPermanentCard);

                    #region ドラッグ操作を追加
                    void AddDragProcess(FieldPermanentCard fieldPermanentCard1)
                    {
                        if (fieldPermanentCard1.ThisPermanent.CanAttack(null))
                        {
                            fieldPermanentCard1.AddDragTarget(OnBeginDragAction, OnDragAction, OnEndDragAction);

                            #region ドラッグ開始
                            void OnBeginDragAction(FieldPermanentCard fieldPermanentCard2)
                            {
                                if (isSync)
                                {
                                    return;
                                }

                                if (gameContext.TurnPlayer.FieldPermanentObjects.Count((fieldPermanentCard3) => fieldPermanentCard3.fieldUnitCommandPanel.isActive()) == 0)
                                {
                                    #region 手札のカードをリセット
                                    foreach (HandCard handCard1 in gameContext.TurnPlayer.HandCardObjects)
                                    {
                                        handCard1.RemoveClickTarget();
                                        handCard1.RemoveDragTarget();
                                        handCard1.RemoveSelectEffect();
                                        handCard1.handCardCommandPanel.CloseCommandPanel();
                                        handCard1.Outline_Select.gameObject.SetActive(false);
                                    }

                                    foreach (Player player in gameContext.Players)
                                    {
                                        foreach (HandCard handCard2 in player.HandCardObjects)
                                        {
                                            handCard2.GetComponent<Draggable_HandCard>().CanPointerEnterExitAction = true;
                                        }
                                    }
                                    #endregion

                                    IsSelecting = true;

                                    GManager.instance.commandText.CloseCommandText();

                                    fieldPermanentCard2.CloseCommandPanel();

                                    TargetArrow targetArrow = GManager.instance.CreateTargetArrow();

                                    targetArrow.SetTargetArrow(fieldPermanentCard2.ThisPermanent.PermanentFrame.GetLocalCanvasPosition() + fieldPermanentCard2.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition, Draggable.GetLocalPosition(Input.mousePosition, targetArrow.transform));

                                    foreach (FieldPermanentCard fieldPermanentCard3 in gameContext.TurnPlayer.FieldPermanentObjects)
                                    {
                                        if (fieldPermanentCard3 != fieldPermanentCard2)
                                        {
                                            fieldPermanentCard3.RemoveSelectEffect();
                                            fieldPermanentCard3.RemoveClickTarget();
                                            fieldPermanentCard3.RemoveDragTarget();
                                        }
                                    }

                                    foreach (FieldPermanentCard enemyFieldPermanentCard in gameContext.NonTurnPlayer.FieldPermanentObjects)
                                    {
                                        if (fieldPermanentCard2.ThisPermanent.CanAttackTargetDigimon(enemyFieldPermanentCard.ThisPermanent, null))
                                        {
                                            enemyFieldPermanentCard.OnSelectEffect(1.1f);
                                            enemyFieldPermanentCard.SetBlueOutline();
                                        }
                                    }

                                    if (fieldPermanentCard1.ThisPermanent.CanAttackTargetDigimon(null, null))
                                    {
                                        if (gameContext.NonTurnPlayer.SecurityCards.Count >= 1)
                                        {
                                            gameContext.NonTurnPlayer.securityObject.securityBreakGlass.ShowTransparentMatarial();
                                        }

                                        gameContext.NonTurnPlayer.securityObject.SetSecurityAttackObject();
                                        gameContext.NonTurnPlayer.securityObject.SetSecurityOutline(false);
                                    }
                                }
                            }
                            #endregion

                            #region ドラッグ中
                            void OnDragAction(FieldPermanentCard fieldPermanentCard2, List<DropArea> dropAreas)
                            {
                                if (isSync)
                                {
                                    StartCoroutine(SetMainPhase());
                                }

                                fieldPermanentCard2.CloseCommandPanel();

                                TargetArrow targetArrow = null;

                                for (int i = 0; i < GManager.instance.targetArrowParent.childCount; i++)
                                {
                                    if (GManager.instance.targetArrowParent.GetChild(i).GetComponent<TargetArrow>() != null && GManager.instance.targetArrowParent.GetChild(i).gameObject.activeSelf)
                                    {
                                        targetArrow = GManager.instance.targetArrowParent.GetChild(i).GetComponent<TargetArrow>();
                                    }
                                }

                                if (targetArrow != null)
                                {
                                    targetArrow.SetTargetArrow(fieldPermanentCard2.ThisPermanent.PermanentFrame.GetLocalCanvasPosition() + fieldPermanentCard2.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition, Draggable.GetLocalPosition(Input.mousePosition, targetArrow.transform));

                                    foreach (FieldPermanentCard enemyFieldPermanentCard in GManager.instance.Opponent.FieldPermanentObjects)
                                    {
                                        if (fieldPermanentCard2.ThisPermanent.CanAttackTargetDigimon(enemyFieldPermanentCard.ThisPermanent, null))
                                        {
                                            bool OnSelect = false;

                                            if (dropAreas.Count((dropArea) => dropArea.IsChildThisDropArea(enemyFieldPermanentCard.gameObject)) > 0)
                                            {
                                                if (fieldPermanentCard2.ThisPermanent.CanAttackTargetDigimon(enemyFieldPermanentCard.ThisPermanent, null))
                                                {
                                                    OnSelect = true;
                                                }
                                            }

                                            if (OnSelect)
                                            {
                                                enemyFieldPermanentCard.OnSelectEffect(1.1f);
                                                enemyFieldPermanentCard.SetOrangeOutline();
                                            }

                                            else
                                            {
                                                enemyFieldPermanentCard.OnSelectEffect(1.1f);
                                                enemyFieldPermanentCard.SetBlueOutline();
                                            }
                                        }
                                    }

                                    if (fieldPermanentCard2.ThisPermanent.CanAttackTargetDigimon(null, null))
                                    {
                                        if (dropAreas.Count((dropArea) => dropArea.IsChildThisDropArea(gameContext.NonTurnPlayer.securityObject.securityAttackDropArea.gameObject)) > 0)
                                        {
                                            if (gameContext.NonTurnPlayer.SecurityCards.Count >= 1)
                                            {
                                                gameContext.NonTurnPlayer.securityObject.securityBreakGlass.ShowBlueMatarial();
                                            }

                                            gameContext.NonTurnPlayer.securityObject.SetSecurityAttackObject();
                                            gameContext.NonTurnPlayer.securityObject.SetSecurityOutline(true);
                                        }

                                        else
                                        {
                                            if (gameContext.NonTurnPlayer.SecurityCards.Count >= 1)
                                            {
                                                gameContext.NonTurnPlayer.securityObject.securityBreakGlass.ShowTransparentMatarial();
                                            }

                                            gameContext.NonTurnPlayer.securityObject.SetSecurityAttackObject();
                                            gameContext.NonTurnPlayer.securityObject.SetSecurityOutline(false);
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region ドラッグ終了
                            void OnEndDragAction(FieldPermanentCard fieldPermanentCard2, List<DropArea> dropAreas)
                            {
                                if (isSync)
                                {
                                    StartCoroutine(SetMainPhase());
                                }

                                IsSelecting = false;

                                TargetArrow targetArrow = null;

                                for (int i = 0; i < GManager.instance.targetArrowParent.childCount; i++)
                                {
                                    if (GManager.instance.targetArrowParent.GetChild(i).GetComponent<TargetArrow>() != null && GManager.instance.targetArrowParent.GetChild(i).gameObject.activeSelf)
                                    {
                                        targetArrow = GManager.instance.targetArrowParent.GetChild(i).GetComponent<TargetArrow>();
                                    }
                                }

                                if (targetArrow != null)
                                {
                                    #region 手札のカードをリセット
                                    foreach (HandCard handCard1 in gameContext.TurnPlayer.HandCardObjects)
                                    {
                                        handCard1.RemoveClickTarget();
                                    }

                                    foreach (Player player in gameContext.Players)
                                    {
                                        foreach (HandCard handCard2 in player.HandCardObjects)
                                        {
                                            handCard2.GetComponent<Draggable_HandCard>().CanPointerEnterExitAction = true;
                                        }
                                    }
                                    #endregion

                                    GManager.instance.You.playMatCardFrame.OffFrame_Select();

                                    fieldPermanentCard2.CloseCommandPanel();

                                    Destroy(targetArrow.gameObject);

                                    foreach (DropArea dropArea in dropAreas)
                                    {
                                        foreach (FieldPermanentCard enemyFieldPermanentCard in GManager.instance.Opponent.FieldPermanentObjects)
                                        {
                                            if (dropArea.IsChildThisDropArea(enemyFieldPermanentCard.gameObject))
                                            {
                                                if (fieldPermanentCard2.ThisPermanent.CanAttackTargetDigimon(enemyFieldPermanentCard.ThisPermanent, null))
                                                {
                                                    #region Reset field cards
                                                    foreach (Player player in gameContext.Players)
                                                    {
                                                        foreach (FieldPermanentCard fieldPermanentCard3 in player.FieldPermanentObjects)
                                                        {
                                                            if (fieldPermanentCard3 != fieldPermanentCard2 && fieldPermanentCard3 != enemyFieldPermanentCard)
                                                            {
                                                                fieldPermanentCard3.RemoveSelectEffect();
                                                            }

                                                            fieldPermanentCard3.CloseCommandPanel();
                                                            fieldPermanentCard3.RemoveClickTarget();
                                                            fieldPermanentCard3.RemoveDragTarget();
                                                        }
                                                    }
                                                    #endregion

                                                    //gameContext.NonTurnPlayer.LifeCardFrame.OffFrame_Select();

                                                    photonView.RPC("SetAttackingPermaent", RpcTarget.All, gameContext.TurnPlayer.GetFieldPermanents().IndexOf(fieldPermanentCard2.ThisPermanent), gameContext.NonTurnPlayer.GetFieldPermanents().IndexOf(enemyFieldPermanentCard.ThisPermanent));
                                                    return;
                                                }
                                            }
                                        }

                                        if (fieldPermanentCard2.ThisPermanent.CanAttackTargetDigimon(null, null))
                                        {
                                            if (dropArea.IsChildThisDropArea(gameContext.NonTurnPlayer.securityObject.securityAttackDropArea.gameObject))
                                            {
                                                #region Reset field cards
                                                foreach (Player player in gameContext.Players)
                                                {
                                                    foreach (FieldPermanentCard fieldPermanentCard3 in player.FieldPermanentObjects)
                                                    {
                                                        if (fieldPermanentCard3 != fieldPermanentCard2)
                                                        {
                                                            fieldPermanentCard3.RemoveSelectEffect();
                                                        }

                                                        fieldPermanentCard3.CloseCommandPanel();
                                                        fieldPermanentCard3.RemoveClickTarget();
                                                        fieldPermanentCard3.RemoveDragTarget();
                                                    }
                                                }
                                                #endregion

                                                photonView.RPC("SetAttackingPermaent", RpcTarget.All, gameContext.TurnPlayer.GetFieldPermanents().IndexOf(fieldPermanentCard2.ThisPermanent), -1);
                                                return;
                                            }
                                        }
                                    }

                                    StartCoroutine(SetMainPhase());
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
            }
            #endregion

            #region cards in hand
            foreach (HandCard handCard in gameContext.TurnPlayer.HandCardObjects)
            {
                #region drag and play
                if (handCard.cardSource.CanPlayFromHandDuringMainPhase)
                {
                    handCard.SetBlueOutline();

                    handCard.AddDragTarget(BeginDrag, OnDropCard, OnDragCard);

                    #region At the start of drag
                    void BeginDrag(HandCard handCard1)
                    {
                        if (isSync)
                        {
                            //return;
                        }

                        if (gameContext.TurnPlayer.FieldPermanentObjects.Count((_fieldPermanentCard1) => _fieldPermanentCard1.fieldUnitCommandPanel.isActive()) == 0)
                        {
                            IsSelecting = true;

                            OffFieldCardTarget(gameContext.TurnPlayer);

                            foreach (HandCard handCard2 in gameContext.TurnPlayer.HandCardObjects)
                            {
                                handCard2.RemoveOnClickAction();

                                if (handCard2 != handCard1)
                                {
                                    handCard2.RemoveSelectEffect();
                                }
                            }

                            foreach (FieldPermanentCard fieldPermanentCard in GManager.instance.You.FieldPermanentObjects)
                            {
                                fieldPermanentCard.RemoveSelectEffect();
                                fieldPermanentCard.RemoveDragTarget();
                                fieldPermanentCard.Outline_Select.gameObject.SetActive(false);
                            }

                            foreach (FieldCardFrame fieldCardFrame in GManager.instance.You.fieldCardFrames)
                            {
                                fieldCardFrame.OffFrame_Select();
                            }

                            foreach (Player player in gameContext.Players_ForTurnPlayer)
                            {
                                foreach (FieldCardFrame fieldCardFrame in player.fieldCardFrames)
                                {
                                    fieldCardFrame.AddClickTarget(null);
                                }
                            }

                            GManager.instance.You.playMatCardFrame.AddClickTarget(null);
                            GManager.instance.You.playMatCardFrame.OffFrame_Select();

                            handCard1.SetBlueOutline();
                            handCard1.OffPlayText();
                            handCard1.OffJogressPlayText();
                            handCard1.OffBurstPlayText();
                            handCard1.OffAppFusionPlayText();
                            handCard1.OffClickText();

                            #region デジモン・テイマー
                            if (handCard1.cardSource.IsPermanent)
                            {
                                bool CanPlayEmptyFrame = false;

                                foreach (FieldCardFrame fieldCardFrame in GManager.instance.You.fieldCardFrames)
                                {
                                    if (fieldCardFrame.IsEmptyFrame())
                                    {
                                        if (handCard1.cardSource.CanPlayCardTargetFrame(fieldCardFrame, true, null))
                                        {
                                            CanPlayEmptyFrame = true;
                                            break;
                                        }
                                    }
                                }

                                if (CanPlayEmptyFrame)
                                {
                                    GManager.instance.You.playMatCardFrame.Frame.transform.parent.gameObject.SetActive(true);
                                    GManager.instance.You.playMatCardFrame.OnFrame_Select(DataBase.SelectColor_Blue);
                                }

                                foreach (FieldCardFrame fieldCardFrame in GManager.instance.You.fieldCardFrames)
                                {
                                    if (handCard1.cardSource.CanPlayCardTargetFrame(fieldCardFrame, true, null) || handCard1.cardSource.CanJogressFromTargetPermanent(fieldCardFrame.GetFramePermanent(), true) || handCard1.cardSource.CanBurstDigivolutionFromTargetPermanent(fieldCardFrame.GetFramePermanent(), true))
                                    {
                                        if (fieldCardFrame.GetFramePermanent() != null)
                                        {
                                            if (fieldCardFrame.GetFramePermanent().ShowingPermanentCard != null)
                                            {
                                                fieldCardFrame.GetFramePermanent().ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
                                                fieldCardFrame.GetFramePermanent().ShowingPermanentCard.SetBlueOutline();
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region オプション
                            else if (handCard1.cardSource.IsOption)
                            {
                                GManager.instance.You.playMatCardFrame.Frame.transform.parent.gameObject.SetActive(true);
                                GManager.instance.You.playMatCardFrame.OnFrame_Select(DataBase.SelectColor_Blue);
                            }
                            #endregion

                            // check playablity
                            ContinuousController.instance.StartCoroutine(SetHandCardPlayablity(handCard.cardSource));
                        }
                    }
                    #endregion

                    #region At the end of drag
                    void OnDropCard(List<DropArea> dropAreas)
                    {
                        if (isSync)
                        {
                            StartCoroutine(Return());
                        }

                        if (gameContext.TurnPlayer.FieldPermanentObjects.Count((_fieldPermanentCard1) => _fieldPermanentCard1.fieldUnitCommandPanel.isActive()) == 0)
                        {
                            foreach (FieldPermanentCard fieldPermanentCard in gameContext.TurnPlayer.FieldPermanentObjects)
                            {
                                fieldPermanentCard.RemoveSelectEffect();
                                fieldPermanentCard.RemoveDragTarget();
                                fieldPermanentCard.Outline_Select.gameObject.SetActive(false);
                            }

                            foreach (FieldCardFrame fieldCardFrame in GManager.instance.You.fieldCardFrames)
                            {
                                fieldCardFrame.OffFrame_Select();
                            }

                            GManager.instance.You.playMatCardFrame.OffFrame_Select();
                            handCard.OffPlayText();
                            handCard.OffJogressPlayText();
                            handCard.OffBurstPlayText();
                            handCard.OffAppFusionPlayText();

                            #region play cards

                            bool isOnHand = dropAreas.Count((dropArea) => dropArea.IsChildThisDropArea(GManager.instance.You.HandDropArea.gameObject)) > 0;
                            bool selected = false;

                            #region If it is not dropped in the hand area
                            if (!isOnHand)
                            {
                                #region character field
                                if (handCard.cardSource.IsPermanent)
                                {
                                    #region Check if it is dropped in the frame
                                    foreach (FieldCardFrame fieldCardFrame in GManager.instance.You.fieldCardFrames)
                                    {
                                        if (handCard.cardSource.CanPlayCardTargetFrame(fieldCardFrame, true, null) || handCard.cardSource.CanJogressFromTargetPermanent(fieldCardFrame.GetFramePermanent(), true) || handCard.cardSource.CanBurstDigivolutionFromTargetPermanent(fieldCardFrame.GetFramePermanent(), true) || handCard.cardSource.CanAppFusionFromTargetPermanent(fieldCardFrame.GetFramePermanent(), true))
                                        {
                                            if (dropAreas.Count((dropArea) => dropArea.IsChildThisDropArea(fieldCardFrame.Frame)) > 0)
                                            {
                                                if (fieldCardFrame.GetFramePermanent() != null)
                                                {
                                                    if (handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>() != null)
                                                    {
                                                        handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging = true;
                                                    }

                                                    OffHandCardTarget(gameContext.TurnPlayer);

                                                    handCard.Outline_Select.gameObject.SetActive(false);

                                                    foreach (Player player in gameContext.Players_ForTurnPlayer)
                                                    {
                                                        foreach (FieldCardFrame fieldCardFrame1 in player.fieldCardFrames)
                                                        {
                                                            fieldCardFrame1.RemoveClickTarget();
                                                        }
                                                    }

                                                    GManager.instance.You.playMatCardFrame.RemoveClickTarget();
                                                    GManager.instance.You.playMatCardFrame.Frame.transform.parent.gameObject.SetActive(false);

                                                    selected = true;

                                                    Permanent targetPermanent = fieldCardFrame.GetFramePermanent();

                                                    bool canNormalDigivolution = handCard.cardSource.CanPlayCardTargetFrame(fieldCardFrame, true, null);
                                                    bool canJogressDigivolution = handCard.cardSource.CanJogressFromTargetPermanent(fieldCardFrame.GetFramePermanent(), true);
                                                    bool canBurstDigivolution = handCard.cardSource.CanBurstDigivolutionFromTargetPermanent(fieldCardFrame.GetFramePermanent(), true);
                                                    bool canAppFusion = handCard.cardSource.CanAppFusionFromTargetPermanent(fieldCardFrame.GetFramePermanent(), true);

                                                    //Only normal evolution possible
                                                    if (canNormalDigivolution && !canJogressDigivolution && !canBurstDigivolution && !canAppFusion)
                                                    {
                                                        Digivolution();
                                                    }

                                                    //Only jogless is possible
                                                    else if (!canNormalDigivolution && canJogressDigivolution && !canBurstDigivolution && !canAppFusion)
                                                    {
                                                        SelectJogressDigivolutionCards(true);
                                                    }

                                                    //Only burst evolution possible
                                                    else if (!canNormalDigivolution && !canJogressDigivolution && canBurstDigivolution && !canAppFusion)
                                                    {
                                                        SelectBurstDigivolutionCards(true);
                                                    }

                                                    //Only app fusion possible
                                                    else if (!canNormalDigivolution && !canJogressDigivolution && !canBurstDigivolution && canAppFusion)
                                                    {
                                                        SelectAppFusionCards(true);
                                                    }

                                                    //Normal evolution and Jogress possible
                                                    else if (canNormalDigivolution && canJogressDigivolution && !canBurstDigivolution && !canAppFusion)
                                                    {
                                                        Vector3 Pos = handCard.transform.position;

                                                        ResetUI();

                                                        handCard.transform.GetChild(0).gameObject.SetActive(false);

                                                        handCard.GetComponent<Draggable_HandCard>().ReturnDefaultPosition();

                                                        StartCoroutine(SelectWheterToJogress());

                                                        IEnumerator SelectWheterToJogress()
                                                        {
                                                            isSync = true;

                                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect_SetPosition(handCard.cardSource, Pos));
                                                            handCard.transform.position = handCard.cardSource.Owner.brainStormObject.BrainStormHandCards[0].transform.position;

                                                            GManager.instance.selectJogressEffect.SetUp_SelectWheterToJogress
                                                                (card: handCard.cardSource,
                                                                evoRoot: fieldCardFrame.GetFramePermanent().TopCard,
                                                                canNoSelect: true,
                                                                endSelectCoroutine_Digivolve: _Digivolution,
                                                                endSelectCoroutine_Jogress: _SelectJogressDigivolutionCards,
                                                                noSelectCoroutine: _NoSelectCoroutine);

                                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectJogressEffect.SelectWheterToJogress());

                                                            IEnumerator _Digivolution()
                                                            {
                                                                yield return null;
                                                                Digivolution();
                                                            }

                                                            IEnumerator _SelectJogressDigivolutionCards()
                                                            {
                                                                yield return null;
                                                                SelectJogressDigivolutionCards(false);
                                                            }

                                                            IEnumerator _NoSelectCoroutine()
                                                            {
                                                                yield return StartCoroutine(Return());
                                                            }
                                                        }
                                                    }

                                                    //Normal evolution and burst evolution possible
                                                    else if (canNormalDigivolution && !canJogressDigivolution && canBurstDigivolution && !canAppFusion)
                                                    {
                                                        Vector3 Pos = handCard.transform.position;

                                                        ResetUI();

                                                        handCard.transform.GetChild(0).gameObject.SetActive(false);

                                                        handCard.GetComponent<Draggable_HandCard>().ReturnDefaultPosition();

                                                        StartCoroutine(SelectWheterToBurst());

                                                        IEnumerator SelectWheterToBurst()
                                                        {
                                                            isSync = true;

                                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect_SetPosition(handCard.cardSource, Pos));
                                                            handCard.transform.position = handCard.cardSource.Owner.brainStormObject.BrainStormHandCards[0].transform.position;

                                                            GManager.instance.selectBurstDigivolutionEffect.SetUp_SelectWheterToBurst
                                                                (card: handCard.cardSource,
                                                                evoRoot: fieldCardFrame.GetFramePermanent().TopCard,
                                                                canNoSelect: true,
                                                                endSelectCoroutine_Digivolve: _Digivolution,
                                                                endSelectCoroutine_Burst: _SelectBurstPermanents,
                                                                noSelectCoroutine: _NoSelectCoroutine);

                                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectBurstDigivolutionEffect.SelectWheterToBurst());

                                                            IEnumerator _Digivolution()
                                                            {
                                                                yield return null;
                                                                Digivolution();
                                                            }

                                                            IEnumerator _SelectBurstPermanents()
                                                            {
                                                                yield return null;
                                                                SelectBurstDigivolutionCards(false);
                                                            }

                                                            IEnumerator _NoSelectCoroutine()
                                                            {
                                                                yield return StartCoroutine(Return());
                                                            }
                                                        }
                                                    }
                                                    //Normal evolution and App Fusion possible
                                                    else if (canNormalDigivolution && !canJogressDigivolution && !canBurstDigivolution && canAppFusion)
                                                    {
                                                        Vector3 Pos = handCard.transform.position;

                                                        ResetUI();

                                                        handCard.transform.GetChild(0).gameObject.SetActive(false);

                                                        handCard.GetComponent<Draggable_HandCard>().ReturnDefaultPosition();

                                                        StartCoroutine(SelectWheterToAppFusion());

                                                        IEnumerator SelectWheterToAppFusion()
                                                        {
                                                            isSync = true;

                                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect_SetPosition(handCard.cardSource, Pos));
                                                            handCard.transform.position = handCard.cardSource.Owner.brainStormObject.BrainStormHandCards[0].transform.position;

                                                            GManager.instance.selectAppFusionEffect.SetUp_SelectWheterToAppFusion
                                                                (card: handCard.cardSource,
                                                                evoRoot: fieldCardFrame.GetFramePermanent().TopCard,
                                                                canNoSelect: true,
                                                                endSelectCoroutine_Digivolve: _Digivolution,
                                                                endSelectCoroutine_AppFusion: _SelectAppFusionPermanents,
                                                                noSelectCoroutine: _NoSelectCoroutine);

                                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectAppFusionEffect.SelectWheterToAppFusion());

                                                            IEnumerator _Digivolution()
                                                            {
                                                                yield return null;
                                                                Digivolution();
                                                            }

                                                            IEnumerator _SelectAppFusionPermanents()
                                                            {
                                                                yield return null;
                                                                SelectAppFusionCards(false);
                                                            }

                                                            IEnumerator _NoSelectCoroutine()
                                                            {
                                                                yield return StartCoroutine(Return());
                                                            }
                                                        }
                                                    }

                                                    #region Select Jogress evolution source
                                                    void SelectJogressDigivolutionCards(bool move)
                                                    {
                                                        Vector3 Pos = handCard.transform.position;

                                                        ResetUI();

                                                        handCard.transform.GetChild(0).gameObject.SetActive(false);

                                                        StartCoroutine(SelectJogressTarget());

                                                        IEnumerator SelectJogressTarget()
                                                        {
                                                            isSync = true;

                                                            if (move)
                                                            {
                                                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect_SetPosition(handCard.cardSource, Pos));
                                                                handCard.transform.position = handCard.cardSource.Owner.brainStormObject.BrainStormHandCards[0].transform.position;
                                                            }

                                                            yield return ContinuousController.instance.StartCoroutine(handCard.cardSource.Owner.brainStormObject.BrainStormCoroutine(handCard.cardSource));

                                                            GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                                                (card: handCard.cardSource,
                                                                isLocal: true,
                                                                isPayCost: true,
                                                                canNoSelect: true,
                                                                endSelectCoroutine_SelectDigivolutionRoots: _EndSelectCoroutine_SelectDigivolutionRoots,
                                                                noSelectCoroutine: _NoSelectCoroutine);

                                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectJogressEffect.SelectDigivolutionRoots());

                                                            IEnumerator _EndSelectCoroutine_SelectDigivolutionRoots(List<Permanent> permanents)
                                                            {
                                                                yield return null;
                                                                photonView.RPC("SetPlayCard", RpcTarget.All, handCard.cardSource.CardIndex, fieldCardFrame.FrameID, new int[] { permanents[0].PermanentFrame.FrameID, permanents[1].PermanentFrame.FrameID }, -1, new int[0]);
                                                            }

                                                            IEnumerator _NoSelectCoroutine()
                                                            {
                                                                yield return null;
                                                                yield return StartCoroutine(Return());
                                                            }

                                                            isSync = false;
                                                        }
                                                    }
                                                    #endregion

                                                    #region Select the burst evolution source and tamer
                                                    void SelectBurstDigivolutionCards(bool move)
                                                    {
                                                        Vector3 Pos = handCard.transform.position;

                                                        ResetUI();

                                                        handCard.transform.GetChild(0).gameObject.SetActive(false);

                                                        StartCoroutine(SelectJogressTarget());

                                                        IEnumerator SelectJogressTarget()
                                                        {
                                                            isSync = true;

                                                            if (move)
                                                            {
                                                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect_SetPosition(handCard.cardSource, Pos));
                                                                handCard.transform.position = handCard.cardSource.Owner.brainStormObject.BrainStormHandCards[0].transform.position;
                                                            }

                                                            yield return ContinuousController.instance.StartCoroutine(handCard.cardSource.Owner.brainStormObject.BrainStormCoroutine(handCard.cardSource));

                                                            GManager.instance.selectBurstDigivolutionEffect.SetUp_SelectTamer
                                                                (card: handCard.cardSource,
                                                                isLocal: true,
                                                                isPayCost: true,
                                                                canNoSelect: true,
                                                                endSelectCoroutine_SelectTamer: EndSelectCoroutine_SelectTamer,
                                                                noSelectCoroutine: _NoSelectCoroutine);


                                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectBurstDigivolutionEffect.SelectTamer());

                                                            IEnumerator EndSelectCoroutine_SelectTamer(Permanent permanent)
                                                            {
                                                                yield return null;
                                                                photonView.RPC("SetPlayCard", RpcTarget.All, handCard.cardSource.CardIndex, fieldCardFrame.FrameID, new int[0], permanent.PermanentFrame.FrameID, new int[0]);
                                                            }

                                                            IEnumerator _NoSelectCoroutine()
                                                            {
                                                                yield return null;
                                                                yield return StartCoroutine(Return());
                                                            }

                                                            isSync = false;
                                                        }
                                                    }
                                                    #endregion

                                                    #region Select the app fusion source and link card
                                                    void SelectAppFusionCards(bool move)
                                                    {
                                                        Vector3 Pos = handCard.transform.position;

                                                        ResetUI();

                                                        handCard.transform.GetChild(0).gameObject.SetActive(false);

                                                        StartCoroutine(SelectAppFusionTarget());

                                                        IEnumerator SelectAppFusionTarget()
                                                        {
                                                            isSync = true;

                                                            if (move)
                                                            {
                                                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect_SetPosition(handCard.cardSource, Pos));
                                                                handCard.transform.position = handCard.cardSource.Owner.brainStormObject.BrainStormHandCards[0].transform.position;
                                                            }

                                                            yield return ContinuousController.instance.StartCoroutine(handCard.cardSource.Owner.brainStormObject.BrainStormCoroutine(handCard.cardSource));
                
                                                            GManager.instance.selectAppFusionEffect.SetUp_SelectLink
                                                                (card: handCard.cardSource,
                                                                isLocal: true,
                                                                isPayCost: true,
                                                                canNoSelect: true,
                                                                endSelectCoroutine_SelectLink: EndSelectCoroutine_SelectLink,
                                                                noSelectCoroutine: _NoSelectCoroutine);

                                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectAppFusionEffect.SelectLink(targetPermanent));

                                                            IEnumerator EndSelectCoroutine_SelectLink(CardSource cardSource)
                                                            {
                                                                yield return null;

                                                                //int sourceIndex = fieldCardFrame.GetFramePermanent().LinkedCards.IndexOf(cardSource);
                                                                photonView.RPC("SetPlayCard", RpcTarget.All, handCard.cardSource.CardIndex, fieldCardFrame.FrameID, new int[0], -1, new int[] { targetPermanent.PermanentFrame.FrameID, targetPermanent.LinkedCards.IndexOf(cardSource)});
                                                            }

                                                            IEnumerator _NoSelectCoroutine()
                                                            {
                                                                yield return null;
                                                                yield return StartCoroutine(Return());
                                                            }

                                                            isSync = false;
                                                        }
                                                    }
                                                    #endregion

                                                    #region usually evolves
                                                    void Digivolution()
                                                    {
                                                        photonView.RPC("SetPlayCard", RpcTarget.All, handCard.cardSource.CardIndex, fieldCardFrame.FrameID, new int[0], -1, new int[0]);
                                                    }
                                                    #endregion

                                                    return;
                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    bool CanPlayEmptyFrame = false;

                                    foreach (FieldCardFrame fieldCardFrame in GManager.instance.You.fieldCardFrames)
                                    {
                                        if (fieldCardFrame.IsEmptyFrame())
                                        {
                                            if (handCard.cardSource.CanPlayCardTargetFrame(fieldCardFrame, true, null))
                                            {
                                                CanPlayEmptyFrame = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (CanPlayEmptyFrame)
                                    {
                                        #region Check for drops on the playmat
                                        if (dropAreas.Count((dropArea) => dropArea.IsChildThisDropArea(GManager.instance.You.playMatCardFrame.Frame)) > 0)
                                        {
                                            if (handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>() != null)
                                            {
                                                handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging = true;
                                            }

                                            OffHandCardTarget(gameContext.TurnPlayer);

                                            handCard.Outline_Select.gameObject.SetActive(false);

                                            foreach (Player player in gameContext.Players_ForTurnPlayer)
                                            {
                                                foreach (FieldCardFrame fieldCardFrame in player.fieldCardFrames)
                                                {
                                                    fieldCardFrame.RemoveClickTarget();
                                                }
                                            }

                                            GManager.instance.You.playMatCardFrame.RemoveClickTarget();
                                            GManager.instance.You.playMatCardFrame.Frame.transform.parent.gameObject.SetActive(false);

                                            photonView.RPC("SetPlayCard", RpcTarget.All, handCard.cardSource.CardIndex, handCard.cardSource.PreferredFrame().FrameID, new int[0], -1, new int[0]);
                                            selected = true;

                                            return;
                                        }
                                        #endregion
                                    }

                                }
                                #endregion

                                #region option
                                else if (handCard.cardSource.IsOption)
                                {
                                    #region Check for drops on the playmat
                                    if (dropAreas.Count((dropArea) => dropArea.IsChildThisDropArea(GManager.instance.You.playMatCardFrame.Frame)) > 0)
                                    {
                                        if (handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>() != null)
                                        {
                                            handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging = true;
                                        }

                                        OffHandCardTarget(gameContext.TurnPlayer);

                                        handCard.Outline_Select.gameObject.SetActive(false);

                                        foreach (Player player in gameContext.Players_ForTurnPlayer)
                                        {
                                            foreach (FieldCardFrame fieldCardFrame in player.fieldCardFrames)
                                            {
                                                fieldCardFrame.RemoveClickTarget();
                                            }
                                        }

                                        GManager.instance.You.playMatCardFrame.RemoveClickTarget();
                                        GManager.instance.You.playMatCardFrame.Frame.transform.parent.gameObject.SetActive(false);

                                        photonView.RPC("SetPlayCard", RpcTarget.All, handCard.cardSource.CardIndex, 0, new int[0], -1, new int[0]);
                                        selected = true;

                                        return;
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            #endregion

                            #endregion

                            if (!selected)
                            {
                                StartCoroutine(Return());
                            }
                        }
                    }
                    #endregion

                    #region While dragging
                    void OnDragCard(List<DropArea> dropAreas)
                    {
                        if (isSync)
                        {
                            StartCoroutine(Return());
                        }

                        GManager.instance.memoryObject.OffMemoryPredictionLine();

                        if (gameContext.TurnPlayer.FieldPermanentObjects.Count((_fieldUnitCard1) => _fieldUnitCard1.fieldUnitCommandPanel.isActive()) == 0)
                        {
                            OffFieldCardTarget(gameContext.TurnPlayer);

                            foreach (FieldPermanentCard fieldPermanentCard in gameContext.TurnPlayer.FieldPermanentObjects)
                            {
                                fieldPermanentCard.RemoveDragTarget();
                                fieldPermanentCard.Outline_Select.gameObject.SetActive(false);
                            }

                            handCard.SetBlueOutline();
                            handCard.OffPlayText();
                            handCard.OffJogressPlayText();
                            handCard.OffBurstPlayText();
                            handCard.OffAppFusionPlayText();

                            #region Digimon/Tamer
                            if (handCard.cardSource.IsPermanent)
                            {
                                #region Check if it is on the frame
                                bool isOnPermanentFrameAndCanEvolve = false;

                                foreach (FieldCardFrame fieldCardFrame in GManager.instance.You.fieldCardFrames)
                                {
                                    int frameIndex = GManager.instance.You.fieldCardFrames.IndexOf(fieldCardFrame);

                                    if (_canPlayTargetFrames[frameIndex])
                                    {
                                        if (fieldCardFrame.GetFramePermanent() != null)
                                        {
                                            if (fieldCardFrame.GetFramePermanent().ShowingPermanentCard != null)
                                            {
                                                if (dropAreas.Count((dropArea) => dropArea.IsChildThisDropArea(fieldCardFrame.Frame)) > 0)
                                                {
                                                    isOnPermanentFrameAndCanEvolve = true;

                                                    if (_canDigivolves[frameIndex])
                                                    {
                                                        handCard.SetPlayText("DIGIVOLVE", new Color32(255, 135, 8, 255));
                                                    }

                                                    if (_canJogresses[frameIndex])
                                                    {
                                                        handCard.SetJogressPlayText();
                                                    }

                                                    if (_canBursts[frameIndex])
                                                    {
                                                        handCard.SetBurstPlayText();
                                                    }

                                                    if (_canAppFusions[frameIndex])
                                                        handCard.SetAppFusionPlayText();

                                                    handCard.SetOrangeOutline();

                                                    fieldCardFrame.GetFramePermanent().ShowingPermanentCard.OnSelectEffect(1.1f);

                                                    fieldCardFrame.GetFramePermanent().ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);

                                                    GManager.instance.memoryObject.ShowMemoryPredictionLine(gameContext.TurnPlayer.ExpectedMemory(_payingCosts[frameIndex]));
                                                }

                                                else
                                                {
                                                    fieldCardFrame.GetFramePermanent().ShowingPermanentCard.RemoveSelectEffect();
                                                    fieldCardFrame.GetFramePermanent().ShowingPermanentCard.SetBlueOutline();

                                                    fieldCardFrame.GetFramePermanent().ShowingPermanentCard.Outline_Select.gameObject.SetActive(true);
                                                }
                                            }
                                        }
                                    }
                                }

                                if (_canPlayEmptyFrame)
                                {
                                    GManager.instance.You.playMatCardFrame.Frame.transform.parent.gameObject.SetActive(true);

                                    #region プレイマットの上にあるかチェック
                                    if (!isOnPermanentFrameAndCanEvolve && dropAreas.Count((dropArea) =>
                                    dropArea.IsChildThisDropArea(GManager.instance.You.playMatCardFrame.Frame)) > 0)
                                    {
                                        handCard.SetPlayText("PLAY", new Color32(47, 255, 64, 255));

                                        handCard.SetOrangeOutline();

                                        GManager.instance.You.playMatCardFrame.OnFrame_Select(DataBase.SelectColor_Orange);

                                        GManager.instance.memoryObject.ShowMemoryPredictionLine(gameContext.TurnPlayer.ExpectedMemory(handCard.cardSource.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false)));
                                    }

                                    else
                                    {
                                        GManager.instance.You.playMatCardFrame.OffFrame_Select();
                                    }
                                    #endregion
                                }
                                #endregion
                            }
                            #endregion

                            #region option
                            else if (handCard.cardSource.IsOption)
                            {
                                #region プレイマットの上にあるかチェック
                                if (dropAreas.Count((dropArea) => dropArea.IsChildThisDropArea(GManager.instance.You.playMatCardFrame.Frame)) > 0)
                                {
                                    handCard.SetPlayText("USE", new Color32(47, 255, 64, 255));

                                    handCard.SetOrangeOutline();

                                    GManager.instance.You.playMatCardFrame.OnFrame_Select(DataBase.SelectColor_Orange);

                                    GManager.instance.memoryObject.ShowMemoryPredictionLine(gameContext.TurnPlayer.ExpectedMemory(handCard.cardSource.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false)));
                                }

                                else
                                {
                                    GManager.instance.You.playMatCardFrame.OffFrame_Select();
                                }
                                #endregion
                            }
                            #endregion
                        }
                    }
                    #endregion

                    IEnumerator Return()
                    {
                        foreach (Player player in gameContext.Players_ForTurnPlayer)
                        {
                            foreach (FieldCardFrame fieldCardFrame in player.fieldCardFrames)
                            {
                                fieldCardFrame.RemoveClickTarget();
                            }

                            player.brainStormObject.EndBrainStorm();
                        }

                        GManager.instance.You.playMatCardFrame.RemoveClickTarget();

                        handCard.GetComponent<Draggable_HandCard>().ReturnDefaultPosition();
                        handCard.transform.GetChild(0).gameObject.SetActive(true);

                        if (handCard.transform.parent != null)
                        {
                            if (handCard.transform.parent.GetComponent<GridLayoutGroup>() != null)
                            {
                                handCard.transform.parent.GetComponent<GridLayoutGroup>().enabled = false;
                            }
                        }

                        yield return new WaitForSeconds(Time.deltaTime);

                        if (handCard.transform.parent != null)
                        {
                            if (handCard.transform.parent.GetComponent<GridLayoutGroup>() != null)
                            {
                                handCard.transform.parent.GetComponent<GridLayoutGroup>().enabled = true;
                            }
                        }

                        isSync = false;
                        StartCoroutine(SetMainPhase());
                    }
                }
                #endregion

                #region Click to declare activation effect
                if (handCard.cardSource.CanDeclareSkill)
                {
                    handCard.SetOrangeOutline();

                    handCard.SetClickText();

                    handCard.AddClickTarget((_handCard) => StartCoroutine(OnClick_Select()));

                    IEnumerator OnClick_Select()
                    {
                        foreach (HandCard handCard2 in handCard.cardSource.Owner.HandCardObjects)
                        {
                            handCard2.GetComponent<Draggable_HandCard>().CanPointerEnterExitAction = false;
                        }

                        yield return null;

                        IsSelecting = true;

                        #region Reset cards on the field
                        foreach (FieldPermanentCard fieldPermanentCard in gameContext.TurnPlayer.FieldPermanentObjects)
                        {
                            fieldPermanentCard.RemoveSelectEffect();
                            fieldPermanentCard.RemoveClickTarget();
                            fieldPermanentCard.RemoveDragTarget();
                            fieldPermanentCard.Outline_Select.gameObject.SetActive(false);
                            fieldPermanentCard.CloseCommandPanel();
                        }

                        foreach (Player player in gameContext.Players)
                        {
                            OffFieldCardTarget(player);
                        }
                        #endregion

                        #region Reset cards in hand
                        foreach (HandCard handCard1 in gameContext.TurnPlayer.HandCardObjects)
                        {
                            handCard.Outline_Select.gameObject.SetActive(false);
                            handCard1.RemoveSelectEffect();
                            handCard1.RemoveClickTarget();
                            handCard1.RemoveDragTarget();
                        }
                        #endregion

                        List<CardCommand> FieldUnitCommands = new List<CardCommand>();

                        #region Open launch effect command
                        if (handCard.cardSource.CanDeclareSkill)
                        {
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
                                        #region Reset cards on the field
                                        foreach (Player player in gameContext.Players)
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

                                        #region Reset cards in hand
                                        foreach (HandCard handCard1 in gameContext.TurnPlayer.HandCardObjects)
                                        {
                                            handCard.Outline_Select.gameObject.SetActive(false);
                                            handCard1.RemoveSelectEffect();
                                            handCard1.RemoveClickTarget();
                                            handCard1.RemoveDragTarget();
                                        }
                                        #endregion

                                        handCard.GetComponent<Draggable_HandCard>().CanPointerEnterExitAction = true;

                                        photonView.RPC("SetActCardSkill", RpcTarget.All, handCard.cardSource.CardIndex, cardEffects1.IndexOf(cardEffect));
                                    }
                                }
                            }
                        }
                        #endregion

                        handCard.handCardCommandPanel.SetUpCommandPanel(FieldUnitCommands, null, handCard);

                        handCard.AddClickTarget((_fieldUnitCard) => StartCoroutine(SetMainPhase()));

                        handCard.Outline_Select.gameObject.SetActive(true);
                        handCard.SetOrangeOutline();
                    }

                }
                #endregion
            }
            #endregion
        }
        #endregion
    }
    #endregion

    #region Get whether the card in hand is playable
    IEnumerator SetHandCardPlayablity(CardSource cardSource)
    {
        if (cardSource.IsOption) yield break;

        FieldCardFrame foundEmptyFrame = GManager.instance.You.fieldCardFrames
            .Find(fieldCardFrame => fieldCardFrame.IsEmptyFrame() && fieldCardFrame.IsBattleAreaFrame());

        bool canPlayEmpty = foundEmptyFrame != null && cardSource.CanPlayCardTargetFrame(foundEmptyFrame, true, null);

        _canPlayEmptyFrame = canPlayEmpty;

        _canPlayTargetFrames = new bool[GManager.instance.You.fieldCardFrames.Count];
        _canDigivolves = new bool[GManager.instance.You.fieldCardFrames.Count];
        _canJogresses = new bool[GManager.instance.You.fieldCardFrames.Count];
        _canBursts = new bool[GManager.instance.You.fieldCardFrames.Count];
        _canAppFusions = new bool[GManager.instance.You.fieldCardFrames.Count];
        _payingCosts = new int[GManager.instance.You.fieldCardFrames.Count];

        if (cardSource.IsDigimon)
        {
            List<FieldCardFrame> FramesWithPermanent = GManager.instance.You.fieldCardFrames.Filter(frame => frame.GetFramePermanent() != null);

            foreach (FieldCardFrame frame in FramesWithPermanent)
            {
                int frameIndex = GManager.instance.You.fieldCardFrames.IndexOf(frame);

                FieldCardFrame fieldCardFrame = GManager.instance.You.fieldCardFrames[frameIndex];
                Permanent targetPermanent = fieldCardFrame.GetFramePermanent();

                bool canPlay = targetPermanent == null ? canPlayEmpty : cardSource.CanPlayCardTargetFrame(fieldCardFrame, true, null);
                bool canDigivolve = targetPermanent != null && cardSource.CanEvolve(targetPermanent, true);
                bool canJogress = targetPermanent != null && cardSource.CanJogressFromTargetPermanent(targetPermanent, true);
                bool canBurst = targetPermanent != null && cardSource.CanBurstDigivolutionFromTargetPermanent(targetPermanent, true);
                bool canAppFusion = targetPermanent != null && cardSource.CanAppFusionFromTargetPermanent(targetPermanent, true);

                _canPlayTargetFrames[frameIndex] = canPlay || canJogress || canBurst || canAppFusion;
                _canDigivolves[frameIndex] = canDigivolve;
                _canJogresses[frameIndex] = canJogress;
                _canBursts[frameIndex] = canBurst;
                _canAppFusions[frameIndex] = canAppFusion;

                List<int> payingCosts = new List<int>(); ;

                int minPayingCost = 0;

                payingCosts.Add(cardSource.PayingCost(SelectCardEffect.Root.Hand, new List<Permanent>() { targetPermanent }, checkAvailability: false));

                if (canJogress)
                {
                    foreach(JogressCondition dnaCondition in cardSource.jogressCondition)
                        payingCosts.Add(cardSource.GetPayingCostWithBaseCost(dnaCondition.cost, SelectCardEffect.Root.Hand, new List<Permanent>() { targetPermanent }, checkAvailability: false));
                }

                if (canBurst)
                {
                    payingCosts.Add(cardSource.GetPayingCostWithBaseCost(cardSource.burstDigivolutionCondition.cost, SelectCardEffect.Root.Hand, new List<Permanent>() { targetPermanent }, checkAvailability: false));
                }

                if (canAppFusion)
                {
                    payingCosts.Add(cardSource.GetPayingCostWithBaseCost(cardSource.appFusionCondition.cost, SelectCardEffect.Root.Hand, new List<Permanent>() { targetPermanent }, checkAvailability: false));
                }

                if (payingCosts.Count > 1)
                {
                    minPayingCost = payingCosts.Min();
                }

                else if (payingCosts.Count == 1)
                {
                    minPayingCost = payingCosts[0];
                }

                _payingCosts[frameIndex] = minPayingCost;

                yield return null;
            }
        }

        else
        {
            for (int i = 0; i < GManager.instance.You.fieldCardFrames.Count; i++)
            {
                FieldCardFrame fieldCardFrame = GManager.instance.You.fieldCardFrames[i];
                Permanent targetPermanent = fieldCardFrame.GetFramePermanent();

                bool canPlay = targetPermanent == null && canPlayEmpty;

                _canPlayTargetFrames[i] = canPlay;

                _payingCosts[i] = cardSource.PayingCost(SelectCardEffect.Root.Hand, new List<Permanent>() { targetPermanent }, checkAvailability: false);

                yield return null;
            }
        }
    }
    #endregion

    #region Reset UI display/click/drag operations
    void ResetUI()
    {
        foreach (Player player in gameContext.Players)
        {
            player.brainStormObject.EndBrainStorm();

            OffHandCardTarget(player);
            OffFieldCardTarget(player);

            player.securityObject.RemoveClickTarget();

            foreach (HandCard handCard in player.HandCardObjects)
            {
                handCard.RemoveSelectEffect();
                handCard.RemoveClickTarget();
                handCard.RemoveDragTarget();
                handCard.Outline_Select.gameObject.SetActive(false);
                handCard.transform.GetChild(0).gameObject.SetActive(true);
            }

            foreach (FieldPermanentCard fieldPermanentCard in player.FieldPermanentObjects)
            {
                fieldPermanentCard.RemoveSelectEffect();
                fieldPermanentCard.RemoveClickTarget();
                fieldPermanentCard.RemoveDragTarget();
                fieldPermanentCard.CloseCommandPanel();
            }

            foreach (FieldCardFrame fieldCardFrame in player.fieldCardFrames)
            {
                fieldCardFrame.OffFrame_Select();
            }

            player.playMatCardFrame.OffFrame_Select();

            player.securityObject.securityBreakGlass.gameObject.SetActive(false);

            player.securityObject.OffShowSecurityAttackObject();
        }

        GManager.instance.memoryObject.OffMemoryPredictionLine();

        GManager.instance.You.playMatCardFrame.Frame.transform.parent.gameObject.SetActive(false);

        GManager.instance.BackButton.CloseSelectCommandButton();

        GManager.instance.selectCommandPanel.CloseSelectCommandPanel();

        GManager.instance.commandText.CloseCommandText();
    }
    #endregion

    #region Activation effect permanent determination
    [PunRPC]
    public void SetActSkill(int permanentIndex, int skillIndex)
    {
        Permanent UseSkillPermanent = gameContext.TurnPlayer.GetFieldPermanents()[permanentIndex];

        if (0 <= skillIndex && skillIndex < UseSkillPermanent.EffectList(EffectTiming.OnDeclaration).Count)
        {
            this.UseCardEffect = UseSkillPermanent.EffectList(EffectTiming.OnDeclaration)[skillIndex];
        }
    }
    #endregion

    #region Activation effect card determination
    [PunRPC]
    public void SetActCardSkill(int cardIndex, int skillIndex)
    {
        CardSource UseSkillCard = gameContext.ActiveCardList[cardIndex];

        if (0 <= skillIndex && skillIndex < UseSkillCard.EffectList(EffectTiming.OnDeclaration).Count)
        {
            this.UseCardEffect = UseSkillCard.EffectList(EffectTiming.OnDeclaration)[skillIndex];
        }
    }
    #endregion

    #region Play card decision
    [PunRPC]
    public void SetPlayCard(int cardIndex, int TargetFrameID, int[] JogressEvoRootsFrameIDs, int BurstTamerFrameID, int[] AppFusionFrameIDs)
    {
        PlayCard = gameContext.ActiveCardList[cardIndex];
        this.TargetFrameID = TargetFrameID;

        if (JogressEvoRootsFrameIDs != null)
        {
            if (JogressEvoRootsFrameIDs.Length == 2)
            {
                this.JogressEvoRootsFrameIDs = new int[JogressEvoRootsFrameIDs.Length];

                for (int i = 0; i < JogressEvoRootsFrameIDs.Length; i++)
                {
                    this.JogressEvoRootsFrameIDs[i] = JogressEvoRootsFrameIDs[i];
                }
            }
        }

        this.BurstTamerFrameID = BurstTamerFrameID;

        if (AppFusionFrameIDs != null)
        {
            if (AppFusionFrameIDs.Length == 2)
            {
                this.AppFusionFrameIDs = new int[AppFusionFrameIDs.Length];

                for (int i = 0; i < AppFusionFrameIDs.Length; i++)
                {
                    this.AppFusionFrameIDs[i] = AppFusionFrameIDs[i];
                }
            }
        }
    }
    #endregion

    #region Attack permanent determination
    [PunRPC]
    public void SetAttackingPermaent(int permanentIndex, int attackTargetPermanentIndex)
    {
        Permanent AttackingPermanent = gameContext.TurnPlayer.GetFieldPermanents()[permanentIndex];

        if (0 <= attackTargetPermanentIndex && attackTargetPermanentIndex < gameContext.NonTurnPlayer.GetFieldPermanents().Count)
        {
            this.DefendingPermanent = gameContext.NonTurnPlayer.GetFieldPermanents()[attackTargetPermanentIndex];
        }

        this.AttackingPermanent = AttackingPermanent;
    }
    #endregion

    #endregion

    #region end phase
    public bool Passed { get; set; } = true;
    IEnumerator EndPhase()
    {
        #region Add log
        PlayLog.OnAddLog?.Invoke($"\nEnd Turn:\n{gameContext.TurnPlayer.PlayerName}\n");
        #endregion

        #region Deselect
        OffHandCardTarget(gameContext.TurnPlayer);
        OffFieldCardTarget(gameContext.TurnPlayer);
        #endregion

        isSync = true;
        gameContext.TurnPhase = GameContext.phase.End;
        Debug.Log($"{gameContext.TurnPlayer}:End Phase");
        yield return GManager.instance.photonWaitController.StartWait("EndPhase");
        isSync = false;

        isFirstPlayerFirstTurn = false;

        //Automatic processing check timing
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

        #region Reset status until end of turn
        GManager.instance.attackProcess.AttackCount = 0;

        foreach (Player player in gameContext.Players)
        {
            player.UntilEachTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();

            player.UntilCalculateFixedCostEffect = new List<Func<EffectTiming, ICardEffect>>();

            player.DigivolveCount_ThisTurn = 0;

            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                permanent.UntilEachTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();
            }
        }

        foreach (Permanent permanent in gameContext.TurnPlayer.GetFieldPermanents())
        {
            permanent.UntilOwnerTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();
        }

        gameContext.NonTurnPlayer.UntilOpponentTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();

        gameContext.TurnPlayer.UntilOwnerTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();

        foreach (Permanent pokemon in gameContext.NonTurnPlayer.GetFieldPermanents())
        {
            pokemon.UntilOpponentTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();
        }
        #endregion

        #region Reset the number of times the effect is used
        foreach (CardSource cardSource in gameContext.ActiveCardList)
        {
            cardSource.cEntity_EffectController.InitUseCountThisTurn();
        }
        #endregion
    }
    #endregion

    #region resetting of selection status
    #region Release waiting status of cards in hand for selection
    public void OffHandCardTarget(Player player)
    {
        foreach (HandCard _handCard in player.HandCardObjects)
        {
            if (_handCard != null)
            {
                _handCard.RemoveDragTarget();
                _handCard.RemoveClickTarget();
                _handCard.RemoveSelectEffect();
                _handCard.handCardCommandPanel.CloseCommandPanel();
                _handCard.OffPlayText();
            }
        }
    }
    #endregion

    #region Release waiting status of cards in the field for selection
    public void OffFieldCardTarget(Player player)
    {
        foreach (FieldPermanentCard fieldPermanentCard in player.FieldPermanentObjects)
        {
            fieldPermanentCard.RemoveClickTarget();
            fieldPermanentCard.CloseCommandPanel();
            fieldPermanentCard.Outline_Select.gameObject.SetActive(false);
        }
    }
    #endregion
    #endregion

    #region Game over
    public bool endGame { get; set; } = false;
    public void OnClickSurrenderButton()
    {
        int localPlayerID = 0;

        if (PhotonNetwork.IsMasterClient)
        {
            localPlayerID = 0;
        }

        else
        {
            localPlayerID = 1;
        }

        photonView.RPC("Surrender", RpcTarget.All, localPlayerID);
    }

    [PunRPC]
    public void Surrender(int loserPlayerID)
    {
        Player player = null;

        if (loserPlayerID == 0)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                player = GManager.instance.You;
            }

            else
            {
                player = GManager.instance.Opponent;
            }
        }

        else if (loserPlayerID == 1)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                player = GManager.instance.Opponent;
            }

            else
            {
                player = GManager.instance.You;
            }
        }

        if (player != null)
        {
            EndGame(player.Enemy, true);
        }

        //EndGame(gameContext.NonTurnPlayer, true);
    }

    public void EndGame(Player Winner, bool Surrendered, string effectName = "")
    {
        foreach (GameObject gb in GManager.instance.CloseWhenEndingGameObjects)
        {
            if (gb != null)
            {
                gb.SetActive(false);
            }
        }

        ContinuousController.instance.CanSetRandom = false;

        GManager.instance.photonWaitController.ResetKeys();

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable());
        }

        if (!ContinuousController.instance.isRandomMatch && !ContinuousController.instance.isAI)
        {
            Debug.Log("Player property initialization");
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable());
        }

        endGame = true;

        if (gameContext.TurnPlayer != null)
        {
            OffFieldCardTarget(gameContext.TurnPlayer);

            OffHandCardTarget(gameContext.TurnPlayer);
        }

        foreach (Player player in gameContext.Players)
        {
            player.securityObject.OffShowSecurityAttackObject();
        }

        GManager.instance.optionPanel.Close_(false);

        GManager.instance.LoadingObject.gameObject.SetActive(false);

        GManager.instance.resultObject.ShowResult(Winner, Surrendered, effectName);

        EventSystem.current.SetSelectedGameObject(GManager.instance.resultObject.transform.GetChild(3).gameObject);

        GManager.instance.commandText.CloseCommandText();

        StopAllCoroutines();
        GManager.instance.StopAllCoroutines();
        ContinuousController.instance.StopAllCoroutines();

        ContinuousController.instance.StartCoroutine(GManager.instance.BattleBGM.FadeOut(1));

        if (GManager.instance.isAuto && GManager.instance.IsAI)
        {
            ContinuousController.instance.isAI = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
        }
    }
    #endregion

    #region Go to the next phase
    [PunRPC]
    public void NextPhase()
    {
        if (gameContext.TurnPhase != GameContext.phase.Main)
        {
            int CurrentPhaseID = (int)gameContext.TurnPhase;

            int NextPhaseID = ++CurrentPhaseID;

            int MaxPhaseCount = Enum.GetNames(typeof(GameContext.phase)).Length;

            if (NextPhaseID >= MaxPhaseCount)
            {
                NextPhaseID = 0;
            }

            isSync = true;
            gameContext.TurnPhase = (GameContext.phase)Enum.ToObject(typeof(GameContext.phase), NextPhaseID);
        }

        else
        {
            ResetUI();
            ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.EndTurnProcess());
        }
    }
    #endregion
}
