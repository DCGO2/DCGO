using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Linq;
using System.Text;
using TMPro;
public class FilterCardList : MonoBehaviour
{
    [Header("検索インプットフィールド")]
    public InputField SearchInputField;

    [Header("カード色")]
    [SerializeField] List<Dropdown> _cardColorDropdowns = new List<Dropdown>();

    [Header("コスト")]
    [SerializeField] Dropdown costDropdown;

    [Header("レアリティ")]
    [SerializeField] Dropdown rarityDropdown;

    [Header("レベル")]
    [SerializeField] Dropdown levelDropdown;

    [Header("カード種別")]
    [SerializeField] Dropdown cardKindDropdown;

    [Header("カードセット")]
    [SerializeField] Dropdown cardSetDropdown;

    [Header("進化元効果")]
    public Toggle onlyInheritedEffectToggle;

    [Header("セキュリティ効果")]
    public Toggle onlySecurityEffectToggle;

    [Header("パラレル")]
    public Toggle showParallelToggle;

    #region カード色
    public Func<CEntity_Base, bool> OnlyContainsColor()
    {
        bool _OnlyContainsColor(CEntity_Base cEntity_Base)
        {
            bool OK_CardColor = false;

            if (_cardColorDropdowns == null || _cardColorDropdowns.Some(dropdown => dropdown == null) || _cardColorDropdowns.Count < 2)
            {
                OK_CardColor = true;
            }

            else
            {
                // All
                if (_cardColorDropdowns[0].value == 0)
                {
                    OK_CardColor = true;
                }

                else
                {
                    List<CardColor> cardColors = _cardColorDropdowns
                                        .Filter(dropdown => 0 < dropdown.value && dropdown.value <= Enum.GetValues(typeof(CardColor)).Length - 1)
                                        .Map(dropdown => (CardColor)Enum.ToObject(typeof(CardColor), dropdown.value - 1));

                    if (cardColors.Every(cardColor => cEntity_Base.cardColors.Contains(cardColor)))
                    {
                        OK_CardColor = true;
                    }
                }
            }

            return OK_CardColor;
        }

        return _OnlyContainsColor;
    }

    #endregion

    #region コスト
    public Func<CEntity_Base, bool> OnlyMatchPlayCost()
    {
        bool _OnlyMatchPlayCost(CEntity_Base cEntity_Base)
        {
            bool OK_PlayCost = false;

            if (costDropdown == null)
            {
                OK_PlayCost = true;
            }

            else
            {
                if (costDropdown.value == 0)
                {
                    OK_PlayCost = true;
                }

                else if (costDropdown.value > 0)
                {
                    int value;

                    if (int.TryParse(costDropdown.options[costDropdown.value].text, out value))
                    {
                        if (cEntity_Base.PlayCost == value)
                        {
                            OK_PlayCost = true;
                        }
                    }
                }
            }

            return OK_PlayCost;
        }

        return _OnlyMatchPlayCost;
    }
    #endregion

    #region レアリティ
    public Func<CEntity_Base, bool> OnlyMatchRarity()
    {
        bool _OnlyMatchRarity(CEntity_Base cEntity_Base)
        {
            bool OK_Rarity = false;

            if (rarityDropdown == null)
            {
                OK_Rarity = true;
            }

            else
            {
                if (rarityDropdown.value == 0)
                {
                    OK_Rarity = true;
                }

                else if (rarityDropdown.value > 0)
                {
                    if (cEntity_Base.rarity == (Rarity)Enum.ToObject(typeof(Rarity), rarityDropdown.value - 1))
                    {
                        OK_Rarity = true;
                    }
                }
            }

            return OK_Rarity;
        }

        return _OnlyMatchRarity;
    }

    #endregion

    #region レベル
    public Func<CEntity_Base, bool> OnlyMatchLevel()
    {
        bool _OnlyMatchLevel(CEntity_Base cEntity_Base)
        {
            bool OK_Level = false;

            if (levelDropdown == null)
            {
                OK_Level = true;
            }

            else
            {
                if (levelDropdown.value == 0)
                {
                    OK_Level = true;
                }

                else if (levelDropdown.value > 0)
                {
                    int value;

                    if (int.TryParse(levelDropdown.options[levelDropdown.value].text, out value))
                    {
                        if (cEntity_Base.Level == value)
                        {
                            OK_Level = true;
                        }
                    }
                }
            }

            return OK_Level;
        }

        return _OnlyMatchLevel;
    }
    #endregion

