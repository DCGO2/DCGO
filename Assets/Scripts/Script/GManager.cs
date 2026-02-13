using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GManager : MonoBehaviourPun
{
    [Header("あなた")]
    public Player You;

    [Header("対戦相手")]
    public Player Opponent;

    [Header("CardSourceプレハブ")]
    public CardSource CardPrefab;

    [Header("場のポケモンカードプレハブ")]
    public FieldPermanentCard fieldCardPrefab;

    [Header("手札のカードプレハブ")]
    public HandCard handCardPrefab;

    [Header("読み込み中オブジェクト")]
    public LoadingObject LoadingObject;

    [Header("コマンド選択パネル")]
    public SelectCommandPanel selectCommandPanel;

    [Header("戻るボタン")]
    public SelectCommand BackButton;

    [Header("結果表示オブジェクト")]
    public ResultObject resultObject;

    [Header("メッセージテキスト")]
    public CommandText commandText;

    [Header("ターンプレイヤー表示")]
    public ShowTurnPlayerObject showTurnPlayerObject;

    [Header("フェイズ通知")]
    public ShowPhaseNotificationObject showPhaseNotificationObject;

    [Header("キャンバス")]
    public Canvas canvas;

    [Header("キャンバス2")]
    public Canvas canvas2;

    [Header("カード詳細")]
    public CardDetail cardDetail;

    [Header("パーマネント詳細")]
    public PermanentDetail pokemonDetail;

    [Header("カード選択パネル")]
    public SelectCardPanel selectCardPanel;

    [Header("カード確認パネル")]
    public CheckCardPanel checkCardPanel;

    [Header("次のフェイズに進むボタン")]
    public NextPhaseButton nextPhaseButton;

    [Header("設定パネル")]
    public OptionPanel optionPanel;

    [Header("ターゲット矢印プレハブ")]
    public TargetArrow targetArrowPrefab;

    [Header("ターゲット矢印親")]
    public Transform targetArrowParent;

    [Header("Camera")]
    public Camera camara;

    [Header("StartBattleSE")]
    public AudioClip StartBattleSE;

    [Header("自動処理チェック")]
    public AutoProcessing autoProcessing;

    [Header("プレイログ")]
    public PlayLog playLog;

    [Header("メモリー")]
    public MemoryObject memoryObject;

    [Header("背景")]
    public Image BackgroundImage;

    [Header("背景")]
    public SpriteRenderer BackgroundSpriteRenderer;

    [Header("攻撃処理")]
    public AttackProcess attackProcess;

    [Header("ジョグレス選択")]
    public SelectJogressEffect selectJogressEffect;
    public SelectDNACondition selectDNACondition;

    [Header("burst evolution selection")]
    public SelectBurstDigivolutionEffect selectBurstDigivolutionEffect;

    [Header("app fusion selection")]
    public SelectAppFusionEffect selectAppFusionEffect;

    [Header("自動処理チェック(割り込み処理)")]
    public AutoProcessing autoProcessing_CutIn;
    [Header("ユーザー選択")]
    public UserSelectionManager userSelectionManager;

    [Header("選択可能なものだけ表示")]
    public HideCannotSelectObject hideCannotSelectObject;

    [Header("サイドバー")]
    public SideBar sideBar;

    [Header("UseSkillSE")]
    public AudioClip UseSkillSE;

    [Header("サイド取得SE")]
    public AudioClip GetSideSE;

    [Header("DeleteHandSE")]
    public AudioClip DeleteHandSE;

    [Header("エネルギー追加SE")]
    public AudioClip AddEnergySE;

    [Header("ShowPlayCardSE")]
    public AudioClip ShowPlayCardSE;

    [Header("ポケモンプレイ時SE")]
    public AudioClip PlayPokemonSE;

    [Header("ShuffleSE")]
    public AudioClip ShuffleSE;

    [Header("DrawSE")]
    public AudioClip DrawSE;

    [Header("TargetArrowSE")]
    public AudioClip TargetArrowSE;

    [Header("MoveSE")]
    public AudioClip MoveSE;

    [Header("気絶SE")]
    public AudioClip KnockOutSE;

    [Header("ダメージSE")]
    public AudioClip DamageSE;

    [Header("コイントスSE")]
    public AudioClip CointTossSE;

    [Header("回復SE")]
    public AudioClip HealSE;

    [Header("毒SE")]
    public AudioClip PoisonSE;

    [Header("火傷SE")]
    public AudioClip BurnedSE;

    [Header("WinSE")]
    public AudioClip WinSE;

    [Header("LoseSE")]
    public AudioClip LoseSE;

    [Header("DecisionSE")]
    [SerializeField] AudioClip DecisionSE;

    [Header("CancelSE")]
    [SerializeField] AudioClip CancelSE;

    [Header("BGM")]
    public List<AudioClip> bgms;

    [Header("BGMObject")]
    public BGMObject BattleBGM;

    [Header("オートモード")]
    public bool isAuto;

    [Header("進化演出")]
    public EvolutionEffectObject EvolutionEffectObject;

    [Header("ジョグレス進化演出")]
    public JogressEffectObject jogressEffectObject;

    [Header("デジクロス演出")]
    public DigiXrosEffectObject digiXrosEffectObject;

    [Header("バースト進化演出")]
    public BurstEffectObject burstEffectObject;
    [Header("Background Particle effects")]
    [SerializeField] List<ParticleSystem> _backgroundParticles = new List<ParticleSystem>();
    [Header("Photon wait controller")]
    public PhotonWaitController photonWaitController;
    [Header("Gameobjects closed when ending game")]
    public List<GameObject> CloseWhenEndingGameObjects = new List<GameObject>();

    //Turn progression management state machine
    public TurnStateMachine turnStateMachine { get; set; }

    public static GManager instance = null;

    public bool IsAI { get; private set; } = false;

    public int CardIndex { get; set; } = 0;

    public bool ActivateShortcuts = false;

    #region Events

    //Cards flipped
    public static Action OnReverseOpponentsCardsChanged;
    public static Action OnCardFlippedChanged;

    public static Action<Player> OnSecurityStackChanged;
    
    #endregion

    private async void Awake()
    {
        //return;

        if (Opening.instance != null)
        {
            if (Opening.instance.OpeningBGM != null)
            {
                Opening.instance.OpeningBGM.StopPlayBGM();
            }
        }

        instance = this;

        StartCoroutine(AwakeCoroutine());
    }

    IEnumerator AwakeCoroutine()
    {
#if UNITY_EDITOR

#endif
        if (!PhotonNetwork.IsConnected)
        {
            IsAI = true;
        }

        if (ContinuousController.instance != null)
        {
            if (ContinuousController.instance.isAI)
            {
                IsAI = true;
            }
        }

        if (!IsAI)
        {
            isAuto = false;
        }

        turnStateMachine = gameObject.AddComponent<TurnStateMachine>();

        GetComponent<Effects>().Init();

        playLog.Init();

        hideCannotSelectObject.Init();

        ChangeBackground();

        yield return StartCoroutine(Init());

        StartCoroutine(turnStateMachine.Init());

        StartCoroutine(CheckDisconnect());

        if (Opening.instance != null)
        {
            Opening.instance.openingObject.SetActive(false);
        }

        Debug.Log("Battle Initialization");

        yield return new WaitWhile(() => ContinuousController.instance == null);

        ContinuousController.instance.CanSetRandom = true;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void PlayDecisionSE()
    {
        ContinuousController.instance.PlaySE(DecisionSE);
    }

    public void PlayCancelSE()
    {
        ContinuousController.instance.PlaySE(CancelSE);
    }

    public async void ChangeBackground()
    {
        Sprite backgroundSprite = await StreamingAssetsUtility.GetSprite("Background_battle");

        if (backgroundSprite != null)
        {
            BackgroundImage.sprite = backgroundSprite;
            BackgroundSpriteRenderer.sprite = backgroundSprite;
            BackgroundImage.gameObject.SetActive(true);

            BackgroundSpriteRenderer.transform.localPosition = new Vector3(-134, BackgroundSpriteRenderer.transform.localPosition.y, -77f);
            BackgroundSpriteRenderer.gameObject.SetActive(true);
        }

        else
        {
            BackgroundImage.gameObject.SetActive(false);
            BackgroundSpriteRenderer.gameObject.SetActive(false);
        }
    }

    IEnumerator CheckDisconnect()
    {
        if (IsAI)
        {
            yield break;
        }

        yield return new WaitWhile(() => turnStateMachine == null);

        yield return new WaitForSeconds(5f);

        while (true)
        {
            if (!PhotonNetwork.IsConnected)
            {
                break;
            }

            else
            {
                if (!PhotonNetwork.InRoom)
                {
                    break;
                }

                else
                {
                    if (PhotonNetwork.CurrentRoom != null)
                    {
                        if (PhotonNetwork.PlayerList.Length < 2)
                        {
                            break;
                        }
                    }
                }
            }

            yield return null;
        }

        if (!turnStateMachine.endGame)
        {
            turnStateMachine.EndGame(null, false);
        }
    }

    public IEnumerator Init()
    {
        yield return StartCoroutine(LoadingObject.StartLoading("Now Loading"));

        selectCommandPanel.Off();

        BackButton.CloseSelectCommandButton();

        resultObject.Init();

        commandText.Init();

        sideBar.Init();

        showTurnPlayerObject.Init();

        showPhaseNotificationObject.Init();

        OffTargetArrow();

        cardDetail.CloseCardDetail();

        pokemonDetail.CloseUnitDetail();

        selectCardPanel.CloseSelectCardPanel();

        yield return StartCoroutine(checkCardPanel.CloseSelectCardPanelCoroutine());

        optionPanel.Init();

        EvolutionEffectObject.Init();

        jogressEffectObject.Init();

        digiXrosEffectObject.Init();

        burstEffectObject.Init();
    }

    public void ReturnToTitle()
    {
        ContinuousController.instance.EndBattle();
    }

    public void ReportBug()
    {
        Application.OpenURL("https://forms.gle/GhZgGVJS1qLeUMcG8");
    }

    public void OnClickSurrenderButton()
    {
        if (turnStateMachine != null)
        {
            if (turnStateMachine.endGame)
            {
                return;
            }

            turnStateMachine.OnClickSurrenderButton();
        }
    }

    public TargetArrow CreateTargetArrow()
    {
        TargetArrow targetArrow = Instantiate(targetArrowPrefab, targetArrowParent);

        return targetArrow;
    }

    public Coroutine OnTargetArrow(Vector3 InitialPosition, Vector3 targetPosition, FieldPermanentCard StartFieldUnitCard, FieldPermanentCard EndFieldUnitCard)
    {
        TargetArrow targetArrow = CreateTargetArrow();

        return targetArrow.OnTargetArrow(InitialPosition, targetPosition, StartFieldUnitCard, EndFieldUnitCard);
    }

    public void OffTargetArrow()
    {
        if (targetArrowParent.childCount > 0)
        {
            if (targetArrowParent.GetChild(targetArrowParent.childCount - 1).GetComponent<TargetArrow>() != null)
            {
                targetArrowParent.GetChild(targetArrowParent.childCount - 1).GetComponent<TargetArrow>().Destroyed = true;
            }

            Destroy(targetArrowParent.GetChild(targetArrowParent.childCount - 1).gameObject);
        }
    }

    void Update()
    {
        if (ContinuousController.instance != null)
        {
            foreach (ParticleSystem particleSystem in _backgroundParticles)
            {
                if (particleSystem != null)
                {
                    particleSystem.gameObject.SetActive(ContinuousController.instance.showBackgroundParticle);
                }
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) &&
           Input.GetKey(KeyCode.LeftShift) && 
           Input.GetKey(KeyCode.LeftAlt) &&
           Input.GetKeyDown(KeyCode.A))
            ActivateShortcuts = !ActivateShortcuts;


        AllowAlphaInputs();
    }

    void AllowAlphaInputs()
    {
        if (ContinuousController.instance.isRandomMatch && !ContinuousController.instance.isAI)
            return;

        if (turnStateMachine == null)
            return;
        
        if (!ActivateShortcuts)
            return;

        //Draw a card
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            photonView.RPC("DrawCardRPC", RpcTarget.Others);
            StartCoroutine(DrawCard(You));
        }

        //Trash a card
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
        {
            photonView.RPC("TrashCardRPC", RpcTarget.Others);
            StartCoroutine(TrashCard(You));
        }

        //Top deck a card
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
        {
            photonView.RPC("TopDeckCardRPC", RpcTarget.Others);
            StartCoroutine(TopDeckCard(You));
        }

        //Place Top Security
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            bool keyInput = Input.GetKey(KeyCode.LeftShift);
            photonView.RPC("PlaceInSecurityRPC", RpcTarget.Others, keyInput);
            StartCoroutine(PlaceInSecurity(You, keyInput));
        }

        //Gain Memory
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Equals))
        {
            photonView.RPC("AlterMemoryRPC", RpcTarget.Others, -1);
            StartCoroutine(AlterMemory(You, 1));
        }

        //Lose Memory
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Minus))
        {
            photonView.RPC("AlterMemoryRPC", RpcTarget.Others, 1);
            StartCoroutine(AlterMemory(You, -1));
        }
            
    }

    [PunRPC]
    public void DrawCardRPC()
    {
        StartCoroutine(DrawCard(Opponent));
    }

    [PunRPC]
    public void TrashCardRPC()
    {
        StartCoroutine(TrashCard(Opponent));
    }

    [PunRPC]
    public void TopDeckCardRPC()
    {
        StartCoroutine(TopDeckCard(Opponent));
    }

    [PunRPC]
    public void PlaceInSecurityRPC(bool keyInput)
    {
        StartCoroutine(PlaceInSecurity(Opponent, keyInput));
    }

    [PunRPC]
    public void AlterMemoryRPC(int value)
    {
        StartCoroutine(AlterMemory(Opponent,value));
    }


    public IEnumerator DrawCard(Player _player)
    {
        yield return StartCoroutine(new DrawClass(_player, 1, null).Draw());

        yield return StartCoroutine(turnStateMachine.SetMainPhase());
    }

    IEnumerator AlterMemory(Player _player, int value)
    {
        yield return StartCoroutine(You.AddMemory(value, null));

        yield return StartCoroutine(turnStateMachine.SetMainPhase());

        yield return StartCoroutine(autoProcessing.EndTurnCheck());
    }

    IEnumerator TrashCard(Player _player)
    {
        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

        selectHandEffect.SetUp(
            selectPlayer: _player,
            canTargetCondition: (CardSource) => true,
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            maxCount: _player.HandCards.Count,
            canNoSelect: true,
            canEndNotMax: true,
            isShowOpponent: true,
            selectCardCoroutine: null,
            afterSelectCardCoroutine: null,
            mode: SelectHandEffect.Mode.Discard,
            cardEffect: null);

        yield return StartCoroutine(selectHandEffect.Activate());

        yield return StartCoroutine(turnStateMachine.SetMainPhase());
    }

    IEnumerator TopDeckCard(Player _player)
    {
        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

        selectHandEffect.SetUp(
            selectPlayer: _player,
            canTargetCondition: (CardSource) => true,
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            maxCount: _player.HandCards.Count,
            canNoSelect: true,
            canEndNotMax: true,
            isShowOpponent: true,
            selectCardCoroutine: null,
            afterSelectCardCoroutine: null,
            mode: SelectHandEffect.Mode.PutLibraryTop,
            cardEffect: null);

        yield return StartCoroutine(selectHandEffect.Activate());

        yield return StartCoroutine(turnStateMachine.SetMainPhase());
    }

    IEnumerator PlaceInSecurity(Player _player, bool placeFaceup)
    {
        CardSource selectedSource = null;

        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

        selectHandEffect.SetUp(
            selectPlayer: _player,
            canTargetCondition: (CardSource) => true,
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            maxCount: 1,
            canNoSelect: true,
            canEndNotMax: false,
            isShowOpponent: true,
            selectCardCoroutine: CardSelectCoroutine,
            afterSelectCardCoroutine: null,
            mode: SelectHandEffect.Mode.Custom,
            cardEffect: null);

        yield return StartCoroutine(selectHandEffect.Activate());

        IEnumerator CardSelectCoroutine(CardSource source)
        {
            if (source != null)
                selectedSource = source;

            yield return null;
        }

        if (selectedSource != null)
        {
            // Place this card face up as the top security card
            yield return StartCoroutine(CardObjectController.AddSecurityCard(selectedSource, toTop: true, faceUp: placeFaceup));
        }

        yield return StartCoroutine(turnStateMachine.SetMainPhase());
    }
}
