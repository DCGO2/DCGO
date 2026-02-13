using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Diagnostics;

public class FieldPermanentCard : MonoBehaviour
{
    [Header("選択状態アウトライン")]
    public Image Outline_Select;

    [Header("カード画像")]
    public Image CardImage;

    [Header("ブロッカーエフェクト")]
    public GameObject BlockerEffect;

    [Header("エナジー個数Text")]
    public Text EnergyCountText;

    [Header("コライダー")]
    public GameObject Collider;

    [Header("アニメーター")]
    public Animator anim;

    [Header("コマンドパネル")]
    public CommandPanel fieldUnitCommandPanel;

    [Header("スキル名Text")]
    public Text SkillNameText;

    [Header("DPテキスト")]
    public Text DPText;

    [Header("DP枠")]
    public List<Image> DPBackground_color = new List<Image>();

    [Header("進化元枚テキスト")]
    public Text EvoRootCountText;

    [Header("進化元枚枠")]
    public Image EvoRootCountBackground;

    [Header("レベルテキスト")]
    public TextMeshProUGUI LevelText;

    [Header("Link Elements")]
    public GameObject LinkedObject;
    public Image LinkIcon;

    [Header("表示親")]
    public GameObject Parent;

    [Header("スキル使用エフェクト")]
    public GameObject UsingSkillEffect;

    [Header("進化元追加エフェクト")]
    public ParticleSystem addDigivolutionCardsEffect;

    //初期スケール
    public Vector3 StartScale { get; set; }

    [Header("パーマネント番号テキスト")]
    public TextMeshProUGUI permanentIndexText;

    [Header("タップオブジェクト")]
    public GameObject TapObject;

    [Header("ダメージテキスト")]
    public Text DirectStrikeText;

    [Header("召喚酔い")]
    public GameObject SummonSicknessObject;

    [Header("進化予定オブジェクト")]
    public GameObject WillEvolutionObject;

    [Header("消滅予定オブジェクト")]
    public GameObject WillBeDeletedObject;

    [Header("デッキバウンス予定オブジェクト")]
    public GameObject WillBeDeckBounceObject;

    [Header("手札バウンス予定オブジェクト")]
    public GameObject WillBeHandBounceObject;

    [Header("場を離れる予定オブジェクト")]
    public GameObject WillRemoveFieldObject;

    [Header("アンタップ予定オブジェクト")]
    public GameObject WillUntapObject;

    public List<TextCardColorMaterial> textCardColorMaterials = new List<TextCardColorMaterial>();

    public UnityAction<FieldPermanentCard> OnClickAction;
    public Permanent ThisPermanent { get; set; }
    public bool IsEffectPlaying { get; set; }
    private void Awake()
    {
        RemoveSelectEffect();

        CloseCommandPanel();

        Outline_Select.gameObject.SetActive(false);

        OffUsingSkillEffect();

        OffSkillName();

        if (BlockerEffect != null)
        {
            BlockerEffect.gameObject.SetActive(false);
        }

        if (EvoRootCountText != null)
        {
            EvoRootCountText.transform.parent.gameObject.SetActive(false);
        }

        if (LinkedObject != null)
        {
            LinkedObject.SetActive(false);
        }

        if (DirectStrikeText != null)
        {
            DirectStrikeText.transform.parent.parent.gameObject.SetActive(true);
            DirectStrikeText.transform.parent.gameObject.SetActive(false);
        }

        if (SummonSicknessObject != null)
        {
            SummonSicknessObject.gameObject.SetActive(false);
        }

        if (WillEvolutionObject != null)
        {
            WillEvolutionObject.SetActive(false);
        }

        if (WillBeDeletedObject != null)
        {
            WillBeDeletedObject.SetActive(false);
        }

        if (WillBeDeckBounceObject != null)
        {
            WillBeDeckBounceObject.SetActive(false);
        }

        if (WillBeHandBounceObject != null)
        {
            WillBeHandBounceObject.SetActive(false);
        }

        if (WillRemoveFieldObject != null)
        {
            WillRemoveFieldObject.SetActive(false);
        }

        if (WillUntapObject != null)
        {
            WillUntapObject.SetActive(false);
        }

        OffPermanentIndexText();
        
        //Events
        GManager.OnReverseOpponentsCardsChanged += SetTransformRotation;
        GManager.OnCardFlippedChanged += SetCardIsFlipped;
    }

