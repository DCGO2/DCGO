using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;

public class DataBase : MonoBehaviour
{
    public static DataBase instance = null;

    private void Awake()
    {
        instance = this;
    }

    public static Dictionary<CardKind, string> CardKindJPNameDictionary = new Dictionary<CardKind, string>()
    {
        {CardKind.Digimon,"デジモン" },
        {CardKind.Tamer,"テイマー" },
        {CardKind.Option,"オプション" },
        {CardKind.DigiEgg,"デジタマ" },
    };

    public static Dictionary<CardKind, string> CardKindENNameDictionary = new Dictionary<CardKind, string>()
    {
        {CardKind.Digimon,"Digimon" },
        {CardKind.Tamer,"Tamer" },
        {CardKind.Option,"Option" },
        {CardKind.DigiEgg,"DigiEgg" },
    };

    public static Dictionary<CardColor, string> CardColorNameDictionary = new Dictionary<CardColor, string>()
    {
        { CardColor.Green,"green"},
        { CardColor.Red,"red"},
        { CardColor.Blue,"blue"},
        { CardColor.Yellow,"yellow"},
        { CardColor.Purple,"purple"},
        { CardColor.Black,"black"},
        { CardColor.White,"white"},
        { CardColor.None,"-"},
    };

    public static Dictionary<CardColor, Color> CardColor_ColorLightDictionary = new Dictionary<CardColor, Color>()
    {
        { CardColor.Green,new Color32(55,255,49,255)},
        { CardColor.Red,new Color32(253,63,49,255)},
        { CardColor.Blue, new Color32(49,118,253,255) },
        { CardColor.Yellow, new Color32(255,231,64,255)},
        { CardColor.Purple,new Color32(234,64,176,255)},
        { CardColor.Black,new Color32(61,61,61,255)},
        { CardColor.White,new Color32(224,224,224,255)},
        { CardColor.None,new Color32(222,222,222,255)},
    };

    public static Dictionary<CardColor, Color> CardColor_ColorDarkDictionary = new Dictionary<CardColor, Color>()
    {
        { CardColor.Red,new Color32(171,12,0,255)},
        { CardColor.Blue, new Color32(0,103,171,255) },
        { CardColor.Green,new Color32(1,171,0,255)},
        { CardColor.Yellow, new Color32(255,242,64,255)},
        { CardColor.Purple,new Color32(155,0,171,255)},
        { CardColor.Black,new Color32(61,61,61,255)},
        { CardColor.White,new Color32(224,224,224,255)},
        { CardColor.None,new Color32(222,222,222,255)},
    };

    public static Dictionary<CardColor, String> CardColorInitialDictionary = new Dictionary<CardColor, String>()
    {
        { CardColor.Red,"R"},
        { CardColor.Blue, "U" },
        { CardColor.Green,"G"},
        { CardColor.Yellow, "Y"},
        { CardColor.Purple,"P"},
        { CardColor.Black,"B"},
        { CardColor.White,"W"},
        { CardColor.None,"N"},
    };

