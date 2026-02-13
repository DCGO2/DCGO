using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class SecurityObject : MonoBehaviour
{
    [Header("プレイヤー")]
    [SerializeField] Player player;

    [Header("セキュリティテキスト")]
    public Text SecurityText;

    [Header("Face up card Icon")]
    public GameObject faceupIcon;

    [Header("ライフカード")]
    public List<Image> LifeCards = new List<Image>();

    [Header("クリック判定")]
    public GameObject Collider;

    [Header("セキュリティガラス")]
    public SecurityBreakGlass securityBreakGlass;

    [Header("セキュリティアタックDropArea")]
    public DropArea securityAttackDropArea;

    [Header("セキュリティアタック表示オブジェクト")]
    [SerializeField] GameObject ShowSecurityAttackObject;

    [Header("セキュリティアタック表示テキスト")]
    [SerializeField] Text ShowSecurityAttackText;

    [Header("セキュリティアタック表示画像")]
    [SerializeField] Image ShowSecurityAttackImage;
    [SerializeField] Image _securityIconImage;

    UnityAction OnClickAction = null;
    private async void Start()
    {
        RemoveClickTarget();

        securityBreakGlass.Init(null);

        OffShowSecurityAttackObject();

        if (_securityIconImage != null)
        {
            if (player != null)
            {
                string key = "SecurityIcon_You";

                if (!player.isYou)
                {
                    key = "SecurityIcon_Opponent";
                }

                Sprite securityIconSprite = await StreamingAssetsUtility.GetSprite(key);

                if (securityIconSprite != null)
                {
                    _securityIconImage.sprite = securityIconSprite;
                }
            }
        }

        GManager.OnSecurityStackChanged += CheckFaceupSecurity;
    }

    public void OffShowSecurityAttackObject()
    {
        if (ShowSecurityAttackObject != null)
        {
            ShowSecurityAttackObject.SetActive(false);
        }
    }

    public void SetSecurityAttackObject()
    {
        if (ShowSecurityAttackObject != null)
        {
            ShowSecurityAttackObject.SetActive(true);

            if (player != null)
            {
                if (ShowSecurityAttackText != null)
                {
                    if (player.SecurityCards.Count >= 1)
                    {
                        ShowSecurityAttackText.text = "Security Attack";
                    }

                    else
                    {
                        ShowSecurityAttackText.text = "Direct Attack";
                    }
                }
            }
        }
    }

    public void SetSecurityOutline(bool isSelected)
    {
        if (ShowSecurityAttackImage != null)
        {
            Outline outline = ShowSecurityAttackImage.GetComponent<Outline>();

            if (outline != null)
            {
                if (isSelected)
                {
                    outline.effectColor = new Color32(0, 0, 0, 255);
                }

                else
                {
                    outline.effectColor = new Color32(0, 0, 0, 50);
                }
            }
        }
    }

    public void SetSecurity(Player player)
    {
        SecurityText.text = $"{player.SecurityCards.Count}";

        for (int i = 0; i < LifeCards.Count; i++)
        {
            if (i < player.SecurityCards.Count)
            {
                LifeCards[i].gameObject.SetActive(true);
                LifeCards[i].sprite = ContinuousController.instance.ReverseCard;
            }

            else
            {
                LifeCards[i].gameObject.SetActive(false);
            }
        }
    }
    public void RemoveClickTarget()
    {
        if (Collider != null)
        {
            Collider.SetActive(false);
            this.OnClickAction = null;
        }
    }

    public void AddClickTarget(UnityAction OnClickAction)
    {
        if (Collider != null)
        {
            Collider.SetActive(true);
            this.OnClickAction = OnClickAction;
        }
    }

    public void OnClick()
    {
        OnClickAction?.Invoke();
        RemoveClickTarget();
    }

    public void CheckFaceupSecurity(Player changedPlayer)
    {
        if (player != changedPlayer)
            return;

        faceupIcon.SetActive((changedPlayer.SecurityCards.Count(cardSource => !cardSource.IsFlipped) > 0));
    }

    public void OnDestroy()
    {
        GManager.OnSecurityStackChanged -= CheckFaceupSecurity;
    }
}