    public IEnumerator ShowAddDigivolutionCardEffect()
    {
        if (addDigivolutionCardsEffect != null)
        {
            addDigivolutionCardsEffect.gameObject.SetActive(true);
            addDigivolutionCardsEffect.Play();
        }

        yield return new WaitForSeconds(0.1f);
    }

    public void OffPermanentIndexText()
    {
        if (permanentIndexText != null)
        {
            permanentIndexText.transform.parent.gameObject.SetActive(false);
        }
    }

    public void SetPermanentIndexText(List<Permanent> permanents)
    {
        if (permanentIndexText != null)
        {
            if (ThisPermanent != null)
            {
                if (ThisPermanent.TopCard != null)
                {
                    int index = permanents.IndexOf(ThisPermanent) + 1;

                    permanentIndexText.transform.parent.parent.gameObject.SetActive(true);
                    permanentIndexText.transform.parent.gameObject.SetActive(true);
                    permanentIndexText.text = $"{index}";
                }
            }
        }
    }
    void RotateSkillNameText()
    {
        SkillNameText.transform.parent.localRotation = Quaternion.Euler(0, 0, -1 * this.transform.localRotation.eulerAngles.z);
    }

    public void OnSkillName(ICardEffect cardEffect)
    {
        if (SkillNameText != null && ThisPermanent != null)
        {
            RotateSkillNameText();

            SkillNameText.transform.parent.gameObject.SetActive(true);
            SkillNameText.text = cardEffect.EffectName;
        }
    }
    public void OffSkillName()
    {
        if (SkillNameText != null)
        {
            SkillNameText.transform.parent.gameObject.SetActive(false);
        }
    }

    public void OffUsingSkillEffect()
    {
        if (UsingSkillEffect != null)
        {
            UsingSkillEffect.SetActive(false);
        }
    }

    public void OnUsingSkillEffect()
    {
        if (UsingSkillEffect != null)
        {
            UsingSkillEffect.SetActive(true);
        }
    }

    float _validPressTime = 0.5f;
    float _requiredTime = 0.0f;
    bool _pressing = false;

    #region Processing on click

