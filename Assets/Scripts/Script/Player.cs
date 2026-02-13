using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using TMPro;
public class Player : MonoBehaviour
{
    #region 初期化
    private async void Start()
    {
        fieldCardFrames = new List<FieldCardFrame>();

        if (BattleAreaFrameParent != null && BreedingAreaFrameParent != null)
        {
            int count = 0;

            for (int i = 0; i < BattleAreaFrameParent.childCount; i++)
            {
                if (BattleAreaFrameParent.GetChild(i).childCount >= 2)
                {
                    FieldCardFrame fieldCardFrame = new FieldCardFrame();
                    fieldCardFrame.Frame = BattleAreaFrameParent.GetChild(i).GetChild(0).gameObject;
                    fieldCardFrame.Frame_Select = BattleAreaFrameParent.GetChild(i).GetChild(1).GetComponent<Image>();
                    fieldCardFrame.FrameID = count;
                    fieldCardFrame.player = this;
                    fieldCardFrame.Frame.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    fieldCardFrame.Frame_Select.color = new Color(0, 0, 0, 0);
                    fieldCardFrames.Add(fieldCardFrame);

                    count++;
                }
            }

            if (BreedingAreaFrameParent.childCount >= 2)
            {
                FieldCardFrame fieldCardFrame = new FieldCardFrame();
                fieldCardFrame.Frame = BreedingAreaFrameParent.GetChild(0).gameObject;
                fieldCardFrame.Frame_Select = BreedingAreaFrameParent.GetChild(1).GetComponent<Image>();
                fieldCardFrame.FrameID = count;
                fieldCardFrame.player = this;
                fieldCardFrame.Frame.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                fieldCardFrame.Frame_Select.color = new Color(0, 0, 0, 0);
                fieldCardFrames.Add(fieldCardFrame);

                count++;
            }

            BattleAreaFrameParent.gameObject.SetActive(true);
            BreedingAreaFrameParent.gameObject.SetActive(true);
        }

        for (int i = 0; i < HandTransform.childCount; i++)
        {
            Destroy(HandTransform.GetChild(i).gameObject);
        }

        for (int i = 0; i < PermanentTransform.childCount; i++)
        {
            Destroy(PermanentTransform.GetChild(i).gameObject);
        }

        for (int i = 0; i < fieldCardFrames.Count; i++)
        {
            fieldCardFrames[i].OffFrame_Select();

            fieldCardFrames[i].Frame.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            fieldCardFrames[i].Frame_Select.color = new Color(0, 0, 0, 0);
        }

        TrashHandCard.gameObject.SetActive(false);

        FieldPermanents = new Permanent[fieldCardFrames.Count];

        SetPlayerUI();

        await SetOriginalPlayMat();

        FirstObject.SetActive(false);

        playMatCardFrame.OffFrame_Select();

        if (PlayerNameText != null)
        {
            PlayerNameText.transform.parent.gameObject.SetActive(false);
        }

        OffHatchObject();

        if (HatchObject != null)
        {
            Image hatchImage = HatchObject.GetComponent<Image>();

            if (hatchImage != null)
            {
                if (ContinuousController.instance != null)
                {
                    hatchImage.sprite = ContinuousController.instance.ReverseCard_Digitama;
                }
            }
        }

        if (_handCountText != null)
        {
            _handCountText.text = "";
        }
    }
    #endregion

    public GameObject FirstObject;
    public bool IsLose { get; set; } = false;
    public Transform playerUIObjectParent;

    public void SetLose()
    {
        IsLose = true;
    }

    public void OnClickFrame(int FrameID)
    {
        FieldCardFrame fieldCardFrame = fieldCardFrames[FrameID];

        fieldCardFrame.OnClickAction?.Invoke(fieldCardFrame);
    }

    int _timerCount = 0;
    int _updateFrame = 20;
    private void Update()
    {
        #region Update only once every few frames
        _timerCount++;

        if (_timerCount < _updateFrame)
        {
            return;
        }

        else
        {
            _timerCount = 0;
        }
        #endregion

        SetPlayerUI();
    }

    public void SetPlayerUI()
    {
        AlignHand();

        SetLibraryCountText();

        SetDigitamaLibraryCountText();

        securityObject.SetSecurity(this);

        ShowTrash();

        SetHandCountText();
    }

    #region セキュリティアタックの座標
    public Vector3 SecurityAttackLocalCanvasPosition;
    #endregion

    #region 処理領域の座標
    public Vector3 ExecutingAreaLocalCanvasPosition;
    #endregion

    #region そのプレイヤーの何ターン目か
    public int TurnCount { get; set; } = 0;
    #endregion