    public static List<string> CardListIDs(string SetID, bool isEnglish)
    {
        List<string> cardListID = new List<string>();

        if (isEnglish)
        {
            switch (SetID)
            {
                case "ST1":
                    cardListID.Add("522101");
                    break;

                case "ST2":
                    cardListID.Add("522102");
                    break;

                case "ST3":
                    cardListID.Add("522103");
                    break;

                case "ST4":
                    cardListID.Add("522104");
                    break;

                case "ST5":
                    cardListID.Add("522105");
                    break;

                case "ST6":
                    cardListID.Add("522106");
                    break;

                case "ST7":
                    cardListID.Add("522107");
                    break;

                case "ST8":
                    cardListID.Add("522108");
                    break;

                case "ST9":
                    cardListID.Add("522109");

                    break;

                case "ST10":
                    cardListID.Add("522110");
                    break;

                case "ST12":
                    cardListID.Add("522112");
                    break;

                case "ST13":
                    cardListID.Add("522113");
                    break;

                case "ST14":
                    cardListID.Add("522114");
                    break;

                case "ST15":
                    cardListID.Add("522115");
                    break;

                case "ST16":
                    cardListID.Add("522116");
                    break;

                case "ST17":
                    cardListID.Add("522117");
                    break;

                case "BT1":
                    cardListID.Add("522001");
                    cardListID.Add("522002");
                    break;

                case "BT2":
                    cardListID.Add("522001");
                    cardListID.Add("522002");
                    break;

                case "BT3":
                    cardListID.Add("522001");
                    cardListID.Add("522002");
                    break;

                case "BT4":
                    cardListID.Add("522003");
                    break;

                case "BT5":
                    cardListID.Add("522004");
                    break;

                case "BT6":
                    cardListID.Add("522005");
                    break;

                case "EX1":
                    cardListID.Add("522006");
                    break;

                case "BT7":
                    cardListID.Add("522007");
                    break;

                case "BT8":
                    cardListID.Add("522008");
                    break;

                case "EX2":
                    cardListID.Add("522009");
                    break;

                case "BT9":
                    cardListID.Add("522010");
                    break;

                case "BT10":
                    cardListID.Add("522011");
                    break;

                case "EX3":
                    cardListID.Add("522012");
                    break;

                case "BT11":
                    cardListID.Add("522013");
                    break;

                case "BT12":
                    cardListID.Add("522014");
                    break;

                case "EX4":
                    cardListID.Add("522015");
                    break;

                case "BT13":
                    cardListID.Add("522016");
                    break;

                case "RB1":
                    cardListID.Add("522017");
                    break;

                case "BT14":
                    cardListID.Add("522018");
                    break;

                case "EX5":
                    cardListID.Add("522019");
                    break;

                case "BT15":
                    cardListID.Add("522020");
                    break;

                case "LM":
                    cardListID.Add("522020");
                    break;

                case "BT16":
                    cardListID.Add("522021");
                    break;

                case "P":
                    cardListID.Add("522007");
                    cardListID.Add("522901");
                    break;
            }
        }
        else
        {
            switch (SetID)
            {
                case "ST1":
                    cardListID.Add("503101");
                    break;

                case "ST2":
                    cardListID.Add("503102");
                    break;

                case "ST3":
                    cardListID.Add("503103");
                    break;

                case "ST4":
                    cardListID.Add("503104");
                    break;

                case "ST5":
                    cardListID.Add("503105");
                    break;

                case "ST6":
                    cardListID.Add("503106");
                    break;

                case "ST7":
                    cardListID.Add("503107");
                    break;

                case "ST8":
                    cardListID.Add("503108");
                    break;

                case "ST9":
                    cardListID.Add("503109");
                    break;

                case "ST10":
                    cardListID.Add("503110");
                    break;

                case "ST11":
                    cardListID.Add("503111");
                    break;

                case "ST12":
                    cardListID.Add("503112");
                    break;

                case "ST13":
                    cardListID.Add("503113");
                    break;

                case "ST14":
                    cardListID.Add("503114");
                    break;

                case "ST15":
                    cardListID.Add("503115");
                    break;

                case "ST16":
                    cardListID.Add("503116");
                    break;

                case "ST17":
                    cardListID.Add("503117");
                    break;

                case "BT1":
                    cardListID.Add("503001");
                    break;

                case "BT2":
                    cardListID.Add("503002");
                    break;

                case "BT3":
                    cardListID.Add("503003");
                    break;

                case "BT4":
                    cardListID.Add("503004");
                    break;

                case "BT5":
                    cardListID.Add("503005");
                    break;

                case "BT6":
                    cardListID.Add("503006");
                    break;

                case "EX1":
                    cardListID.Add("503007");
                    break;

                case "BT7":
                    cardListID.Add("503008");
                    break;

                case "BT8":
                    cardListID.Add("503009");
                    break;

                case "EX2":
                    cardListID.Add("503010");
                    break;

                case "BT9":
                    cardListID.Add("503011");
                    break;

                case "BT10":
                    cardListID.Add("503012");
                    break;

                case "EX3":
                    cardListID.Add("503013");
                    break;

                case "BT11":
                    cardListID.Add("503014");
                    break;

                case "BT12":
                    cardListID.Add("503015");
                    break;

                case "EX4":
                    cardListID.Add("503016");
                    break;

                case "RB1":
                    cardListID.Add("503017");
                    break;

                case "BT13":
                    cardListID.Add("503018");
                    break;

                case "BT14":
                    cardListID.Add("503019");
                    break;

                case "EX5":
                    cardListID.Add("503020");
                    break;

                case "BT15":
                    cardListID.Add("503021");
                    break;

                case "LM":
                    cardListID.Add("503201");
                    break;

                case "BT16":
                    cardListID.Add("503022");
                    break;

                case "P":
                    cardListID.Add("503008");
                    cardListID.Add("503901");
                    break;
            }
        }

        return cardListID;
    }