    #region Processing when clicked
    public void OnClick()
    {
        if (ThisPermanent != null)
        {
            if (ThisPermanent.TopCard != null)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    OnClickAction?.Invoke(this);
                }

                else if (Input.GetMouseButtonUp(1))
                {
                    OnRightClicked();
                }
            }
        }
    }

    void OnRightClicked()
    {
        if (!ThisPermanent.TopCard.IsFlipped || ThisPermanent.TopCard.Owner.isYou)
        {
            GManager.instance.pokemonDetail.OpenUnitDetail(ThisPermanent);
        }
    }
    #endregion

    #region Added processing when clicked
    public void AddClickTarget(UnityAction<FieldPermanentCard> _OnClickAction)
    {
        OnClickAction = _OnClickAction;
    }
    #endregion

    #region Delete the process when clicked
    public void RemoveClickTarget()
    {
        OnClickAction = null;
    }
    #endregion
    #endregion

    #region Set permanent data
    public void SetPermanentData(Permanent permanent, bool updateIsTapped)
    {
        ThisPermanent = permanent;

        SetTransformRotation();
        SetCardIsFlipped();
        ShowPermanentData(updateIsTapped);
    }
    #endregion

    #region Reflect permanent data on UI
    bool _oldTurnSuspendedCards = false;

    void SetTransformRotation()
    {
        if (ThisPermanent == null)
            return;

        if (ThisPermanent.TopCard == null)
            return;

        if (ThisPermanent.TopCard.Owner.isYou)
            return;

        if (ContinuousController.instance == null)
            return;


        if (ContinuousController.instance.reverseOpponentsCards)
            Parent.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
        else
            Parent.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }

    async void SetCardIsFlipped()
    {
        if (ThisPermanent == null)
            return;

        if (ThisPermanent.TopCard == null)
            return;

        if (ContinuousController.instance == null)
            return;

        if (CardImage == null)
            return;

        // card image
        if (ThisPermanent.TopCard.IsFlipped)
        {
            if (CardImage.sprite != ContinuousController.instance.ReverseCard)
                CardImage.sprite = ContinuousController.instance.ReverseCard;
        }

        else
        {
            CardImage.sprite = await ThisPermanent.TopCard.GetCardSprite();
        }

        CardImage.gameObject.SetActive(true);
    }

    void SetCardSuspended(bool updateIsTapped)
    {
        if (ThisPermanent == null)
            return;

        if (ThisPermanent.TopCard == null)
            return;

        if (ContinuousController.instance == null)
            return;

        if (ThisPermanent.IsSuspended)
        {
            if (TapObject != null)
                TapObject.SetActive(true);

            if (updateIsTapped)
            {
                bool turnSuspendedCards = ContinuousController.instance != null && ContinuousController.instance.turnSuspendedCards;

                if (anim != null)
                {
                    if (ThisPermanent.OldIsSuspended != ThisPermanent.IsSuspended || _oldTurnSuspendedCards != turnSuspendedCards)
                    {
                        ThisPermanent.OldIsSuspended = ThisPermanent.IsSuspended;
                        _oldTurnSuspendedCards = turnSuspendedCards;

                        if (turnSuspendedCards)
                        {
                            if (anim.GetInteger("Tap") != 1)
                                anim.SetInteger("Tap", 1);
                        }

                        else
                        {
                            if (anim.GetInteger("Tap") != -1)
                                anim.SetInteger("Tap", -1);
                        }
                    }
                }
            }
        }

        else
        {
            if (TapObject != null)
                TapObject.SetActive(false);

            if (ContinuousController.instance != null)
            {
                if (ContinuousController.instance.turnSuspendedCards)
                {
                    if (ThisPermanent.OldIsSuspended != ThisPermanent.IsSuspended)
                    {
                        ThisPermanent.OldIsSuspended = ThisPermanent.IsSuspended;

                        if (anim != null)
                        {
                            if (anim.GetInteger("Tap") != -1)
                                anim.SetInteger("Tap", -1);
                        }
                    }
                }
            }
        }
    }

    public void ShowPermanentData(bool updateIsTapped)
    {
        if (ThisPermanent == null)
        {
            return;
        }

        if (ThisPermanent.TopCard != null)
        {
            SetCardSuspended(updateIsTapped);


            if (ThisPermanent.TopCard.IsFlipped)
            {
                DPText.transform.parent.gameObject.SetActive(false);
                LevelText.transform.parent.gameObject.SetActive(false);
                EvoRootCountText.transform.parent.gameObject.SetActive(false);
                LinkedObject.SetActive(false);
                return;
            }

            //DP
            if (ThisPermanent.IsDigimon)
            {
                int DP = ThisPermanent.DP;

                if (DP >= 0)
                {
                    DPText.transform.parent.gameObject.SetActive(true);

                    if (DPText.text != DP.ToString())
                        DPText.text = DP.ToString();

                    for (int i = 0; i < DPBackground_color.Count; i++)
                    {
                        CardColor cardColor = CardColor.None;

                        if (i < ThisPermanent.TopCard.CardColors.Count)
                        {
                            cardColor = ThisPermanent.TopCard.CardColors[i];
                            float fillAmount = (float)((i + 1) / (float)ThisPermanent.TopCard.CardColors.Count);

                            if (DPBackground_color[i].color != DataBase.CardColor_ColorLightDictionary[cardColor])
                                DPBackground_color[i].color = DataBase.CardColor_ColorLightDictionary[cardColor];

                            DPBackground_color[i].fillAmount = fillAmount;
                            DPBackground_color[i].gameObject.SetActive(true);
                        }

                        else
                        {
                            DPBackground_color[i].gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    DPText.transform.parent.gameObject.SetActive(false);
                }                
            }

            else
            {
                DPText.transform.parent.gameObject.SetActive(false);
            }

            //Level
            if (ThisPermanent.IsDigimon && ThisPermanent.TopCard.HasLevel)
            {
                LevelText.transform.parent.gameObject.SetActive(true);

                if(LevelText.text != ThisPermanent.Level.ToString())
                    LevelText.text = ThisPermanent.Level.ToString();

                foreach (TextCardColorMaterial textCardColorMaterial in textCardColorMaterials)
                {
                    if (textCardColorMaterial.cardColor == ThisPermanent.TopCard.CardColors[0])
                    {
                        LevelText.fontSharedMaterial = new Material(textCardColorMaterial.material);
                        break;
                    }
                }
            }

            else
            {
                LevelText.transform.parent.gameObject.SetActive(false);
            }

            //Digivolution Count
            if (EvoRootCountText != null)
            {
                if (ThisPermanent.DigivolutionCards.Count >= 1)
                {
                    EvoRootCountText.transform.parent.gameObject.SetActive(true);

                    if(EvoRootCountText.text != $"×{ThisPermanent.DigivolutionCards.Count}")
                        EvoRootCountText.text = $"×{ThisPermanent.DigivolutionCards.Count}";

                    if(EvoRootCountBackground.color != DataBase.CardColor_ColorLightDictionary[ThisPermanent.TopCard.CardColors[0]])
                        EvoRootCountBackground.color = DataBase.CardColor_ColorLightDictionary[ThisPermanent.TopCard.CardColors[0]];
                }

                else
                {
                    EvoRootCountText.transform.parent.gameObject.SetActive(false);
                }
            }

            //Links
            if (LinkedObject != null)
            {
                if (ThisPermanent.LinkedCards.Count > 0)
                {
                    LinkedObject.SetActive(true);

                    //if (LinkedDPText.text != $"+{ThisPermanent.LinkedDP}")
                    //    LinkedDPText.text = $"+{ThisPermanent.LinkedDP}";

                    if (LinkIcon.color != DataBase.CardColor_ColorLightDictionary[ThisPermanent.TopCard.CardColors[0]])
                        LinkIcon.color = DataBase.CardColor_ColorLightDictionary[ThisPermanent.TopCard.CardColors[0]];
                    
                }

                else
                {
                    LinkedObject.SetActive(false);
                }
            }

            //Summoning Sickness
            if (SummonSicknessObject != null)
            {
                bool isActive = false;

                if (ThisPermanent.EnterFieldTurnCount == GManager.instance.turnStateMachine.TurnCount)
                {
                    if (GManager.instance.turnStateMachine.gameContext.TurnPlayer == ThisPermanent.TopCard.Owner)
                    {
                        if (!ThisPermanent.HasRush)
                        {
                            if (ThisPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(ThisPermanent))
                            {
                                isActive = true;
                            }
                        }
                    }

                }

                SummonSicknessObject.SetActive(isActive);
            }

            //Blocker
            if (BlockerEffect != null)
            {
                bool isActive = false;

                if (ThisPermanent.IsDigimon)
                {
                    if (ThisPermanent.HasBlocker)
                    {
                        //if (thisPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(thisPermanent))
                        {
                            isActive = true;
                        }
                    }
                }

                BlockerEffect.SetActive(isActive);
            }
        }
    }
    #endregion

    #region Automatically reflect card data
    int _frameCount = 0;
    int _updateFrame = 75;
    public bool destroyed { get; set; } = false;
    public bool skipDestroy { get; set; } = false;
    public bool skipUpdate { get; set; } = false;

    //TODO: Pretty poor gargage collection, need to optimize - MB
    private void LateUpdate()
    {
        if (skipUpdate)
        {
            return;
        }

        #region exception check
        if (destroyed)
        {
            return;
        }

        if (ThisPermanent == null)
        {
            return;
        }

        if (ThisPermanent.TopCard == null)
        {
            if (!destroyed && !skipDestroy)
            {
                StartCoroutine(DestroyCoroutine());
                return;
            }
        }
        #endregion

        //Skill name
        if (SkillNameText.transform.parent.gameObject.activeSelf)
        {
            RotateSkillNameText();
        }

        if (DirectStrikeText != null)
        {
            if (ThisPermanent.IsSuspended)
            {
                DirectStrikeText.transform.parent.parent.transform.localPosition = new Vector2(DirectStrikeText.transform.parent.parent.transform.localPosition.x, 24);
            }

            else
            {
                DirectStrikeText.transform.parent.parent.transform.localPosition = new Vector2(DirectStrikeText.transform.parent.parent.transform.localPosition.x, 78);
            }

            DirectStrikeText.transform.parent.parent.localRotation = Quaternion.Euler(0, 0, -1 * this.transform.localRotation.eulerAngles.z);
        }

        if (ThisPermanent.TopCard.Owner.GetFieldPermanents().Count >= ThisPermanent.TopCard.Owner.fieldCardFrames.Count * 0.7f)
        {
            _updateFrame = 115;
        }

        else
        {
            _updateFrame = 75;
        }

        #region Get long press
#if !UNITY_EDITOR && UNITY_ANDROID
        if (pressing)
        {
            if(requiredTime < Time.time)
            {
                OnRightClicked();
                pressing = false;
            }
        }
#endif
        #endregion

        #region Reflected only once every few frames
        _frameCount++;

        if (_frameCount < _updateFrame)
        {
            return;
        }

        else
        {
            _frameCount = 0;
        }
        #endregion

        ShowPermanentData(true);
    }
    #endregion

    public void PointerDown(BaseEventData eventData)
    {
        if (!_pressing)
        {
            _pressing = true;
            _requiredTime = Time.time + _validPressTime;
        }

        else
        {
            _pressing = false;
        }
    }

    public void PointerUp(BaseEventData eventData)
    {
        if (_pressing)
        {
            _pressing = false;
        }
    }

    public void PointerExit(BaseEventData eventData)
    {
        if (_pressing)
        {
            _pressing = false;
        }
    }

    #region このオブジェクトを削除
    IEnumerator DestroyCoroutine()
    {
        yield return null;
        destroyed = true;
        Destroy(this.gameObject);
    }
    #endregion

    #region 選択状態
    public bool isExpand { get; set; }

    #region 選択状態
    public void OnSelectEffect(float expand)
    {
        if (isExpand)
        {
            return;
        }

        Outline_Select.gameObject.SetActive(true);

        if (this.gameObject.activeSelf)
        {
            StartCoroutine(ExpandCoroutine(expand));
        }

        isExpand = true;

        SetOrangeOutline();
    }
    #endregion

    #region オレンジアウトライン
    public void SetOrangeOutline()
    {
        Outline_Select.color = DataBase.SelectColor_Orange;
    }
    #endregion

    #region　ブルーアウトライン
    public void SetBlueOutline()
    {
        Outline_Select.color = DataBase.SelectColor_Blue;
    }
    #endregion

    #region 大きくする
    IEnumerator ExpandCoroutine(float expand)
    {
        float ExpandTime = 0.06f;

        float targetScale = StartScale.x * expand;

        float expandSpeed = (targetScale - transform.localScale.x) / ExpandTime;

        while (transform.localScale.x < targetScale)
        {
            transform.localScale += new Vector3(expandSpeed * Time.deltaTime, expandSpeed * Time.deltaTime, 0);

            yield return new WaitForSeconds(Time.deltaTime);

            if (!isExpand)
            {
                transform.localScale = StartScale;
                yield break;
            }
        }

        transform.localScale = new Vector3(targetScale, targetScale, 1);

        if (!isExpand)
        {
            transform.localScale = StartScale;
            yield break;
        }
    }
    #endregion

    #region 選択状態リセット
    public void RemoveSelectEffect()
    {
        Outline_Select.gameObject.SetActive(false);

        if (!isExpand)
        {
            return;
        }

        transform.localScale = StartScale;

        isExpand = false;
    }
    #endregion
    #endregion

    #region キャンバス座標への変換
    public Vector3 GetLocalCanvasPosition()
    {
        return this.transform.localPosition + this.transform.parent.localPosition;
    }
    #endregion

    #region コマンドパネルを閉じる
    public void CloseCommandPanel()
    {
        if (fieldUnitCommandPanel != null)
        {
            fieldUnitCommandPanel.CloseCommandPanel();
        }
    }
    #endregion

    #region ドラッグ
    public UnityAction<FieldPermanentCard> OnBeginDragAction { get; set; }
    public UnityAction<FieldPermanentCard, List<DropArea>> OnDragAction { get; set; }
    public UnityAction<FieldPermanentCard, List<DropArea>> OnEndDragAction { get; set; }

    public void RemoveDragTarget()
    {
        this.OnBeginDragAction = null;
        this.OnDragAction = null;
        this.OnEndDragAction = null;
    }

    public void AddDragTarget(UnityAction<FieldPermanentCard> OnBeginDragAction, UnityAction<FieldPermanentCard, List<DropArea>> OnDragAction, UnityAction<FieldPermanentCard, List<DropArea>> OnEndDragAction)
    {
        this.OnBeginDragAction = OnBeginDragAction;
        this.OnDragAction = OnDragAction;
        this.OnEndDragAction = OnEndDragAction;
    }

    public void OnBeginDrag()
    {
        OnBeginDragAction?.Invoke(this);
    }

    public void OnDrag()
    {
        OnDragAction?.Invoke(this, Draggable.GetRaycastArea());
    }

    public void OnEndDrag()
    {
        OnEndDragAction?.Invoke(this, Draggable.GetRaycastArea());
    }
    #endregion
}

[System.Serializable]
public class TextCardColorMaterial
{
    public CardColor cardColor;
    public Material material;
}


