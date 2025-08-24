using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

[CreateAssetMenu(menuName = "Create/CEntity_Base")]
public class CEntity_Base : ScriptableObject
{
    public int CardIndex = 0;
    public List<CardColor> cardColors = new List<CardColor>();
    public int PlayCost = 0;
    public List<EvoCost> EvoCosts = new List<EvoCost>();
    public int Level = 0;
    public string CardName_JPN = "";
    public string CardName_ENG = "";
    public List<string> Form_JPN = new List<string>();
    public List<string> Form_ENG = new List<string>();
    public List<string> Attribute_JPN = new List<string>();
    public List<string> Attribute_ENG = new List<string>();
    public List<string> Type_JPN = new List<string>();
    public List<string> Type_ENG = new List<string>();
    public string CardSpriteName = "";
    public CardKind cardKind = CardKind.Digimon;
    [TextArea] public string EffectDiscription_JPN = "";
    [TextArea] public string EffectDiscription_ENG = "";
    [TextArea] public string InheritedEffectDiscription_JPN = "";
    [TextArea] public string InheritedEffectDiscription_ENG = "";
    [TextArea] public string SecurityEffectDiscription_JPN = "";
    [TextArea] public string SecurityEffectDiscription_ENG = "";
    public string CardEffectClassName = "";
    public int DP = 0;
    public Rarity rarity;
    public int OverflowMemory = 0;
    public int LinkDP = 0;
    [TextArea] public string LinkEffect = "";
    [TextArea] public string LinkRequirement = "";

    public bool HasInhetitedEffect => !string.IsNullOrEmpty(InheritedEffectDiscription_ENG) && !InheritedEffectDiscription_ENG.Equals("-");
    public bool HasSecutiryEffect => !string.IsNullOrEmpty(SecurityEffectDiscription_ENG) && !SecurityEffectDiscription_ENG.Equals("-");
    public string CardID = "";
    public int MaxCountInDeck = 4;
    public bool HasLoadStarted { get; set; } = false;
    public Sprite CardSprite { get; set; } = null;
    public async Task LoadCardImage()
    {
        if (String.IsNullOrEmpty(CardSpriteName))
        {
            return;
        }

        if (HasLoadStarted)
        {
            return;
        }

        HasLoadStarted = true;

        Sprite sprite = await StreamingAssetsUtility.GetSprite(CardSpriteName, isCard: true);

        CardSprite = sprite;
    }

    public async Task<Sprite> GetCardSprite()
    {
        if (!HasLoadStarted)
        {
            await LoadCardImage();
        }

        return CardSprite;
    }
    public bool IsACE => OverflowMemory >= 1;
    public bool IsStandardValid => true;

    #region regulation mark
    public string RegulationMark
    {
        get
        {
            string regulationMark = "";

            switch (GetParseByHyphen(CardSpriteName)[0])
            {
                case "ST1":
                    regulationMark = "00";
                    break;

                case "ST2":
                    regulationMark = "00";
                    break;

                case "ST3":
                    regulationMark = "00";
                    break;

                case "ST4":
                    regulationMark = "00";
                    break;

                case "ST5":
                    regulationMark = "00";
                    break;

                case "ST6":
                    regulationMark = "00";
                    break;

                case "ST7":
                    regulationMark = "01";
                    break;

                case "ST8":
                    regulationMark = "01";
                    break;

                case "ST9":
                    regulationMark = "01";
                    break;

                case "ST10":
                    regulationMark = "01";
                    break;

                case "ST11":
                    regulationMark = "01";
                    break;

                case "ST12":
                    regulationMark = "02";
                    break;

                case "ST13":
                    regulationMark = "02";
                    break;

                case "ST14":
                    regulationMark = "02";
                    break;

                case "BT1":
                    regulationMark = "00";
                    break;

                case "BT2":
                    regulationMark = "00";
                    break;

                case "BT3":
                    regulationMark = "00";
                    break;

                case "BT4":
                    regulationMark = "00";
                    break;

                case "BT5":
                    regulationMark = "00";
                    break;

                case "BT6":
                    regulationMark = "01";
                    break;

                case "BT7":
                    regulationMark = "01";
                    break;

                case "BT8":
                    regulationMark = "01";
                    break;

                case "BT9":
                    regulationMark = "01";
                    break;

                case "BT10":
                    regulationMark = "02";
                    break;

                case "BT11":
                    regulationMark = "02";
                    break;

                case "BT12":
                    regulationMark = "02";
                    break;

                case "BT13":
                    regulationMark = "02";
                    break;

                case "EX1":
                    regulationMark = "01";
                    break;

                case "EX2":
                    regulationMark = "01";
                    break;

                case "EX3":
                    regulationMark = "02";
                    break;

                case "EX4":
                    regulationMark = "02";
                    break;

                case "RB1":
                    regulationMark = "02";
                    break;
            }

            return regulationMark;
        }
    }
    #endregion