    public List<Sprite> ColorIcons_circle = new List<Sprite>();
    public List<Sprite> ColorIcons_bar = new List<Sprite>();

    public static Color SelectColor_Orange => new Color32(255, 98, 31, 255);
    public static Color SelectColor_Blue => new Color32(30, 246, 255, 255);
    public static Color SelectColor_Green => new Color32(57, 255, 30, 255);
    public static Color CommandColor_Attack => new Color32(255, 54, 54, 255);
    public static Color CommandColor_Move => new Color32(54, 111, 255, 255);
    public static Color CommandColor_Skill => new Color32(86, 255, 102, 255);

    public static bool IsXAntibodyString(string text) =>
    !string.IsNullOrEmpty(text) && text.Replace(" ", "").Replace("-", "").ToLower() == "xantibody";

    public static bool IsContainingXAntibodyString(string text) =>
    !string.IsNullOrEmpty(text) && text.Replace(" ", "").Replace("-", "").ToLower().Contains("xantibody");

    public static string BlockerEffectDiscription()
    {
        return "<Blocker> (When an opponent's Digimon attacks, you may suspend this Digimon to force the opponent to attack it instead.)";
    }

    public static string RebootEffectDiscription()
    {
        return "<Reboot> (Unsuspend this Digimon during your opponent's unsuspend phase.)";
    }

    public static string PierceEffectDiscription()
    {
        return "<Piercing> (When this Digimon attacks and deletes an opponent's Digimon and survives the battle, it performs any security checks it normally would.)";
    }

    public static string RetaliationEffectDiscription()
    {
        return "<Retaliation> (When this Digimon is deleted after losing a battle, delete the Digimon it was battling.)";
    }

    public static string BilitzEffectDiscription()
    {
        return "<Blitz> (This Digimon can attack when your opponent has 1 or more memory.)";
    }

    public static string ArmorPurgeEffectDiscription()
    {
        return "<Armor Purge> (When this Digimon would be deleted, you may trash the top card of this Digimon to prevent that deletion.)";
    }

    public static string SaveEffectDiscription()
    {
        return "[On Deletion] <Save> (You may place this card under one of your Tamers.)";
    }

    public static string EvadeEffectDiscription()
    {
        return "<Evade> (When this Digimon would be deleted, you may suspend it to prevent that deletion.)";
    }

    public static string RaidEffectDiscription()
    {
        return "<Raid> (When this Digimon attacks, you may switch the target of attack to 1 of your opponent's unsuspended Digimon with the highest DP.)";
    }

    public static string BarrierEffectDiscription()
    {
        return "<Barrier> (When this Digimon would be deleted in battle, by trashing the top card of your security stack, prevent that deletion.)";
    }

    public static string BlastDigivolveEffectDiscription()
    {
        return "[Hand] [Counter] <Blast Digivolve> (Your Digimon may digivolve into this card without paying the cost.)";
    }

    public static string BlastDNADigivolveEffectDiscription()
    {
        return "[Hand] [Counter] <Blast DNA Digivolve> (One of your specified Digimon and 1 of the specified card in the hand may DNA Digivolve into this card.)";
    }

    public static string FortitudeEffectDiscription()
    {
        return "<Fortitude> (When this Digimon with digivolution cards is deleted, play this card without paying the cost.)";
    }

    public static string AllianceEffectDiscription()
    {
        return "<Alliance> (When this Digimon attacks, by suspending 1 of your other Digimon, this Digimon adds the suspended Digimon's DP and gains <Security Attack +1> for the attack.)";
    }

    public static string PartitionEffectDiscription()
    {
        return "<Partition> (When this Digimon with 1 of each specified card in its digivolution cards would leave the battle area other than by one of your effects or in battle, you may play 1 of each card without paying their costs.)";
    }

    public static string CollisionEffectDiscription()
    {
        return "<Collision> (During this Digimon's attack, all of your opponent's Digimon gain <Blocker>, and your opponent blocks if possible.)";
    }

    public static string VortexEffectDiscription()
    {
        return "<Vortex> (At the end of your turn, this Digimon may attack an opponent's Digimon. With this effect, it can attack the turn it was played.)";
    }