    #region 処理領域表示オブジェクト
    public BrainStormObject brainStormObject;
    #endregion

    #region 山札の枚数表示テキスト
    public Text LibraryCountText;
    public List<Image> DeckCardImages;
    public Text DigitamaLibraryCountText;
    public List<Image> DigitamaDeckCardImages;

    public EventTrigger HatchObject;
    public UnityAction OnClickHatchObjectAction;
    [SerializeField] TextMeshProUGUI _handCountText;
    public void OnClickHatchObject()
    {
        OnClickHatchObjectAction?.Invoke();
    }

    public void OffHatchObject()
    {
        this.OnClickHatchObjectAction = null;

        if (HatchObject != null)
        {
            HatchObject.gameObject.SetActive(false);
        }
    }

    public void SetUpHatchObject(UnityAction OnClickHatchObjectAction)
    {
        this.OnClickHatchObjectAction = OnClickHatchObjectAction;

        if (HatchObject != null)
        {
            HatchObject.gameObject.SetActive(true);

            Image targetImage = null;

            for (int i = 0; i < DigitamaDeckCardImages.Count; i++)
            {
                if (DigitamaDeckCardImages[i].gameObject.activeSelf)
                {
                    targetImage = DigitamaDeckCardImages[i];
                }
            }

            if (targetImage != null)
            {
                HatchObject.transform.localPosition = targetImage.transform.localPosition;
            }
        }
    }

    void SetHandCountText()
    {
        if (_handCountText != null)
        {
            _handCountText.text = $"Hand\n{HandCards.Count}";
        }
    }

