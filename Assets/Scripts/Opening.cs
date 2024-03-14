using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;
public class Opening : MonoBehaviour
{
    [Header("読み込み中表示オブジェクト")]
    public LoadingObject LoadingObject;

    [Header("読み込み中表示オブジェクト(明るい)")]
    public LoadingObject LoadingObject_light;

    [Header("読み込み中表示オブジェクト_アンロード")]
    public LoadingObject LoadingObject_Unload;

    [Header("ホーム")]
    public HomeMode home;

    [Header("デッキ")]
    public DeckMode deck;

    [Header("バトル")]
    public BattleMode battle;

    [Header("クリック時エフェクト")]
    public GameObject OnClickEffect;

    [Header("キャンバス")]
    public RectTransform canvasRect;

    [Header("YesNoオブジェクト")]
    [SerializeField] List<YesNoObject> YesNoObjects = new List<YesNoObject>();
    [Header("Background Particle effects")]
    [SerializeField] List<ParticleSystem> _backgroundParticles = new List<ParticleSystem>();
    [SerializeField] CardImagePanel _cardImagePanel;

    public CheckUpdate checkUpdate;

    public static Opening instance = null;

    public Text VerText;

    public OptionPanel optionPanel;
    public PatchNotes patchNotesPanel;

    public GameObject ModeButtons;

    public Vector3 DeckInfoPrefabStartScale;

    public Vector3 DeckInfoPrefabExpandScale;

    [SerializeField] Transform camerasParent;

    public Title title;

    [Header("背景Image")]
    public List<Image> BackgroundImages = new List<Image>();

    public GameObject openingObject;

    [Header("TitleButtonSE")]
    public AudioClip TitleButtonSE;

    [Header("DrawSE")]
    public AudioClip DrawSE;

    [Header("MoveSE")]
    public AudioClip MoveSE;

    [Header("DecisionSE")]
    public AudioClip DecisionSE;

    [Header("CancelSE")]
    public AudioClip CancelSE;

    [Header("BGM")]
    public AudioClip bgm;

    [Header("BGMObject")]
    public BGMObject OpeningBGM;
    private void Awake()
    {
        instance = this;

        if (openingCameras.Count >= 1)
        {
            MainCamera = openingCameras[0];
        }
    }

    int count = 0;
    int UpdateFrame = 5;
    private void Update()
    {
        #region 数フレームに一度だけ更新
        count++;

        if (count < UpdateFrame)
        {
            return;
        }

        else
        {
            count = 0;
        }
        #endregion

        GetRayCast();

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
    }

    YesNoObject ActiveYesNoObject()
    {
        foreach (YesNoObject yesNoObject in YesNoObjects)
        {
            if (!yesNoObject.gameObject.activeSelf)
            {
                return yesNoObject;
            }
        }

        if (YesNoObjects.Count >= 1)
        {
            return YesNoObjects[0];
        }

        return null;
    }

    public void OffYesNoObjects()
    {
        foreach (YesNoObject yesNoObject in YesNoObjects)
        {
            yesNoObject.Close_(false);
        }
    }

    public void SetUpActiveYesNoObject(List<UnityAction> OnClickActions, List<string> CommandTexts, string _InfoText, bool CanClose)
    {
        YesNoObject activeYesNoObject = ActiveYesNoObject();

        if (activeYesNoObject != null)
        {
            activeYesNoObject.Off();
            activeYesNoObject.transform.parent.gameObject.SetActive(true);
            activeYesNoObject.transform.SetSiblingIndex(activeYesNoObject.transform.parent.childCount - 1);
            activeYesNoObject.SetUpYesNoObject(OnClickActions, CommandTexts, _InfoText, CanClose);
        }
    }