    #region セットID
    public string SetID
    {
        get
        {
            if (GetParseByHyphen(CardSpriteName).Length >= 1)
            {
                return GetParseByHyphen(CardSpriteName)[0];
            }

            return "";
        }
    }
    #endregion

    #region whether it is permanent card
    public bool IsPermanent => cardKind == CardKind.Digimon || cardKind == CardKind.Tamer || cardKind == CardKind.DigiEgg;
    #endregion

    #region カードIndexをデッキコードに用いる文字列に変換(256進数)
    public string CardIndex_String
    {
        get
        {
            string CardID_String = ConvertBinaryNumber.IntToNString(CardIndex, DeckData.m);

            while (CardID_String.Length < DeckData.CardKindCellLength)
            {
                CardID_String = $"0{CardID_String}";
            }

            return CardID_String;
        }
    }
    #endregion

    #region カード画像名をアンダーバーで区切り
    public static string[] GetParseByUnderBar(string CardImageName)
    {
        string[] parseByUnderBar = new string[] { CardImageName };

        if (CardImageName.Contains('_'))
        {
            parseByUnderBar = CardImageName.Split('_');
        }

        return parseByUnderBar;
    }
    #endregion

    #region カード画像名をハイフンで区切り
    public static string[] GetParseByHyphen(string CardImageName)
    {
        string[] parseByHyphen = new string[] { CardImageName };

        if (CardImageName.Contains('-'))
        {
            parseByHyphen = CardImageName.Split('-');
        }

        return parseByHyphen;
    }
    #endregion

    #region デッキにこれと同カードIDのカードが入っている枚数
    public int SameCardIDCount(List<CEntity_Base> DeckCards)
    {
        return DeckCards.Count((cEntity_Base) => cEntity_Base.CardID == CardID);
    }
    #endregion

    #region パラレルか
    public bool isParallel
    {
        get
        {
            if (!string.IsNullOrEmpty(CardSpriteName))
            {
                string s = CardSpriteName.Replace(CardID, "");

                if (!string.IsNullOrEmpty(s))
                {
                    if (s.Contains("_P"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region レベルを持つか
    public bool HasLevel
    {
        get
        {
            if (Level == 0)
            {
                return false;
            }

            if (cardKind != CardKind.Digimon && cardKind != CardKind.DigiEgg)
            {
                return false;
            }

            return true;
        }
    }
    #endregion

    #region コストを持つか
    public bool HasCost => cardKind != CardKind.DigiEgg && PlayCost >= 0;
    #endregion

    #region Wheter the card has play cost
    public bool HasPlayCost => cardKind != CardKind.Option && PlayCost >= 0;
    #endregion

    #region Wheter the card has use cost
    public bool HasUseCost => cardKind == CardKind.Option && PlayCost >= 0;
    #endregion
}
public enum CardKind
{
    Digimon,
    Tamer,
    Option,
    DigiEgg,
}

[Serializable]
public class EvoCost : IEquatable<EvoCost>
{
    public CardColor CardColor;
    public int Level;
    public int MemoryCost;

    public bool Equals(EvoCost other)
    {
        if (other is null)
        {
            return false;
        }

        if (object.ReferenceEquals(this, other))
        {
            return true;
        }

        return CardColor.Equals(other.CardColor) && Level.Equals(other.Level) && MemoryCost.Equals(other.MemoryCost);
    }

    public override int GetHashCode() => CardColor.GetHashCode() ^ Level.GetHashCode() + MemoryCost.GetHashCode();
}

public enum CardColor
{
    Red,
    Blue,
    Yellow,
    Green,
    White,
    Black,
    Purple,
    None,
}

public enum Rarity
{
    C,
    U,
    R,
    SR,
    SEC,
    P,
    None,
}