    public static string OverclockEffectDiscription(string trait)
    {
        return $"<Overclock [{trait}]> (At the end of your turn, by deleting 1 of your Tokens or other [{trait}] trait Digimon, this Digimon attacks a player without suspending.)";
    }


    public static string TrainingEffectDiscription()
    {
        return "<Training> (In the main phase, by suspending this Digimon, place your deck's top card face down as this Digimon's bottom digivolution card. This effect can also activate in the breeding area).";
    }

    public static string DecodeEffectDiscription(string[] decodeStrings)
    {
        return $"<Decode {decodeStrings[0]}> (When this Digimon would leave the battle area other than in battle, you may play 1 {decodeStrings[1]} Digimon card from its digivolution cards without paying the cost.)";
    }

    public static string ExecuteEffectDiscription()
    {
        return "<Execute> (At the end of your turn, this Digimon may attack. At the end of that attack, delete this Digimon. Your opponent's unsuspended Digimon can also be attacked with this effect.)";
    }

    public static string ProgressEffectDiscription()
    {
        return "<Progress> (While attacking, your opponent's effects don't affect this Digimon.)";
    }

    public static string LinkEffectDiscription()
    {
        return "[Link] (Plug this card from the hand or battle area sideways into the specified Digimon in the battle area.)";
    }

    public static string ReplaceToASCII(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        return text
        .Replace("＜", "<")
        .Replace("＞", ">")
        .Replace("、", ",")
        .Replace("，", ",")
        .Replace("“", "\"")
        .Replace("・", "")
        .Replace("　", "")
        .Replace("！", "!");
    }

    public static string DigiburstRegex = "デジバースト[0-9]";

    public static string FirstPlayerKey = "FirstPlayerId";

    public static string FirstPlayerIndexIdKey = "FirstPlayerIndexId";

    #region use for sort

    public static CardKind[] cardKinds = new[] { CardKind.Digimon, CardKind.Tamer, CardKind.Option };
    public static CardColor[] cardColors = new[] { CardColor.Red, CardColor.Blue, CardColor.Yellow, CardColor.Green, CardColor.Black, CardColor.Purple, CardColor.White, CardColor.None };

    public static string[] SetIDs = new string[]
    {
        "BT28",
        "BT27",
        "BT26",
        "BT25",
        "BT24",
        "BT23",
        "BT22",
        "BT21",
        "BT20",
        "BT19",
        "BT18",
        "BT17",
        "BT16",
        "BT15",
        "BT14",
        "BT13",
        "BT12",
        "BT11",
        "BT10",
        "BT9",
        "BT8",
        "BT7",
        "BT6",
        "BT5",
        "BT4",
        "BT3",
        "BT2",
        "BT1",

        "EX28",
        "EX27",
        "EX26",
        "EX25",
        "EX24",
        "EX23",
        "EX22",
        "EX21",
        "EX20",
        "EX19",
        "EX18",
        "EX17",
        "EX16",
        "EX15",
        "EX14",
        "EX13",
        "EX12",
        "EX11",
        "EX10",
        "EX9",
        "EX8",
        "EX7",
        "EX6",
        "EX5",
        "EX4",
        "EX3",
        "EX2",
        "EX1",

        "RB11",
        "RB10",
        "RB9",
        "RB8",
        "RB7",
        "RB6",
        "RB5",
        "RB4",
        "RB3",
        "RB2",
        "RB1",

        "LM11",
        "LM10",
        "LM9",
        "LM8",
        "LM7",
        "LM6",
        "LM5",
        "LM4",
        "LM3",
        "LM2",
        "LM1",

        "ST28",
        "ST27",
        "ST26",
        "ST25",
        "ST24",
        "ST23",
        "ST22",
        "ST21",
        "ST20",
        "ST19",
        "ST18",
        "ST17",
        "ST16",
        "ST15",
        "ST14",
        "ST13",
        "ST12",
        "ST11",
        "ST10",
        "ST9",
        "ST8",
        "ST7",
        "ST6",
        "ST5",
        "ST4",
        "ST3",
        "ST2",
        "ST1",

        "P",
    };

    #endregion

    #region ENG ban list