    void GetRayCast()
    {
        if (GManager.instance != null)
        {
            return;
        }

        if (ContinuousController.instance == null)
        {
            return;
        }

        //bool isRay = false;

        List<RaycastResult> results = new List<RaycastResult>();
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        // マウスポインタの位置にレイ飛ばし、ヒットしたものを保存
        pointer.position = Input.mousePosition;
        EventSystem.current.RaycastAll(pointer, results);

        // ヒットしたUIの名前
        //isRay = true;

        CardPrefab_CreateDeck cardPrefab = null;

        foreach (RaycastResult target in results)
        {
            if (target.gameObject.CompareTag("CardPrefab_CreateDeck_CardImage"))
            {
                cardPrefab = target.gameObject.transform.parent.parent.parent.GetComponent<CardPrefab_CreateDeck>();

                /*
                if (cardPrefab != null)
                {
                    if (Opening.instance.deck.trialDraw.gameObject.activeSelf)
                    {
                        if (cardPrefab.transform.parent != Opening.instance.deck.trialDraw.CardScroll.content)
                        {
                            cardPrefab = null;
                        }
                    }

                    else if (Opening.instance.deck.deckListPanel.gameObject.activeSelf)
                    {
                        if (cardPrefab.transform.parent != Opening.instance.deck.deckListPanel.DeckScroll.content)
                        {
                            cardPrefab = null;
                        }
                    }
                }
                */

                if (cardPrefab != null)
                {
                    break;
                }
            }
        }

        //isRay = cardPrefab != null;

        if (cardPrefab != null)
        {
            if (deck.editDeck.isDragging)
            {
                cardPrefab = null;
            }

            else
            {
                foreach (RaycastResult target in results)
                {
                    if (target.gameObject.layer == LayerMask.NameToLayer("IgnoreCardPrefabRaycast"))
                    {
                        if (DropArea.IsChild(Opening.instance.deck.trialDraw.gameObject, target.gameObject))
                        {
                            if (cardPrefab.transform.parent == Opening.instance.deck.trialDraw.CardScroll.content)
                            {
                                continue;
                            }
                        }

                        else if (DropArea.IsChild(Opening.instance.deck.deckListPanel.gameObject, target.gameObject))
                        {
                            if (cardPrefab.transform.parent == Opening.instance.deck.deckListPanel.DeckScroll.content)
                            {
                                continue;
                            }

                            if (cardPrefab.transform.parent == Opening.instance.deck.trialDraw.CardScroll.content)
                            {
                                continue;
                            }
                        }

                        cardPrefab = null;
                        break;
                    }
                }
            }
        }

        if (cardPrefab != null)
        {
            if (Opening.instance.deck.editDeck.gameObject.activeSelf && Opening.instance.deck.editDeck.isEditting && !Opening.instance.deck.editDeck.isDragging)
            {
                for (int i = 0; i < Opening.instance.deck.editDeck.CardPoolCardPrefabs_CreateDeck.Count; i++)
                {
                    if (Opening.instance.deck.editDeck.CardPoolCardPrefabs_CreateDeck[i] != cardPrefab)
                    {
                        Opening.instance.deck.editDeck.CardPoolCardPrefabs_CreateDeck[i]._OnExit();
                    }
                }

                for (int i = 0; i < Opening.instance.deck.editDeck.DeckScroll.content.childCount; i++)
                {
                    if (Opening.instance.deck.editDeck.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>() != cardPrefab)
                    {
                        Opening.instance.deck.editDeck.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                    }
                }
            }

            if (Opening.instance.deck.deckListPanel.gameObject.activeSelf)
            {
                for (int i = 0; i < Opening.instance.deck.deckListPanel.DeckScroll.content.childCount; i++)
                {
                    if (Opening.instance.deck.deckListPanel.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>() != cardPrefab)
                    {
                        Opening.instance.deck.deckListPanel.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                    }
                }
            }

            if (Opening.instance.deck.trialDraw.gameObject.activeSelf)
            {
                for (int i = 0; i < Opening.instance.deck.trialDraw.CardScroll.content.childCount; i++)
                {
                    if (Opening.instance.deck.trialDraw.CardScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>() != cardPrefab)
                    {
                        Opening.instance.deck.trialDraw.CardScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                    }
                }
            }

            cardPrefab.OnEnter();
        }

        //if (!isRay)
        else
        {
            if (Opening.instance.deck.editDeck.gameObject.activeSelf && Opening.instance.deck.editDeck.isEditting)
            {
                for (int i = 0; i < Opening.instance.deck.editDeck.CardPoolCardPrefabs_CreateDeck.Count; i++)
                {
                    Opening.instance.deck.editDeck.CardPoolCardPrefabs_CreateDeck[i]._OnExit();
                }

                for (int i = 0; i < Opening.instance.deck.editDeck.DeckScroll.content.childCount; i++)
                {
                    Opening.instance.deck.editDeck.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                }
            }

            if (Opening.instance.deck.deckListPanel.gameObject.activeSelf)
            {
                for (int i = 0; i < Opening.instance.deck.deckListPanel.DeckScroll.content.childCount; i++)
                {
                    Opening.instance.deck.deckListPanel.DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                }
            }

            if (Opening.instance.deck.trialDraw.gameObject.activeSelf)
            {
                for (int i = 0; i < Opening.instance.deck.trialDraw.CardScroll.content.childCount; i++)
                {
                    Opening.instance.deck.trialDraw.CardScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>()._OnExit();
                }
            }
        }
    }

