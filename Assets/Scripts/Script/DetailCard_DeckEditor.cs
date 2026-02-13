using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq;
public class DetailCard_DeckEditor : MonoBehaviour
{
    [Header("カード画像")]
    [SerializeField] Image detailCardImage;

    [Header("カード名背景")]
    [SerializeField] List<Image> cardNameBackgrounds = new List<Image>();

    [Header("カード名テキスト")]
    [SerializeField] TextMeshProUGUI cardNameText;

    [Header("カードID & レアリティテキスト")]
    [SerializeField] TextMeshProUGUI cardID_RarityText;

    [Header("プレイコストタイトル")]
    [SerializeField] TextMeshProUGUI _playCostTitleText;

    [Header("プレイコスト")]
    [SerializeField] TextMeshProUGUI playCostText;

    [Header("DPコスト")]
    [SerializeField] TextMeshProUGUI DPText;

    [Header("進化コストコストタイトル")]
    [SerializeField] TextMeshProUGUI _digivolutionCostTitleText;

    [Header("進化コストオブジェクト")]
    [SerializeField] GameObject evoCostObject;

    [Header("進化コストタブリスト")]
    [SerializeField] List<EvoCostColorTab_DetailCard> evoCostColorTabs = new List<EvoCostColorTab_DetailCard>();

    [Header("デジモン詳細オブジェクト")]
    [SerializeField] GameObject digimonDetailObject;

    [Header("レベルテキスト")]
    [SerializeField] TextMeshProUGUI levelText;

    [Header("形態テキスト")]
    [SerializeField] TextMeshProUGUI formText;

    [Header("属性テキスト")]
    [SerializeField] TextMeshProUGUI attributeText;

    [Header("タイプテキスト")]
    [SerializeField] TextMeshProUGUI typeText;

    [Header("効果テキスト")]
    [SerializeField] TextMeshProUGUI effectDiscriptionText;

    [Header("追加効果テキスト")]
    [SerializeField] TextMeshProUGUI alternativeEffectDiscriptionText;

    [Header("追加効果タイトルテキスト")]
    [SerializeField] TextMeshProUGUI alternativeEffectTitleText;

    [Header("追加効果タイトル背景")]
    [SerializeField] List<Image> alternativeEffectTitleBackgrounds = new List<Image>();

    [SerializeField] TMP_FontAsset Font_ENG;
    [SerializeField] TMP_FontAsset Font_JPN;
    [SerializeField] Material Material_ENG;