    public static CardRestriction ENGBanList = new CardRestriction(new List<CardLimitCount>()
                {
                    new CardLimitCount("BT11-064", 1),
                    new CardLimitCount("BT10-009", 1),
                    new CardLimitCount("BT9-099", 1),
                    new CardLimitCount("BT7-107", 1),
                    new CardLimitCount("BT7-072", 1),
                    new CardLimitCount("BT7-064", 1),
                    new CardLimitCount("BT7-038", 1),
                    new CardLimitCount("BT6-100", 1),
                    new CardLimitCount("BT5-109", 0),
                    new CardLimitCount("BT3-103", 1),
                    new CardLimitCount("BT3-054", 1),
                    new CardLimitCount("BT2-047", 1),
                    new CardLimitCount("EX2-039", 1),
                    new CardLimitCount("EX1-068", 1),
                    new CardLimitCount("P-008", 1),
                    new CardLimitCount("P-025", 1),
                    new CardLimitCount("BT2-069", 1),
                    new CardLimitCount("BT13-012", 1),
                    new CardLimitCount("EX4-019", 1),
                    new CardLimitCount("BT7-069", 1),
                    new CardLimitCount("BT14-002", 1),
                    new CardLimitCount("BT15-102", 1),
                    new CardLimitCount("EX5-015", 1),
                    new CardLimitCount("EX5-018", 1),
                    new CardLimitCount("EX5-062", 1),
                    new CardLimitCount("P-123", 1),
                    new CardLimitCount("P-130", 1),
                    new CardLimitCount("ST2-13", 1),
                    new CardLimitCount("BT9-098", 1),
                    new CardLimitCount("BT15-057", 1),
                    new CardLimitCount("BT14-084", 1),
                    new CardLimitCount("BT2-090", 0),
                    new CardLimitCount("BT4-104", 1),
                    new CardLimitCount("BT4-111", 1),
                    new CardLimitCount("BT11-033", 1),
                    new CardLimitCount("BT17-069", 1),
                    new CardLimitCount("EX4-030", 1),
                    new CardLimitCount("P-029", 1),
                    new CardLimitCount("P-030", 1),
                    new CardLimitCount("ST9-09", 1),
                    new CardLimitCount("EX5-065", 0),
                    new CardLimitCount("BT1-090", 1),
                    new CardLimitCount("BT16-011", 1),
                    new CardLimitCount("EX3-057", 1),
                    new CardLimitCount("BT6-104", 1),
                    new CardLimitCount("BT13-110", 1),
                    new CardLimitCount("EX4-006", 1),
                    new CardLimitCount("BT19-040", 1),
                    new CardLimitCount("EX2-070", 1),
                    new CardLimitCount("EX1-021", 1),
                },
                new List<BannedPair>()
                {
                    new BannedPair("BT20-037", new List<string>(){
                        "BT17-035",
                        "EX8-037",
                    }),
                    new BannedPair("EX2-007", new List<string>(){
                        "EX7-064",
                    })
                });

    #endregion

    #region JPN ban list

    public static CardRestriction JPNBanList = new CardRestriction(new List<CardLimitCount>()
                {
                    new CardLimitCount("BT11-064", 1),
                    new CardLimitCount("BT10-009", 1),
                    new CardLimitCount("BT9-099", 1),
                    new CardLimitCount("BT7-107", 1),
                    new CardLimitCount("BT7-072", 1),
                    new CardLimitCount("BT7-064", 1),
                    new CardLimitCount("BT7-038", 1),
                    new CardLimitCount("BT6-100", 1),
                    new CardLimitCount("BT5-109", 0),
                    new CardLimitCount("BT3-103", 1),
                    new CardLimitCount("BT3-054", 1),
                    new CardLimitCount("BT2-047", 1),
                    new CardLimitCount("EX2-039", 1),
                    new CardLimitCount("EX1-068", 1),
                    new CardLimitCount("P-008", 1),
                    new CardLimitCount("P-025", 1),
                    new CardLimitCount("BT2-069", 1),
                    new CardLimitCount("BT13-012", 1),
                    new CardLimitCount("EX4-019", 1),
                    new CardLimitCount("BT7-069", 1),
                    new CardLimitCount("BT14-002", 1),
                    new CardLimitCount("BT15-102", 1),
                    new CardLimitCount("EX5-015", 1),
                    new CardLimitCount("EX5-018", 1),
                    new CardLimitCount("EX5-062", 1),
                    new CardLimitCount("P-123", 1),
                    new CardLimitCount("P-130", 1),
                    new CardLimitCount("ST2-13", 1),
                    new CardLimitCount("BT9-098", 1),
                    new CardLimitCount("BT15-057", 1),
                    new CardLimitCount("BT14-084", 1),
                },
                new List<BannedPair>()
                {
                    new BannedPair("EX5-065", new List<string>(){
                        "P-097",
                        "BT13-102",
                        "ST16-14",
                    })
                });