    void SetLibraryCountText()
    {
        LibraryCountText.text = $"{LibraryCards.Count}";

        if (LibraryCards.Count == 0)
        {
            for (int i = 0; i < DeckCardImages.Count; i++)
            {
                DeckCardImages[i].gameObject.SetActive(false);
            }
        }

        else
        {
            int count = LibraryCards.Count / 8;

            for (int i = 0; i < DeckCardImages.Count; i++)
            {
                if (i <= count)
                {
                    DeckCardImages[i].gameObject.SetActive(true);
                    DeckCardImages[i].sprite = ContinuousController.instance.ReverseCard;
                }

                else
                {
                    DeckCardImages[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetDigitamaLibraryCountText()
    {
        DigitamaLibraryCountText.text = $"{DigitamaLibraryCards.Count}";

        if (DigitamaLibraryCards.Count == 0)
        {
            for (int i = 0; i < DigitamaDeckCardImages.Count; i++)
            {
                DigitamaDeckCardImages[i].gameObject.SetActive(false);
            }
        }

        else
        {
            int count = DigitamaLibraryCards.Count;

            for (int i = 0; i < DigitamaDeckCardImages.Count; i++)
            {
                if (i < count)
                {
                    DigitamaDeckCardImages[i].gameObject.SetActive(true);
                    DigitamaDeckCardImages[i].sprite = ContinuousController.instance.ReverseCard_Digitama;
                }

                else
                {
                    DigitamaDeckCardImages[i].gameObject.SetActive(false);
                }
            }
        }
    }
    #endregion

    #region トラッシュ
    [Header("トラッシュ画像")]
    public Image TrashCardImage;

    [Header("トラッシュ枚数テキスト")]
    public Text TrashCountText;

    async void ShowTrash()
    {
        if (TrashCardImage != null)
        {
            if (TrashCards.Count == 0)
            {
                TrashCardImage.gameObject.SetActive(false);
            }

            else
            {
                TrashCardImage.gameObject.SetActive(true);
                TrashCardImage.sprite = await TrashCards[0].GetCardSprite();

                if (!isYou)
                {
                    if (ContinuousController.instance != null)
                    {
                        Quaternion localRotation = Quaternion.Euler(0, 0, 0);

                        if (ContinuousController.instance.reverseOpponentsCards)
                        {
                            localRotation = Quaternion.Euler(0, 0, 180);
                        }

                        TrashCardImage.transform.localRotation = localRotation;
                    }
                }
            }
        }

        if (TrashCountText != null)
        {
            TrashCountText.text = $"{TrashCards.Count}";
        }

    }
    #endregion

    #region シャッフルアニメーション
    public IEnumerator ShuffleAnimation()
    {
        float animTime = 0.03f;

        yield return null;

        for (int k = 0; k < 3; k++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < DeckCardImages.Count; i++)
                {
                    float targetY = 0;

                    if (j % 2 == 0)
                    {
                        targetY = 0;
                    }

                    else
                    {
                        targetY = 30;
                    }

                    if (k % 2 == 0)
                    {
                        if (i % 2 == 0)
                        {
                            targetY *= -1;
                        }
                    }

                    else
                    {
                        if (i % 2 == 1)
                        {
                            targetY *= -1;
                        }
                    }

                    DeckCardImages[i].transform.DOLocalMoveY(targetY, animTime);
                }

                yield return new WaitForSeconds(animTime);
            }
        }

        for (int i = 0; i < DeckCardImages.Count; i++)
        {
            DeckCardImages[i].transform.localPosition = new Vector2(DeckCardImages[i].transform.localPosition.x, DeckCardImages[i].transform.localPosition.x);
        }
    }
    #endregion

    #region セキュリティ表示
    public SecurityObject securityObject;
    #endregion

    #region 手札を整列
    public void AlignHand()
    {
        if (isYou)
        {
            if (GManager.instance != null)
            {
                if (GManager.instance.turnStateMachine != null)
                {
                    if (GManager.instance.turnStateMachine.DoneStartGame)
                    {
                        if (!HandTransform.GetComponent<HandContoller>().isDragging)
                        {
                            foreach (CardSource cardSource in HandCards)
                            {
                                if (cardSource.ShowingHandCard != null)
                                {
                                    cardSource.ShowingHandCard.transform.SetParent(HandTransform);
                                }
                            }

                            CardObjectController.AlignHand(this);
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region キーカード
    public CEntity_Base KeyCard { get; set; }
    #endregion

    #region カード情報

    #region デッキのカード
    public List<CardSource> LibraryCards = new List<CardSource>();
    #endregion

    #region デジタマデッキのカード
    public List<CardSource> DigitamaLibraryCards = new List<CardSource>();
    #endregion

    #region 手札のカード
    public List<CardSource> HandCards = new List<CardSource>();
    #endregion

    #region 場外のカード
    public List<CardSource> TrashCards = new List<CardSource>();
    #endregion

    #region ロストのカード
    public List<CardSource> LostCards = new List<CardSource>();
    #endregion

    #region ライフのカード
    public List<CardSource> SecurityCards = new List<CardSource>();
    #endregion

    #region 処理領域のカード
    public List<CardSource> ExecutingCards = new List<CardSource>();
    #endregion

    #endregion

    #region このプレイヤーがあなたかどうか
    [Header("このプレイヤーがあなたかどうか")]
    public bool isYou;
    #endregion

    #region CardSource置き場
    [Header("CardSource置き場")]
    public Transform CardSorcesParent;
    #endregion

    #region カード配置場所

    [Header("パーマネントの配置場所")]
    public Transform PermanentTransform;

    [Header("手札の配置場所")]
    public Transform HandTransform;

    #endregion

    #region プレイマットSpriteRenderer
    public SpriteRenderer PlayMatSpriteRenderer;

    public SpriteRenderer PlayMatSpriteRenderer_Original;

    public async Task SetOriginalPlayMat()
    {
        if (PlayMatSpriteRenderer_Original != null)
        {
            string filiName = "";

            if (isYou)
            {
                filiName = "PlayMat_You";
            }

            else
            {
                filiName = "PlayMat_Opponent";
            }

            Sprite playMatSprite = await StreamingAssetsUtility.GetSprite(filiName);

            if (playMatSprite != null)
            {
                Sprite sprite = playMatSprite;
                PlayMatSpriteRenderer_Original.sprite = sprite;
                PlayMatSpriteRenderer_Original.gameObject.SetActive(true);

                float width = 3045f;

                float spriteWidth = sprite.rect.width;
                float spriteHeight = sprite.rect.height;

                if (spriteWidth == 0)
                {
                    spriteWidth = 0.01f;
                }

                float scale = width / spriteWidth;

                PlayMatSpriteRenderer_Original.transform.localScale = new Vector3(scale, scale, 1);
                PlayMatSpriteRenderer_Original.transform.localPosition = new Vector3(-15.2f, -6f, 0);
            }

            else
            {
                PlayMatSpriteRenderer_Original.gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region 枠
    [Header("パーマネント枠")]
    public List<FieldCardFrame> fieldCardFrames = new List<FieldCardFrame>();

    [Header("バトルエリア枠親")]
    [SerializeField] Transform BattleAreaFrameParent;

    [Header("育成エリア枠親")]
    [SerializeField] Transform BreedingAreaFrameParent;
    #endregion

    #region 手札領域の幅
    [Header("手札領域の幅")]
    public float HandWidth;
    #endregion

    #region バトルエリアのパーマネント
    public List<Permanent> GetBattleAreaPermanents()
    {
        List<Permanent> GetBattleAreaPermanents = new List<Permanent>();

        for (int i = 0; i < FieldPermanents.Length; i++)
        {
            if (FieldCardFrame.isBattleAreaFrameID(i))
            {
                if (FieldPermanents[i] != null)
                {
                    if (FieldPermanents[i].TopCard != null)
                    {
                        GetBattleAreaPermanents.Add(FieldPermanents[i]);
                    }
                }
            }
        }

        return GetBattleAreaPermanents;
    }
    #endregion

    #region 育成エリアのパーマネント
    public List<Permanent> GetBreedingAreaPermanents()
    {
        List<Permanent> GetBreedingAreaPermanents = new List<Permanent>();

        for (int i = 0; i < FieldPermanents.Length; i++)
        {
            if (!FieldCardFrame.isBattleAreaFrameID(i))
            {
                if (FieldPermanents[i] != null)
                {
                    if (FieldPermanents[i].TopCard != null)
                    {
                        GetBreedingAreaPermanents.Add(FieldPermanents[i]);
                    }
                }
            }
        }

        return GetBreedingAreaPermanents;
    }
    #endregion

    #region 場のパーマネント
    public Permanent[] FieldPermanents = new Permanent[16];

    public List<Permanent> GetFieldPermanents()
    {
        List<Permanent> GetFieldPermanents = new List<Permanent>();

        for (int i = 0; i < FieldPermanents.Length; i++)
        {
            if (FieldPermanents[i] != null)
            {
                if (FieldPermanents[i].TopCard != null)
                {
                    GetFieldPermanents.Add(FieldPermanents[i]);
                }
            }
        }

        return GetFieldPermanents;
    }

    public List<Permanent> GetBattleAreaDigimons()
    {
        List<Permanent> battleAreaPermanents = GetBattleAreaPermanents();
        List<Permanent> GetBattleAreaDigimons = new List<Permanent>();


        for (int i = 0; i < battleAreaPermanents.Count; i++)
        {
            if (battleAreaPermanents[i] != null)
            {
                if (battleAreaPermanents[i].TopCard != null)
                {
                    if (battleAreaPermanents[i].IsDigimon)
                    {
                        GetBattleAreaDigimons.Add(battleAreaPermanents[i]);
                    }
                }
            }
        }

        return GetBattleAreaDigimons;
    }
    #endregion

    #region Player Name
    private string _playerName = "Opponent";
    public string PlayerName {

        get {
            if (isYou || GManager.instance.IsAI)
                return _playerName; 
            else 
                return "Opponent";
        }
        set { _playerName = value; }
    }
    [Header("プレイヤー名")]
    public TextMeshProUGUI PlayerNameText;
    #endregion

    #region 勝利数
    public int WinCount { get; set; }

    public void ShowWinCount()
    {
        //WinCountText.transform.parent.gameObject.SetActive(true);
    }

    public void HideWinCount()
    {
        //WinCountText.transform.parent.gameObject.SetActive(false);
    }
    #endregion

    #region PlayerID
    public int PlayerID { get; set; }
    #endregion

    #region Enemy
    public Player Enemy
    {
        get
        {
            if (GManager.instance != null)
            {
                if (GManager.instance.turnStateMachine != null)
                {
                    if (GManager.instance.turnStateMachine.gameContext != null)
                    {
                        if (GManager.instance.turnStateMachine.gameContext.Players.Contains(this))
                        {
                            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                            {
                                if (player != this)
                                {
                                    return player;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
    #endregion

    #region プレイマット
    [Header("PlayMat")]
    public GameObject PlayMat;
    #endregion

    #region プレイマットの枠
    public FieldCardFrame playMatCardFrame;
    #endregion

    #region 手札のドロップエリア
    [Header("手札のドロップエリア")]
    public DropArea HandDropArea;
    #endregion

    #region 場のパーマネントカードオブジェクト
    public List<FieldPermanentCard> FieldPermanentObjects
    {
        get
        {
            List<FieldPermanentCard> _FieldPermanentObjects = new List<FieldPermanentCard>();

            for (int i = 0; i < PermanentTransform.childCount; i++)
            {
                FieldPermanentCard fieldPermanentCard = PermanentTransform.GetChild(i).GetComponent<FieldPermanentCard>();

                if (fieldPermanentCard != null)
                {
                    _FieldPermanentObjects.Add(fieldPermanentCard);
                }
            }

            return _FieldPermanentObjects;
        }
    }
    #endregion

    #region 手札のカードオブジェクト
    public List<HandCard> HandCardObjects
    {
        get
        {
            List<HandCard> _HandCards = new List<HandCard>();

            foreach (CardSource cardSource in HandCards)
            {
                if (cardSource.ShowingHandCard != null)
                {
                    _HandCards.Add(cardSource.ShowingHandCard);
                }
            }

            return _HandCards;
        }
    }
    #endregion

    #region プレイヤーに掛かっている効果

    #region All effects on the player
    public List<ICardEffect> EffectList(EffectTiming timing)
    {
        List<ICardEffect> PlayerEffects = new List<ICardEffect>();

        foreach (Func<EffectTiming, ICardEffect> GetCardEffect in PermanentEffects)
        {
            if (GetCardEffect(timing) != null)
            {
                PlayerEffects.Add(GetCardEffect(timing));
            }
        }

        foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilEndBattleEffects)
        {
            if (GetCardEffect(timing) != null)
            {
                PlayerEffects.Add(GetCardEffect(timing));
            }
        }

        foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilEachTurnEndEffects)
        {
            if (GetCardEffect(timing) != null)
            {
                PlayerEffects.Add(GetCardEffect(timing));
            }
        }

        foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilOwnerTurnEndEffects)
        {
            if (GetCardEffect(timing) != null)
            {
                PlayerEffects.Add(GetCardEffect(timing));
            }
        }

        foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilOwnerActivePhaseEffects)
        {
            if (GetCardEffect(timing) != null)
            {
                PlayerEffects.Add(GetCardEffect(timing));
            }
        }

        foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilSecurityCheckEndEffects)
        {
            if (GetCardEffect(timing) != null)
            {
                PlayerEffects.Add(GetCardEffect(timing));
            }
        }

        foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilOpponentTurnEndEffects)
        {
            if (GetCardEffect(timing) != null)
            {
                PlayerEffects.Add(GetCardEffect(timing));
            }
        }

        foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilCalculateFixedCostEffect)
        {
            if (GetCardEffect(timing) != null)
            {
                PlayerEffects.Add(GetCardEffect(timing));
            }
        }

        foreach (ICardEffect cardEffect in PlayerEffects)
        {
            if (cardEffect.EffectSourceCard == null)
            {
                foreach (CardSource cardSource in GManager.instance.turnStateMachine.gameContext.ActiveCardList)
                {
                    if (cardSource.Owner == this && !cardSource.IsToken)
                    {
                        cardEffect.SetEffectSourceCard(cardSource);
                        break;
                    }
                }
            }
        }

        return PlayerEffects;
    }
    #endregion

    #region 消えないプレイヤーに掛かっている効果
    public List<Func<EffectTiming, ICardEffect>> PermanentEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region バトル終了時に消えるプレイヤーに掛かっている効果
    public List<Func<EffectTiming, ICardEffect>> UntilEndBattleEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region お互いのターン終了時に消えるプレイヤーに掛かっている効果
    public List<Func<EffectTiming, ICardEffect>> UntilEachTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region 自分のターン終了時に消えるプレイヤーに掛かっている効果
    public List<Func<EffectTiming, ICardEffect>> UntilOwnerTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region 自分のアクティブフェイズ終了時に消えるプレイヤーに掛かっている効果
    public List<Func<EffectTiming, ICardEffect>> UntilOwnerActivePhaseEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region 相手のターン終了時に消えるプレイヤーに掛かっている効果
    public List<Func<EffectTiming, ICardEffect>> UntilOpponentTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region カード使用後に消えるプレイヤーに掛かっている効果
    public List<Func<EffectTiming, ICardEffect>> UntilCalculateFixedCostEffect = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region セキュリティチェック後に消えるプレイヤーに掛かっている効果
    public List<Func<EffectTiming, ICardEffect>> UntilSecurityCheckEndEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #endregion

    #region トラッシュのカードオブジェクト
    public HandCard TrashHandCard;
    #endregion

    #region このプレイヤーから見た時のメモリー
    public int MemoryForPlayer
    {
        get
        {
            int memory = GManager.instance.turnStateMachine.gameContext.Memory;

            if (PlayerID == 0)
            {
                memory *= -1;
            }

            return memory;
        }
    }
    #endregion

    #region メモリーを固定の値にする
    public IEnumerator SetFixedMemory(int Memory, ICardEffect cardEffect)
    {
        if (MemoryForPlayer < Memory)
        {
            #region メモリーを+出来ないなら処理終了
            if (cardEffect != null)
            {
                if (!CanAddMemory(cardEffect))
                {
                    yield break;
                }
            }
            #endregion
        }

        if (PlayerID == 0)
        {
            GManager.instance.turnStateMachine.gameContext.Memory = -1 * Memory;
        }

        else
        {
            GManager.instance.turnStateMachine.gameContext.Memory = Memory;
        }

        if (GManager.instance.turnStateMachine.gameContext.Memory >= 10)
        {
            GManager.instance.turnStateMachine.gameContext.Memory = 10;
        }

        else if (GManager.instance.turnStateMachine.gameContext.Memory <= -10)
        {
            GManager.instance.turnStateMachine.gameContext.Memory = -10;
        }

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.memoryObject.SetMemory());
    }
    #endregion

    #region Can you increase memory?
    public bool CanAddMemory(ICardEffect cardEffect)
    {
        if (this.MemoryForPlayer >= 10)
        {
            return false;
        }

        #region Effects that impair memory
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                #region Effects of permanents in play
                foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is ICannotAddMemoryEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((ICannotAddMemoryEffect)cardEffect1).cannotAddMemory(this, cardEffect))
                            {
                                return false;
                            }
                        }
                    }
                }
                #endregion
            }

            #region player effect
            foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
            {
                if (cardEffect1 is ICannotAddMemoryEffect)
                {
                    if (cardEffect1.CanUse(null))
                    {
                        if (((ICannotAddMemoryEffect)cardEffect1).cannotAddMemory(this, cardEffect))
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion
        }
        #endregion

        return true;
    }
    #endregion

    #region メモリーを+する
    public IEnumerator AddMemory(int plusMemory, ICardEffect cardEffect)
    {
        if (plusMemory == 0)
        {
            yield break;
        }

        if (plusMemory >= 1)
        {
            #region メモリーを+出来ないなら処理終了
            if (cardEffect != null)
            {
                if (!CanAddMemory(cardEffect))
                {
                    yield break;
                }
            }
            #endregion
        }

        if (PlayerID == 0)
        {
            GManager.instance.turnStateMachine.gameContext.Memory -= plusMemory;
        }

        else
        {
            GManager.instance.turnStateMachine.gameContext.Memory += plusMemory;
        }

        if (GManager.instance.turnStateMachine.gameContext.Memory >= 10)
        {
            GManager.instance.turnStateMachine.gameContext.Memory = 10;
        }

        else if (GManager.instance.turnStateMachine.gameContext.Memory <= -10)
        {
            GManager.instance.turnStateMachine.gameContext.Memory = -10;
        }

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.memoryObject.SetMemory());
    }
    #endregion

    #region 払えるメモリーコストの上限
    public int MaxMemoryCost
    {
        get
        {
            int MaxMemoryCost = 0;

            if (PlayerID == 0)
            {
                MaxMemoryCost = Mathf.Abs(10 - GManager.instance.turnStateMachine.gameContext.Memory);
            }

            else
            {
                MaxMemoryCost = Mathf.Abs(-10 - GManager.instance.turnStateMachine.gameContext.Memory);
            }

            return MaxMemoryCost;
        }
    }
    #endregion

    #region コスト支払い後のメモリー予測値
    public int ExpectedMemory(int memoryCost)
    {
        int ExpectedMemory = GManager.instance.turnStateMachine.gameContext.Memory;

        if (PlayerID == 0)
        {
            ExpectedMemory += memoryCost;
        }

        else
        {
            ExpectedMemory -= memoryCost;
        }

        return ExpectedMemory;
    }
    #endregion

    #region デジタマを孵化できるか
    public bool CanHatch => DigitamaLibraryCards.Count >= 1 && GetBreedingAreaPermanents().Count == 0;
    #endregion

    #region デジモンを移動できるか
    public bool CanMove => GetBreedingAreaPermanents().Count(permanent => permanent.CanMove) >= 1 && fieldCardFrames.Count((frame) => frame.IsEmptyFrame()) >= 1;
    #endregion

    #region このターンにデジモンを進化させた回数
    public int DigivolveCount_ThisTurn { get; set; } = 0;
    #endregion

    #region 吸収進化でタップできるデジモンの条件(効果可否判定に使用)
    public bool CanTapWhenAbsorbEvolution_CheckAvailability(Permanent permanent, ICardEffect cardEffect)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (!permanent.IsSuspended && permanent.CanSuspend)
                {
                    if (permanent.TopCard.Owner.GetBattleAreaDigimons().Contains(permanent))
                    {
                        #region 吸収進化でタップできるデジモンの条件を変更させる効果
                        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                        {
                            foreach (Permanent permanent1 in player.GetFieldPermanents())
                            {
                                #region 場のパーマネントの効果
                                foreach (ICardEffect cardEffect1 in permanent1.EffectList(EffectTiming.None))
                                {
                                    if (cardEffect1 is ICanSuspendByDigisorptionEffect)
                                    {
                                        if (cardEffect1.CanUse(null))
                                        {
                                            if (((ICanSuspendByDigisorptionEffect)cardEffect1).isCheckAvailability())
                                            {
                                                if (((ICanSuspendByDigisorptionEffect)cardEffect1).canSuspendDigisorption(permanent, cardEffect))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }

                            #region プレイヤーの効果
                            foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
                            {
                                if (cardEffect1 is ICanSuspendByDigisorptionEffect)
                                {
                                    if (cardEffect1.CanUse(null))
                                    {
                                        if (((ICanSuspendByDigisorptionEffect)cardEffect1).isCheckAvailability())
                                        {
                                            if (((ICanSuspendByDigisorptionEffect)cardEffect1).canSuspendDigisorption(permanent, cardEffect))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        #endregion

                        if (permanent.TopCard.Owner == this)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region 吸収進化でタップできるデジモンの条件
    public bool CanTapWhenAbsorbEvolution(Permanent permanent, ICardEffect cardEffect)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (!permanent.IsSuspended && permanent.CanSuspend)
                {
                    if (permanent.TopCard.Owner.GetBattleAreaDigimons().Contains(permanent))
                    {
                        #region 吸収進化でタップできるデジモンの条件を変更させる効果
                        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                        {
                            foreach (Permanent permanent1 in player.GetFieldPermanents())
                            {
                                #region 場のパーマネントの効果
                                foreach (ICardEffect cardEffect1 in permanent1.EffectList(EffectTiming.None))
                                {
                                    if (cardEffect1 is ICanSuspendByDigisorptionEffect)
                                    {
                                        if (cardEffect1.CanUse(null))
                                        {
                                            if (!((ICanSuspendByDigisorptionEffect)cardEffect1).isCheckAvailability())
                                            {
                                                if (((ICanSuspendByDigisorptionEffect)cardEffect1).canSuspendDigisorption(permanent, cardEffect))
                                                {
                                                    return true;
                                                }

                                                else
                                                {
                                                    return false;
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }

                            #region プレイヤーの効果
                            foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
                            {
                                if (cardEffect1 is ICanSuspendByDigisorptionEffect)
                                {
                                    if (cardEffect1.CanUse(null))
                                    {
                                        if (!((ICanSuspendByDigisorptionEffect)cardEffect1).isCheckAvailability())
                                        {
                                            if (((ICanSuspendByDigisorptionEffect)cardEffect1).canSuspendDigisorption(permanent, cardEffect))
                                            {
                                                return true;
                                            }

                                            else
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        #endregion

                        if (permanent.TopCard.Owner == this)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region コストを減らせるか
    public bool CanReduceCost(List<Permanent> targetPermanents, CardSource cardSource)
    {
        #region コストを減らせなくさせる効果
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                #region 場のパーマネントの効果
                foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICannotReduceCostEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICannotReduceCostEffect)cardEffect).CannotReduceCost(this, targetPermanents, cardSource))
                            {
                                return false;
                            }
                        }
                    }
                }
                #endregion
            }

            #region プレイヤーの効果
            foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
            {
                if (cardEffect is ICannotReduceCostEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        if (((ICannotReduceCostEffect)cardEffect).CannotReduceCost(this, targetPermanents, cardSource))
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion
        }
        #endregion

        return true;
    }
    #endregion

    #region DP消滅効果の上限
    public int MaxDP_DeleteEffect(int maxDP, ICardEffect cardEffect)
    {
        int _maxDP = maxDP;

        #region DP消滅効果の上限を変更する効果

        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                #region 場のパーマネントの効果
                foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is IChangeDPDeleteEffectMaxDPEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            _maxDP = ((IChangeDPDeleteEffectMaxDPEffect)cardEffect1).GetMaxDP(_maxDP, cardEffect);
                        }
                    }
                }
                #endregion
            }

            #region プレイヤーの効果
            foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
            {
                if (cardEffect1 is IChangeDPDeleteEffectMaxDPEffect)
                {
                    if (cardEffect1.CanUse(null))
                    {
                        _maxDP = ((IChangeDPDeleteEffectMaxDPEffect)cardEffect1).GetMaxDP(_maxDP, cardEffect);
                    }
                }
            }
            #endregion
        }

        #endregion

        return _maxDP;
    }
    #endregion

    #region 進化条件を無視できるか
    public bool CanIgnoreDigivolutionRequirement(Permanent targetPermanent, CardSource cardSource)
    {
        #region 進化条件を無視できなくさせる効果
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                #region 場のパーマネントの効果
                foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is ICannotIgnoreDigivolutionConditionEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((ICannotIgnoreDigivolutionConditionEffect)cardEffect1).cannotIgnoreDigivolutionCondition(this, targetPermanent, cardSource))
                            {
                                return false;
                            }
                        }
                    }
                }
                #endregion
            }

            #region Player effect
            foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
            {
                if (cardEffect1 is ICannotIgnoreDigivolutionConditionEffect)
                {
                    if (cardEffect1.CanUse(null))
                    {
                        if (((ICannotIgnoreDigivolutionConditionEffect)cardEffect1).cannotIgnoreDigivolutionCondition(this, targetPermanent, cardSource))
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion
        }
        #endregion

        return true;
    }
    #endregion

    #region セキュリティを増やせるか
    public bool CanAddSecurity(ICardEffect cardEffect)
    {
        if (GManager.instance.turnStateMachine.gameContext.IsSecurityLooking)
        {
            return false;
        }

        #region セキュリティを増やせない効果
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                #region 場のパーマネントの効果
                foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is ICannotAddSecurityEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((ICannotAddSecurityEffect)cardEffect1).cannotAddSecurity(this, cardEffect))
                            {
                                return false;
                            }
                        }
                    }
                }
                #endregion
            }

            #region プレイヤーの効果
            foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
            {
                if (cardEffect1 is ICannotAddSecurityEffect)
                {
                    if (cardEffect1.CanUse(null))
                    {
                        if (((ICannotAddSecurityEffect)cardEffect1).cannotAddSecurity(this, cardEffect))
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion
        }
        #endregion

        return true;
    }
    #endregion

    #region セキュリティを減らせるか
    public bool CanReduceSecurity()
    {
        if (GManager.instance.turnStateMachine.gameContext.IsSecurityLooking)
        {
            return false;
        }

        return true;
    }
    #endregion
}

[Serializable]
public class ShowSupplyingEnergyIcon
{
    public CardColor cardColor;
    public Image SupplyingEnergyIcon;
    public Text SupplyingEnergyText;
}

[Serializable]
public class FieldCardFrame
{
    [Header("枠")]
    public GameObject Frame;

    [Header("選択状態表示枠")]
    public Image Frame_Select;

    [Header("枠ID")]
    public int FrameID;

    [Header("プレイヤー")]
    public Player player;

    public UnityAction<FieldCardFrame> OnClickAction { get; private set; }
    public static bool isBattleAreaFrameID(int FrameID)
    {
        return 0 <= FrameID && FrameID <= GManager.instance.You.fieldCardFrames.Count - 2;
    }
    public bool IsBattleAreaFrame()
    {
        return isBattleAreaFrameID(this.FrameID);
    }

    public bool isBreedingAreaFrame()
    {
        return !IsBattleAreaFrame();
    }

    Permanent framePermanent = null;

    public void SetFramePermanent(Permanent permanent)
    {
        framePermanent = permanent;
    }

    public Permanent GetFramePermanent()
    {
        /*
        foreach (Permanent permanent in player.GetFieldPermanents())
        {
            if (permanent.PermanentFrame == this)
            {
                return permanent;
            }
        }
        */

        if (framePermanent != null)
        {
            return framePermanent;
        }

        return null;
    }

    public bool IsEmptyFrame()
    {
        return GetFramePermanent() == null;
    }

    public void AddClickTarget(UnityAction<FieldCardFrame> OnClickAction)
    {
        this.OnClickAction = OnClickAction;

        if (Frame != null)
        {
            BoxCollider boxCollider = Frame.GetComponent<BoxCollider>();

            if (boxCollider != null)
            {
                boxCollider.enabled = true;
            }
        }
    }

    public void RemoveClickTarget()
    {
        this.OnClickAction = null;

        if (Frame != null)
        {
            BoxCollider boxCollider = Frame.GetComponent<BoxCollider>();

            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }
        }
    }

    public void OffFrame_Select()
    {
        if (Frame_Select != null)
        {
            Frame_Select.gameObject.SetActive(false);
        }
    }

    public void OnFrame_Select(Color color)
    {
        if (Frame_Select != null)
        {
            Frame_Select.gameObject.SetActive(true);
            Frame_Select.color = color;
            //Frame_Select.color = new Color(color.r, color.g, color.b, 0);
        }
    }

    public Vector3 GetLocalCanvasPosition()
    {
        Vector3 vector = Frame.transform.localPosition + Frame.transform.parent.localPosition + Frame.transform.parent.parent.localPosition;

        return Frame.transform.localPosition + Frame.transform.parent.localPosition + Frame.transform.parent.parent.localPosition;
    }

    public FieldCardFrame FacingFrame
    {
        get
        {
            if (this.IsBattleAreaFrame())
            {
                return player.fieldCardFrames[this.FrameID + 4];
            }

            else
            {
                return player.fieldCardFrames[this.FrameID - 4];
            }

            return null;
        }
    }
}