    #region カード種別
    public Func<CEntity_Base, bool> OnlyMatchCardKind()
    {
        bool _OnlyContainsCardKind(CEntity_Base cEntity_Base)
        {
            bool OK_CardKind = false;

            if (cardKindDropdown == null)
            {
                OK_CardKind = true;
            }

            else
            {
                if (cardKindDropdown.value == 0)
                {
                    OK_CardKind = true;
                }

                else if (cardKindDropdown.value > 0)
                {
                    if (ContinuousController.instance != null)
                    {
                        switch (cardKindDropdown.value)
                        {
                            case 1:
                                if (cEntity_Base.cardKind == CardKind.Digimon)
                                {
                                    OK_CardKind = true;
                                }
                                break;

                            case 2:
                                if (cEntity_Base.cardKind == CardKind.DigiEgg)
                                {
                                    OK_CardKind = true;
                                }
                                break;

                            case 3:
                                if (cEntity_Base.cardKind == CardKind.Tamer)
                                {
                                    OK_CardKind = true;
                                }
                                break;

                            case 4:
                                if (cEntity_Base.cardKind == CardKind.Option)
                                {
                                    OK_CardKind = true;
                                }
                                break;
                        }
                    }
                }
            }

            return OK_CardKind;
        }

        return _OnlyContainsCardKind;
    }
    #endregion

    #region カードセット
    public Func<CEntity_Base, bool> OnlyMatchCardSet()
    {
        bool _OnlyMatchCardSet(CEntity_Base cEntity_Base)
        {
            bool OK_CardSet = false;

            if (cardSetDropdown == null)
            {
                OK_CardSet = true;
            }

            else
            {
                if (cardSetDropdown.value == 0)
                {
                    OK_CardSet = true;
                }

                else if (cardSetDropdown.value > 0)
                {
                    string cardSetString = cardSetDropdown.options[cardSetDropdown.value].text;

                    if (cEntity_Base.SetID == cardSetString)
                    {
                        OK_CardSet = true;
                    }
                }
            }

            return OK_CardSet;
        }

        return _OnlyMatchCardSet;
    }
    #endregion

    #region 特殊なカード種別
    public Func<CEntity_Base, bool> OnlyMatchSpecialCardKind()
    {
        bool _OnlyMatchSpecialCardKind(CEntity_Base cEntity_Base)
        {
            if (onlyInheritedEffectToggle.isOn || onlySecurityEffectToggle.isOn)
            {
                if (onlyInheritedEffectToggle.isOn)
                {
                    if (cEntity_Base.HasInhetitedEffect)
                    {
                        return true;
                    }
                }

                if (onlySecurityEffectToggle.isOn)
                {
                    if (cEntity_Base.HasSecutiryEffect)
                    {
                        return true;
                    }
                }
            }

            else
            {
                return true;
            }

            return false;
        }

        return _OnlyMatchSpecialCardKind;
    }

    #endregion

    #region パラレル
    public Func<CEntity_Base, bool> OnlyMatchParallelCondition()
    {
        bool _OnlyMatchParallelCondition(CEntity_Base cEntity_Base)
        {
            if (!showParallelToggle.isOn)
            {
                if (cEntity_Base.isParallel)
                {
                    return false;
                }

                return true;
            }

            else
            {
                return true;
            }
        }

        return _OnlyMatchParallelCondition;
    }

    #endregion