    #endregion
}

[System.Serializable]
public class ColorSpriteDic : TableBase<CardColor, Sprite, SamplePair>
{
}

[System.Serializable]
public class TableBase<TKey, TValue, Type> where Type : KeyAndValue<TKey, TValue>
{
    [SerializeField]
    private List<Type> list;

    private Dictionary<TKey, TValue> table;

    public Dictionary<TKey, TValue> GetTable()
    {
        if (table == null)
        {
            table = ConvertListToDictionary(list);
        }
        return table;
    }

    /// <summary>
    /// Editor Only
    /// </summary>
    public List<Type> GetList()
    {
        return list;
    }

    static Dictionary<TKey, TValue> ConvertListToDictionary(List<Type> list)
    {
        Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();
        foreach (KeyAndValue<TKey, TValue> pair in list)
        {
            dic.Add(pair.Key, pair.Value);
        }
        return dic;
    }
}

/// <summary>
/// Serializable KeyValuePair
/// </summary>
[System.Serializable]
public class KeyAndValue<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public KeyAndValue(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }

    public KeyAndValue(KeyValuePair<TKey, TValue> pair)
    {
        Key = pair.Key;
        Value = pair.Value;
    }
}

[System.Serializable]
public class SamplePair : KeyAndValue<CardColor, Sprite>
{
    public SamplePair(CardColor key, Sprite value) : base(key, value)
    {
    }
}

public class DictionaryUtility
{
    public static CardColor GetCardColor(string s, Dictionary<CardColor, string> PokemonTypeStringDictionary)
    {
        CardColor pokemonType = PokemonTypeStringDictionary.First(x => x.Value == s).Key;

        return pokemonType;
    }

    public static CardKind GetCardKind(string CardKindName, Dictionary<CardKind, string> CardKindNameDictionary)
    {
        CardKind cardKind = CardKindNameDictionary.First(x => x.Value == CardKindName).Key;

        return cardKind;
    }

    public static Rarity GetEvoStage(string EvoStageName, Dictionary<Rarity, string> EvoStageNameDictionary)
    {
        Rarity evoStage = EvoStageNameDictionary.First(x => x.Value == EvoStageName).Key;

        return evoStage;
    }
}

public class ParmanentParameterComparer : ParameterComparer
{
    public override bool Equals(object[] i_lhs, object[] i_rhs)
    {
        if (i_lhs.Length == i_rhs.Length)
        {
            bool same = true;

            List<Permanent> i_lhs_list = new List<Permanent>();
            List<Permanent> i_rhs_list = new List<Permanent>();

            for (int i = 0; i < i_lhs.Length; i++)
            {
                i_lhs_list.Add(((Permanent[])i_lhs)[i]);
                i_rhs_list.Add(((Permanent[])i_rhs)[i]);
            }

            i_lhs_list = i_lhs_list.OrderBy((value) => value.PermanentFrame.FrameID).ToList();
            i_rhs_list = i_rhs_list.OrderBy((value) => value.PermanentFrame.FrameID).ToList();

            for (int i = 0; i < i_lhs.Length; i++)
            {
                if (i_lhs[i] != i_rhs[i])
                {
                    same = false;
                    break;
                }
            }

            if (!same)
            {
                return false;
            }

            return true;
        }

        return false;
    }
}

public class ParameterComparer : IEqualityComparer<object[]>
{
    public virtual bool Equals(object[] i_lhs, object[] i_rhs)
    {
        return false;
    }

    public virtual int GetHashCode(object[] i_obj)
    {
        return 0;
    }

    #region select k elements from n different elements

    public static IEnumerable<T[]> Enumerate<T>(IEnumerable<T> items, int k)
    {
        if (items.Count() < k)
        {
            k = items.Count();
        }

        if (k == 1)
        {
            foreach (var item in items)
            {
                yield return new T[] { item };
            }
            yield break;
        }
        foreach (var item in items)
        {
            var leftside = new T[] { item };
            var unused = items.Except(leftside);
            foreach (var rightside in Enumerate(unused, k - 1))
            {
                yield return leftside.Concat(rightside).ToArray();
            }
        }
    }

    #endregion
}