    public async void SetUpDetailCard(CEntity_Base cEntity_Base)
    {
        this.gameObject.SetActive(true);

        //カード画像
        detailCardImage.sprite = await cEntity_Base.GetCardSprite();

        //カード名背景
        for (int i = 0; i < cardNameBackgrounds.Count; i++)
        {
            CardColor cardColor = CardColor.None;

            if (i < cEntity_Base.cardColors.Count)
            {
                cardColor = cEntity_Base.cardColors[i];
                cardNameBackgrounds[i].color = DataBase.CardColor_ColorDarkDictionary[cardColor];
                cardNameBackgrounds[i].gameObject.SetActive(true);
            }

            else
            {
                cardNameBackgrounds[i].gameObject.SetActive(false);
            }            
        }

        //カード名
        if (ContinuousController.instance.language == Language.ENG)
        {
            cardNameText.font = Font_ENG;
            cardNameText.fontSharedMaterial = Material_ENG;
            cardNameText.text = cEntity_Base.CardName_ENG;
        }

        else
        {
            cardNameText.font = Font_JPN;
            cardNameText.text = cEntity_Base.CardName_JPN;
        }
        


        if (cEntity_Base.cardColors.Count > 0 && (cEntity_Base.cardColors[0] == CardColor.Yellow || cEntity_Base.cardColors[0] == CardColor.White))
        {
            cardNameText.color = Color.black;
        }

        else
        {
            cardNameText.color = Color.white;
        }

        //カードID & レアリティ
        if (cEntity_Base.cardColors.Count >= 1)
        {
            CardColor cardColor = cEntity_Base.cardColors[0];

            if (cEntity_Base.cardColors.Count >= 2)
            {
                cardColor = cEntity_Base.cardColors[1];
            }

            if (cardColor == CardColor.Yellow || cardColor == CardColor.White)
            {
                cardID_RarityText.color = Color.black;
            }

            else
            {
                cardID_RarityText.color = Color.white;
            }
        }
        cardID_RarityText.text = $"{cEntity_Base.CardID} {cEntity_Base.rarity}";

        //プレイコスト
        if (cEntity_Base.HasCost)
        {
            playCostText.transform.parent.parent.gameObject.SetActive(true);
            playCostText.transform.parent.gameObject.SetActive(true);

            playCostText.text = $"{cEntity_Base.PlayCost}";

            if (_playCostTitleText != null)
            {
                if (cEntity_Base.cardKind != CardKind.Option)
                {
                    _playCostTitleText.text = LocalizeUtility.GetLocalizedString(
                        EngMessage: "PlayCost",
                        JpnMessage: "登場コスト"
                    );
                }

                else
                {
                    _playCostTitleText.text = LocalizeUtility.GetLocalizedString(
                        EngMessage: "UseCost",
                        JpnMessage: "使用コスト"
                    );
                }
            }
        }

        else
        {
            playCostText.transform.parent.parent.gameObject.SetActive(false);
            playCostText.transform.parent.gameObject.SetActive(false);
        }

        //DP
        if (cEntity_Base.cardKind == CardKind.Digimon)
        {
            DPText.transform.parent.gameObject.SetActive(true);
            DPText.text = $"{cEntity_Base.DP}";
        }

        else
        {
            DPText.transform.parent.gameObject.SetActive(false);
        }

        //進化コスト
        if (cEntity_Base.EvoCosts.Count >= 1)
        {
            evoCostObject.SetActive(true);

            if (_digivolutionCostTitleText != null)
            {
                _digivolutionCostTitleText.text = LocalizeUtility.GetLocalizedString(
                        EngMessage: "DigivolutionCost",
                        JpnMessage: "進化コスト"
                    );
            }

            foreach (EvoCostColorTab_DetailCard evoCostColorTab in evoCostColorTabs)
            {
                evoCostColorTab.levelText.transform.parent.gameObject.SetActive(false);

                foreach (EvoCost evoCost in cEntity_Base.EvoCosts)
                {
                    if (evoCost.CardColor == evoCostColorTab.cardColor)
                    {
                        evoCostColorTab.levelText.transform.parent.gameObject.SetActive(true);
                        evoCostColorTab.levelText.text = $"Lv{evoCost.Level}";
                        evoCostColorTab.memoryCostText.text = $"{evoCost.MemoryCost}";
                        break;
                    }
                }
            }
        }

        else
        {
            evoCostObject.SetActive(false);
        }



        //Level
        if (cEntity_Base.HasLevel)
            levelText.text = $"Lv{cEntity_Base.Level}";            

        #region Form (trait)
        string formString = "";

        for (int i = 0; i < cEntity_Base.Form_ENG.Count; i++)
        {
            if (i >= 1)
            {
                formString += " ";
            }

            if (ContinuousController.instance.language == Language.ENG)
            {
                formString += $"{cEntity_Base.Form_ENG[i]}";
            }

            else
            {
                formString += $"{cEntity_Base.Form_JPN[i]}";
            }
        }

        if (ContinuousController.instance.language == Language.ENG)
        {
            formText.font = Font_ENG;
            formText.fontSharedMaterial = Material_ENG;
        }

        else
        {
            formText.font = Font_JPN;
        }

        formText.text = $"{formString}";
        #endregion

        #region Attribute
        string attributeString = "";

        for (int i = 0; i < cEntity_Base.Attribute_ENG.Count; i++)
        {
            if (i >= 1)
            {
                attributeString += " ";
            }

            if (ContinuousController.instance.language == Language.ENG)
            {
                attributeString += $"{cEntity_Base.Attribute_ENG[i]}";
            }

            else
            {
                attributeString += $"{cEntity_Base.Attribute_JPN[i]}";
            }
        }

        if (ContinuousController.instance.language == Language.ENG)
        {
            attributeText.font = Font_ENG;
            attributeText.fontSharedMaterial = Material_ENG;
        }

        else
        {
            attributeText.font = Font_JPN;
        }

        attributeText.text = $"{attributeString}";

        if (!string.IsNullOrEmpty(attributeString))
        {
            ((RectTransform)formText.transform).sizeDelta = new Vector2(110, ((RectTransform)formText.transform).sizeDelta.y);
        }

        else
        {
            ((RectTransform)formText.transform).sizeDelta = new Vector2(184.81f, ((RectTransform)formText.transform).sizeDelta.y);
        }
        #endregion

        #region Type
        string typeString = "";

        for (int i = 0; i < cEntity_Base.Type_ENG.Count; i++)
        {
            if (i >= 1)
            {
                typeString += "/";
            }

            if (ContinuousController.instance.language == Language.ENG)
            {
                typeString += $"{cEntity_Base.Type_ENG[i]}";
            }

            else
            {
                if (cEntity_Base.Type_JPN.Count > i)
                {
                    typeString += $"{cEntity_Base.Type_JPN[i]}";
                }
            }
        }

        if (ContinuousController.instance.language == Language.ENG)
        {
            typeText.font = Font_ENG;
            typeText.fontSharedMaterial = Material_ENG;
        }

        else
        {
            typeText.font = Font_JPN;
        }

        typeText.text = $"{typeString}";
        #endregion

        levelText.gameObject.SetActive(cEntity_Base.HasLevel);
        formText.gameObject.SetActive(!string.IsNullOrEmpty(formString));
        attributeText.gameObject.SetActive(!string.IsNullOrEmpty(attributeString));
        typeText.gameObject.SetActive(!string.IsNullOrEmpty(typeString));

        digimonDetailObject.SetActive(levelText.gameObject.activeSelf || formText.gameObject.activeSelf || attributeText.gameObject.activeSelf  || typeText.gameObject.activeSelf);

        string changeColorString(string s)
        {
            List<string> keyWordEffect = new List<string>()
            {
                "Blocker",
                "Security Attack +",
                "Security Attack -",
                "Delay",
                "Draw 1",
                "Draw 2",
                "Draw 3",
                "Armor Purge",
                "Jamming",
                "DNA Digivolution",
                "Save",
                "Decoy",
                "Digi-Burst",
                "Piercing",
                "Blitz",
                "Material Save",
                "Recovery +",
                "Evade",
                "Digisorption",
                "Reboot",
                "Rush",
                "De-Digivolve",
                "Retaliation",
                "Raid",
            };

            return s;
        }

        //効果テキスト
        if (ContinuousController.instance.language == Language.ENG)
        {
            effectDiscriptionText.font = Font_ENG;
            effectDiscriptionText.fontSharedMaterial = Material_ENG;
            effectDiscriptionText.text = DataBase.ReplaceToASCII(cEntity_Base.EffectDiscription_ENG);
        }

        else
        {
            effectDiscriptionText.font = Font_JPN;
            effectDiscriptionText.text = DataBase.ReplaceToASCII(cEntity_Base.EffectDiscription_JPN);
        }

        //追加効果タイトル
        if (cEntity_Base.HasInhetitedEffect || cEntity_Base.HasSecutiryEffect)
        {
            alternativeEffectTitleText.transform.parent.gameObject.SetActive(true);

            for (int i = 0; i < alternativeEffectTitleBackgrounds.Count; i++)
            {
                CardColor cardColor = CardColor.None;

                if (i < cEntity_Base.cardColors.Count)
                {
                    cardColor = cEntity_Base.cardColors[i];
                }

                else
                {
                    cardColor = cEntity_Base.cardColors[0];
                }

                alternativeEffectTitleBackgrounds[i].color = DataBase.CardColor_ColorDarkDictionary[cardColor];

                if (cEntity_Base.cardColors.Contains(CardColor.Yellow) || cEntity_Base.cardColors.Contains(CardColor.White))
                {
                    alternativeEffectTitleText.color = Color.black;
                }

                else
                {
                    alternativeEffectTitleText.color = Color.white;
                }
            }

            if (ContinuousController.instance.language == Language.ENG)
            {
                alternativeEffectTitleText.font = Font_ENG;
                alternativeEffectTitleText.fontSharedMaterial = Material_ENG;

                alternativeEffectDiscriptionText.font = Font_ENG;
                alternativeEffectDiscriptionText.fontSharedMaterial = Material_ENG;
            }

            else
            {
                alternativeEffectTitleText.font = Font_JPN;

                alternativeEffectDiscriptionText.font = Font_JPN;
            }

            if (cEntity_Base.HasInhetitedEffect)
            {
                if (ContinuousController.instance.language == Language.ENG)
                {
                    alternativeEffectTitleText.text = $"Inherited Effect";
                    alternativeEffectDiscriptionText.text = DataBase.ReplaceToASCII(cEntity_Base.InheritedEffectDiscription_ENG);
                }

                else
                {
                    alternativeEffectTitleText.text = $"進化元効果";
                    alternativeEffectDiscriptionText.text = DataBase.ReplaceToASCII(cEntity_Base.InheritedEffectDiscription_JPN);
                }
            }

            else if (cEntity_Base.HasSecutiryEffect)
            {
                if (ContinuousController.instance.language == Language.ENG)
                {
                    alternativeEffectTitleText.text = $"Security Effect";
                    alternativeEffectDiscriptionText.text = DataBase.ReplaceToASCII(cEntity_Base.SecurityEffectDiscription_ENG);
                }

                else
                {
                    alternativeEffectTitleText.text = $"セキュリティ効果";
                    alternativeEffectDiscriptionText.text = DataBase.ReplaceToASCII(cEntity_Base.SecurityEffectDiscription_JPN);
                }
            }

            effectDiscriptionText.fontSizeMax = 25;
            effectDiscriptionText.GetComponent<RectTransform>().sizeDelta = new Vector2(effectDiscriptionText.GetComponent<RectTransform>().sizeDelta.x, 106.1f);
        }

        else
        {
            alternativeEffectTitleText.transform.parent.gameObject.SetActive(false);

            alternativeEffectDiscriptionText.text = "";

            effectDiscriptionText.fontSizeMax = 25;
            effectDiscriptionText.GetComponent<RectTransform>().sizeDelta = new Vector2(effectDiscriptionText.GetComponent<RectTransform>().sizeDelta.x, 245f);
        }
    }


    public void OffDetailCard()
    {
        this.gameObject.SetActive(false);
    }
}

[Serializable]
public class EvoCostColorTab_DetailCard
{
    [Header("色")]
    public CardColor cardColor;

    [Header("レベルテキスト")]
    public TextMeshProUGUI levelText;

    [Header("メモリーコストテキスト")]
    public TextMeshProUGUI memoryCostText;
}