    #region 検索
    public Func<CEntity_Base, bool> OnlyContainsName()
    {
        string inputText = SearchInputField.text;
        string[] SplitedTexts = inputText.Split(new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

        bool _OnlyContainsName(CEntity_Base cEntity_Base)
        {
            if (!string.IsNullOrEmpty(inputText))
            {
                foreach (string text in SplitedTexts)
                {
                    if (!matchString(text))
                    {
                        return false;
                    }
                }
            }

            return true;

            bool matchString(string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    #region card name
                    if (!string.IsNullOrEmpty(cEntity_Base.CardName_JPN))
                    {
                        if (Convert(cEntity_Base.CardName_JPN).Contains(Convert(text)))
                        {
                            return true;
                        }
                    }

                    if (!string.IsNullOrEmpty(cEntity_Base.CardName_ENG))
                    {
                        if (Convert(cEntity_Base.CardName_ENG).Contains(Convert(text)))
                        {
                            return true;
                        }
                    }

                    #endregion

                    #region card image name
                    if (cEntity_Base.CardSprite != null)
                    {
                        if (!string.IsNullOrEmpty(cEntity_Base.CardSprite.name))
                        {
                            if (Convert(cEntity_Base.CardSprite.name).Contains(Convert(text)))
                            {
                                return true;
                            }
                        }
                    }
                    #endregion

                    #region card text
                    if (!string.IsNullOrEmpty(cEntity_Base.EffectDiscription_JPN))
                    {
                        if (Convert(cEntity_Base.EffectDiscription_JPN).Contains(Convert(text)))
                        {
                            return true;
                        }
                    }

                    if (!string.IsNullOrEmpty(cEntity_Base.EffectDiscription_ENG))
                    {
                        if (Convert(cEntity_Base.EffectDiscription_ENG).Contains(Convert(text)))
                        {
                            return true;
                        }
                    }

                    if (!string.IsNullOrEmpty(cEntity_Base.InheritedEffectDiscription_JPN))
                    {
                        if (Convert(cEntity_Base.InheritedEffectDiscription_JPN).Contains(Convert(text)))
                        {
                            return true;
                        }
                    }

                    if (!string.IsNullOrEmpty(cEntity_Base.InheritedEffectDiscription_ENG))
                    {
                        if (Convert(cEntity_Base.InheritedEffectDiscription_ENG).Contains(Convert(text)))
                        {
                            return true;
                        }
                    }

                    if (!string.IsNullOrEmpty(cEntity_Base.SecurityEffectDiscription_JPN))
                    {
                        if (Convert(cEntity_Base.SecurityEffectDiscription_JPN).Contains(Convert(text)))
                        {
                            return true;
                        }
                    }

                    if (!string.IsNullOrEmpty(cEntity_Base.SecurityEffectDiscription_ENG))
                    {
                        if (Convert(cEntity_Base.SecurityEffectDiscription_ENG).Contains(Convert(text)))
                        {
                            return true;
                        }
                    }
                    #endregion

                    #region カードID
                    if (!string.IsNullOrEmpty(cEntity_Base.CardID))
                    {
                        if (Convert(cEntity_Base.CardID).Contains(Convert(text)))
                        {
                            return true;
                        }
                    }
                    #endregion

                    #region 形態
                    foreach (string feature in cEntity_Base.Form_JPN)
                    {
                        if (!string.IsNullOrEmpty(feature))
                        {
                            if (Convert(feature).Contains(Convert(text)))
                            {
                                return true;
                            }
                        }
                    }

                    foreach (string feature in cEntity_Base.Form_ENG)
                    {
                        if (!string.IsNullOrEmpty(feature))
                        {
                            if (Convert(feature).Contains(Convert(text)))
                            {
                                return true;
                            }
                        }
                    }
                    #endregion

                    #region 属性
                    foreach (string feature in cEntity_Base.Attribute_JPN)
                    {
                        if (!string.IsNullOrEmpty(feature))
                        {
                            if (Convert(feature).Contains(Convert(text)))
                            {
                                return true;
                            }
                        }
                    }

                    foreach (string feature in cEntity_Base.Attribute_ENG)
                    {
                        if (!string.IsNullOrEmpty(feature))
                        {
                            if (Convert(feature).Contains(Convert(text)))
                            {
                                return true;
                            }
                        }
                    }
                    #endregion

                    #region タイプ
                    foreach (string feature in cEntity_Base.Type_JPN)
                    {
                        if (!string.IsNullOrEmpty(feature))
                        {
                            if (Convert(feature).Contains(Convert(text)))
                            {
                                return true;
                            }
                        }
                    }

                    foreach (string feature in cEntity_Base.Type_ENG)
                    {
                        if (!string.IsNullOrEmpty(feature))
                        {
                            if (Convert(feature).Contains(Convert(text)))
                            {
                                return true;
                            }
                        }
                    }
                    #endregion
                }

                return false;
            }
        }

        return _OnlyContainsName;
    }
    #endregion

    static internal string Convert(string s)
    {
        StringBuilder sb = new StringBuilder();
        char[] target = s.ToCharArray();
        char c;
        for (int i = 0; i < target.Length; i++)
        {
            c = target[i];
            if (c >= 'ぁ' && c <= 'ゔ')
            { //-> ひらがなの範囲
                c = (char)(c + 0x0060);  //-> 変換
            }
            sb.Append(c);
        }

        string text = sb.ToString();

        while (text.Contains(" ") || text.Contains("　") || text.Contains("\n"))
        {
            text = text.Replace(" ", "");
            text = text.Replace("　", "");
            text = text.Replace("\n", "");
        }

        return text.ToLower();
    }