    public void OpenRuleBook()
    {
        OpenURL openURL = GetComponent<OpenURL>();

        openURL.Open();
    }

    public List<Camera> openingCameras
    {
        get
        {
            List<Camera> openingCameras = new List<Camera>();

            for (int i = 0; i < camerasParent.childCount; i++)
            {
                Camera camera = camerasParent.GetChild(i).GetComponent<Camera>();

                if (camera != null)
                {
                    openingCameras.Add(camera);
                }
            }

            return openingCameras;
        }
    }

    public Camera MainCamera { get; set; }

    public void PlayDecisionSE()
    {
        ContinuousController.instance.PlaySE(Opening.instance.DecisionSE);
    }

    public void PlayCancelSE()
    {
        ContinuousController.instance.PlaySE(Opening.instance.CancelSE);
    }
    public void OffModeButtons()
    {
        ModeButtons.SetActive(false);
    }

    public void OnModeButtons()
    {
        ModeButtons.SetActive(true);
    }

    public void CreateOnClickEffect()
    {
        GameObject effect = Instantiate(OnClickEffect, canvasRect.transform);

        var mousePos = Input.mousePosition;
        var magnification = canvasRect.sizeDelta.x / Screen.width;
        mousePos.x = mousePos.x * magnification - canvasRect.sizeDelta.x / 2;
        mousePos.y = mousePos.y * magnification - canvasRect.sizeDelta.y / 2;
        mousePos.z = 0;// -0.5f;// transform.localPosition.z;

        effect.transform.localPosition = mousePos;

        effect.transform.localRotation = Quaternion.EulerAngles(new Vector3(77, 0, 0));

        StartCoroutine(Effects.DeleteCoroutine(effect, null));
    }

    private void Start()
    {
        foreach (YesNoObject yesNoObject in YesNoObjects)
        {
            yesNoObject.Close_(false);
        }

        ChangeBackground();

        home.OffHome();

        deck.OffDeck();

        battle.OffBattle();

        _cardImagePanel.Close_(false);

        StartCoroutine(Init());
    }

    async void ChangeBackground()
    {
        Sprite backgroundSprite = await StreamingAssetsUtility.GetSprite("Background_home");

        if (backgroundSprite != null)
        {
            foreach (Image BackgroundImage in BackgroundImages)
            {
                BackgroundImage.sprite = backgroundSprite;
            }
        }
    }

    public IEnumerator Init()
    {
        OpeningBGM.StopPlayBGM();
        yield return StartCoroutine(LoadingObject.StartLoading("Now Loading"));

        LoadingObject_light.gameObject.SetActive(false);
        LoadingObject_Unload.gameObject.SetActive(false);

        yield return StartCoroutine(ContinuousController.LoadCoroutine());

        yield return new WaitWhile(() => ContinuousController.instance == null);

        // ContinuousController.instance.LoadVolume();

        home.OffHome();

        optionPanel.Init();

        patchNotesPanel.Init();

        yield return StartCoroutine(deck.editDeck.InitEditDeck());

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();

            while (PhotonNetwork.IsConnected)
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.1f);

        VerText.text = $"Ver{ContinuousController.instance.GameVerString}";

        deck.SetUpDeckMode();

        deck.OffDeck();
        title.SetUpTitle();

        LoadCardImages();

        yield return new WaitForSeconds(0.15f);

        yield return StartCoroutine(LoadingObject.EndLoading());
    }

    async void LoadCardImages()
    {
#if UNITY_EDITOR
        foreach (CEntity_Base cEntity_Base in ContinuousController.instance.CardList)
        {
            cEntity_Base.HasLoadStarted = false;
        }
#endif

        foreach (DeckData deckData in ContinuousController.instance.DeckDatas)
        {
            CEntity_Base keyCard = deckData.KeyCard;

            if (keyCard != null)
            {
                keyCard.HasLoadStarted = false;
                await keyCard.LoadCardImage();
            }
        }
    }
}