    #region initialization
    public void Init(UnityAction onClickSearchButtonAction)
    {
        #region カード色
        for (int i = 0; i < _cardColorDropdowns.Count; i++)
        {
            Dropdown dropdown = _cardColorDropdowns[i];

            if (dropdown != null)
            {
                dropdown.options = new List<Dropdown.OptionData>();

                dropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData(i == 0 ? "All" : "-") });

                if (DataBase.instance != null)
                {
                    foreach (CardColor cardColor in Enum.GetValues(typeof(CardColor)))
                    {
                        if ((int)cardColor < DataBase.instance.ColorIcons_circle.Count)
                        {
                            dropdown.AddOptions(new List<Dropdown.OptionData>()
                        {
                            new Dropdown.OptionData(DataBase.CardColorInitialDictionary[cardColor], DataBase.instance.ColorIcons_circle[(int)cardColor])
                        });
                        }
                    }
                }

                dropdown.value = 0;
            }
        }

        #endregion

        #region コスト
        if (costDropdown != null)
        {
            costDropdown.options = new List<Dropdown.OptionData>();

            costDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("All") });

            if (ContinuousController.instance != null)
            {
                List<int> playCosts = new List<int>();

                foreach (CEntity_Base cEntity_Base in ContinuousController.instance.CardList)
                {
                    if (!playCosts.Contains(cEntity_Base.PlayCost))
                    {
                        playCosts.Add(cEntity_Base.PlayCost);
                    }
                }

                playCosts = playCosts.OrderBy((value) => value).ToList();

                foreach (int playCost in playCosts)
                {
                    costDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData(playCost.ToString()) });
                }
            }

            costDropdown.value = 0;
        }
        #endregion

        #region レアリティ
        if (rarityDropdown != null)
        {
            rarityDropdown.options = new List<Dropdown.OptionData>();

            rarityDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("All") });

            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
            {
                if (rarity == Rarity.None)
                {
                    continue;
                }

                rarityDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData(rarity.ToString()) });
            }

            rarityDropdown.value = 0;
        }
        #endregion

        #region レベル
        if (levelDropdown != null)
        {
            levelDropdown.options = new List<Dropdown.OptionData>();

            levelDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("All") });

            if (ContinuousController.instance != null)
            {
                List<int> levels = new List<int>();

                foreach (CEntity_Base cEntity_Base in ContinuousController.instance.CardList)
                {
                    if (!levels.Contains(cEntity_Base.Level))
                    {
                        levels.Add(cEntity_Base.Level);
                    }
                }

                levels = levels.OrderBy((value) => value).ToList();

                foreach (int level in levels)
                {
                    levelDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData(level.ToString()) });
                }
            }

            levelDropdown.value = 0;
        }
        #endregion

        #region カード種別
        if (cardKindDropdown != null)
        {
            cardKindDropdown.options = new List<Dropdown.OptionData>();

            cardKindDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("All") });

            cardKindDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("Digimon") });

            cardKindDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("Digiegg") });

            cardKindDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("Tamer") });

            cardKindDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("Option") });

            cardKindDropdown.value = 0;
        }
        #endregion

        #region カードセット
        if (cardSetDropdown != null)
        {
            cardSetDropdown.options = new List<Dropdown.OptionData>();

            cardSetDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("All") });

            if (ContinuousController.instance != null)
            {
                List<string> setIDs = new List<string>();

                foreach (CEntity_Base cEntity_Base in ContinuousController.instance.CardList)
                {
                    if (!setIDs.Contains(cEntity_Base.SetID))
                    {
                        setIDs.Add(cEntity_Base.SetID);
                    }
                }

                setIDs = setIDs.OrderBy((value) => DataBase.SetIDs.ToList().IndexOf(value)).ToList();

                foreach (string setID in setIDs)
                {
                    cardSetDropdown.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData(setID) });
                }
            }

            cardSetDropdown.value = 0;
        }
        #endregion

        #region 進化元効果
        onlyInheritedEffectToggle.isOn = false;
        #endregion

        #region セキュリティ効果
        onlySecurityEffectToggle.isOn = false;
        #endregion

        #region パラレル
        showParallelToggle.isOn = true;
        #endregion

        #region 検索
        SearchInputField.text = "";
        #endregion

        this.onClickSearchButtonAction = onClickSearchButtonAction;
    }
    #endregion

    UnityAction onClickSearchButtonAction = null;

    public void OnClickSearchButton()
    {
        onClickSearchButtonAction?.Invoke();
    }

    public void OnClickResetButton()
    {
        foreach (Dropdown dropdown in _cardColorDropdowns)
        {
            dropdown.value = 0;
        }

        costDropdown.value = 0;
        rarityDropdown.value = 0;
        levelDropdown.value = 0;
        cardKindDropdown.value = 0;
        cardSetDropdown.value = 0;
        onlyInheritedEffectToggle.isOn = false;
        onlySecurityEffectToggle.isOn = false;
        showParallelToggle.isOn = true;
        SearchInputField.text = "";
    }

    public void OnEndEditSearchInputfield(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            OnClickSearchButton();
            Opening.instance.PlayDecisionSE();
        }
    }
}