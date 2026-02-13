using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ContinuousController : MonoBehaviour
{
    [Header("game language")]
    // public Language language;

    [Header("Game version")]
    public float GameVer;

    [Header("ignore updates")]
    public bool IgnoreUpdate;

    [Header("card list")]
    public CEntity_Base[] CardList = new CEntity_Base[] { };

    [Header("Card list sorted by card ID")]
    public CEntity_Base[] SortedCardList = new CEntity_Base[] { };

    [Header("Card back image")]
    public Sprite ReverseCard;
    public Sprite ReverseCard_Digitama;

    [Header("SE prefab")]
    public SoundObject soundObject;

    [Header("deck code encryption")]
    public ShuffleDeckCode ShuffleDeckCode;
    DeckData _battleDeckData = null;

    public DeckData BattleDeckData
    {
        get
        {
            return _battleDeckData;
        }

        set
        {
            _battleDeckData = value;

            if (value != null)
            {
                LastBattleDeckData = value;
            }
        }
    }
    public DeckData LastBattleDeckData { get; private set; } = null;

    public bool NeedUpdate { get; set; }

    public bool isRandomMatch { get; set; }
    [HideInInspector] public List<SkillInfo> nullSkillInfos = null;
    public String GameVerString => Application.version;//GameVer.ToString(CultureInfo.InvariantCulture);
    #region Key for property to save deck data for battle
    public static string DeckDataPropertyKey => "BattleDeckData";
    #endregion

    #region Key for the property that stores the player name data
    public static string PlayerNameKey => "PlayerNameKey";
    #endregion

    #region Key for the property that stores the win count data
    public static string WinCountKey => "WinCountKey";
    #endregion

    [Header("Player name character limit")]
    public int PlayerNameMaxLength;

    #region Call up a scene for data storage
    public static IEnumerator LoadCoroutine()
    {
        if (instance == null)
        {
            SceneManager.LoadSceneAsync("ContinuousControllerScene", LoadSceneMode.Additive);

            while (instance == null)
            {
                yield return null;
            }

            instance.Init();
        }
    }
    #endregion

    #region List of Deck Recipes
    public List<DeckData> DeckDatas { get; set; } = new List<DeckData>();
    #endregion

    #region Deck Recipe Key
    public string DeckDatasPlayerPrefsKey { get { return "DeckDatas3"; } }
    #endregion

    public CEntity_Base DiaboromonToken { get; private set; }
    public CEntity_Base AmonToken { get; private set; }
    public CEntity_Base UmonToken { get; private set; }
    public CEntity_Base FujitsumonToken { get; private set; }
    public CEntity_Base GyuukimonToken { get; private set; }
    public CEntity_Base KoHagurumonToken { get; private set; }
    public CEntity_Base FamiliarToken { get; private set; }
    public CEntity_Base SelfDeleteFamiliarToken { get; private set; }
    public CEntity_Base VoleeZerdruckenToken { get; private set; }
    public CEntity_Base UkaNoMitamaToken { get; private set; }
    public CEntity_Base WarGrowlmonToken { get; private set; }
    public CEntity_Base TaomonToken { get; private set; }
    public CEntity_Base RapidmonToken { get; private set; }
    public CEntity_Base PipeFoxToken { get; private set; }
    public CEntity_Base AthoRenePorToken { get; private set; }
    public CEntity_Base HinukamuyToken { get; private set; }
    public CEntity_Base PetrificationToken { get; private set; }
    public CardRestriction BanList { get; private set; } = new CardRestriction(new List<CardLimitCount>(), new List<BannedPair>());

    void LoadBanList()
    {
        BanList = DataBase.ENGBanList;
    }

    async Task CreateTokenData()
    {
        DiaboromonToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.White },
            PlayCost = 14,
            Level = 6,
            CardName_JPN = "ディアボロモン",
            CardName_ENG = "Diaboromon",
            Form_JPN = new List<string>() { "究極体" },
            Form_ENG = new List<string>() { "Mega" },
            Attribute_JPN = new List<string>() { "不明" },
            Attribute_ENG = new List<string>() { "Unknown" },
            Type_JPN = new List<string>() { "種族不明" },
            Type_ENG = new List<string>() { "Unidentified" },
            CardSpriteName = "BT2-082-token",
            cardKind = CardKind.Digimon,
            DP = 3000,
        };

        await DiaboromonToken.GetCardSprite();

        AmonToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Red },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "紅炎のアモン",
            CardName_ENG = "Amon of Crimson Flame",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "BT14-018-token-red",
            cardKind = CardKind.Digimon,
            DP = 6000,
            CardEffectClassName = "BT4_038"
        };

        await AmonToken.GetCardSprite();

        UmonToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Yellow },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "蒼雷のウモン",
            CardName_ENG = "Umon of Blue Thunder",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "BT14-018-token-yellow",
            cardKind = CardKind.Digimon,
            DP = 6000,
            CardEffectClassName = "BT1_031"
        };

        await UmonToken.GetCardSprite();

        FujitsumonToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Purple },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "フジツモン",
            CardName_ENG = "Fujitsumon",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "EX5-058-token",
            cardKind = CardKind.Digimon,
            DP = 3000,
            CardEffectClassName = "EX5_058_token"
        };

        await FujitsumonToken.GetCardSprite();

        GyuukimonToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Purple },
            PlayCost = 7,
            Level = 5,
            CardName_JPN = "ギュウキモン",
            CardName_ENG = "Gyuukimon",
            Form_JPN = new List<string>() { "究極の" },
            Form_ENG = new List<string>() { "Ultimate" },
            Attribute_JPN = new List<string>() { "ウイルス" },
            Attribute_ENG = new List<string>() { "Virus" },
            Type_JPN = new List<string>() { "ダークアニマル" },
            Type_ENG = new List<string>() { "Dark Animal" },
            CardSpriteName = "LM-018-token",
            cardKind = CardKind.Digimon,
            DP = 3000,
        };

        await GyuukimonToken.GetCardSprite();

        KoHagurumonToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Black },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "",
            CardName_ENG = "KoHagurumon",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "BT16-052-token",
            cardKind = CardKind.Digimon,
            DP = 1000,
            CardEffectClassName = "BT16_052_token"
        };

        await KoHagurumonToken.GetCardSprite();
        
        FamiliarToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Yellow },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "",
            CardName_ENG = "Familiar",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "EX7-030-token",
            cardKind = CardKind.Digimon,
            DP = 3000,
            CardEffectClassName = "EX7_030_token"
        };

        await FamiliarToken.GetCardSprite();
        
        SelfDeleteFamiliarToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Yellow },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "",
            CardName_ENG = "Familiar",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "EX7-030-token",
            cardKind = CardKind.Digimon,
            DP = 3000,
            CardEffectClassName = "P_165_token"
        };

        await SelfDeleteFamiliarToken.GetCardSprite();

        VoleeZerdruckenToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Purple },
            PlayCost = -1,
            Level = 4,
            CardName_JPN = "",
            CardName_ENG = "Volée & Zerdrücken",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "EX7-058-token",
            cardKind = CardKind.Digimon,
            DP = 5000,
            CardEffectClassName = "EX7_058_token"
        };

        await VoleeZerdruckenToken.GetCardSprite();

        UkaNoMitamaToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Yellow },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "",
            CardName_ENG = "Uka-no-Mitama",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "EX8-037-token",
            cardKind = CardKind.Digimon,
            DP = 9000,
            CardEffectClassName = "EX8_037_token"
        };

        await UkaNoMitamaToken.GetCardSprite();

        WarGrowlmonToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Red },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "",
            CardName_ENG = "WarGrowlmon",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "BT19-091-token",
            cardKind = CardKind.Digimon,
            DP = 6000
        };

        await WarGrowlmonToken.GetCardSprite();

        TaomonToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Yellow },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "",
            CardName_ENG = "Taomon",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "BT19-091-token",
            cardKind = CardKind.Digimon,
            DP = 6000
        };

        await TaomonToken.GetCardSprite();
        
        RapidmonToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Green },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "",
            CardName_ENG = "Rapidmon",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "BT19-091-token",
            cardKind = CardKind.Digimon,
            DP = 6000
        };

        await RapidmonToken.GetCardSprite();

        PipeFoxToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.Yellow },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "",
            CardName_ENG = "Pipe-Fox",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "BT19-040-token",
            cardKind = CardKind.Digimon,
            DP = 6000,
            CardEffectClassName = "BT19_040_token"
        };

        await PipeFoxToken.GetCardSprite();

        AthoRenePorToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.White },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "",
            CardName_ENG = "Atho, René & Por",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "BT20-017-token",
            cardKind = CardKind.Digimon,
            DP = 6000,
            CardEffectClassName = "BT20_017_token"
        };

        await AthoRenePorToken.GetCardSprite();

        HinukamuyToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.White },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "",
            CardName_ENG = "HinukamuyToken",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "BT23-057-token",
            cardKind = CardKind.Digimon,
            DP = 6000,
            CardEffectClassName = "BT23_057_token"
        };

        await HinukamuyToken.GetCardSprite();

        

        PetrificationToken = new CEntity_Base()
        {
            cardColors = new List<CardColor>() { CardColor.White },
            PlayCost = -1,
            Level = 0,
            CardName_JPN = "",
            CardName_ENG = "Petrification",
            Form_JPN = new List<string>(),
            Form_ENG = new List<string>(),
            Attribute_JPN = new List<string>(),
            Attribute_ENG = new List<string>(),
            Type_JPN = new List<string>(),
            Type_ENG = new List<string>(),
            CardSpriteName = "BT21-029-token",
            cardKind = CardKind.Digimon,
            DP = 3000,
            CardEffectClassName = "BT21_029_token"
        };

        await PetrificationToken.GetCardSprite();
    }

    public static ContinuousController instance = null;

    private void Awake()
    {
        instance = this;
    }

    public async void Init()
    {
        Application.targetFrameRate = 60;
        int random = RandomUtility.getRamdom();
        UnityEngine.Random.InitState(random);
        Debug.Log($"Game Initialize - random number sequence initialization,InitState:{random}");

        Sprite reverseCardSprite = await StreamingAssetsUtility.GetSprite("card_back_main");

        if (reverseCardSprite != null)
        {
            ReverseCard = reverseCardSprite;
        }

        Sprite reverseDigieggCardSprite = await StreamingAssetsUtility.GetSprite("card_back_sub");

        if (reverseDigieggCardSprite != null)
        {
            ReverseCard_Digitama = reverseDigieggCardSprite;
        }

        LoadBanList();

        // deck data
        //DeckDatas = PlayerPrefsUtil.LoadList<DeckData>(DeckDatasPlayerPrefsKey);
        LoadDeckLists();
        GetComponent<StarterDeck>().SetStarterDecks();

        // player data
        LoadPlayerName();
        LoadWinCount();

        // game play
        LoadAutoEffectOrder();
        LoadAutoDeckBottomOrder();
        LoadAutoDeckTopOrder();
        LoadAutoMinDigivolutionCost();
        LoadAutoMaxCardCount();
        LoadAutoHatch();
        LoadShowCutInAnimation();
        LoadReverseOpponentsCards();
        LoadTurnSuspendedCards();
        LoadCheckBeforeEndingSelection();
        LoadSuspendedCardsDirectionIsLeft();

        //Graphics
        LoadShowBackgroundParticle();

        // Sound
        LoadVolume();

        // ServerRegion
        LoadServerRegion();

        // Language
        LoadLanguage();

        await CreateTokenData();

        DontDestroyOnLoad(gameObject);
    }

    [Obsolete("This is obsolete, switching to save files")]
    public void ModifyAllDeckDatas()
    {
        List<DeckData> tempDeckDatas = new List<DeckData>();

        foreach (DeckData deckData in DeckDatas)
        {
            tempDeckDatas.Add(deckData);
        }

        foreach (DeckData deckData in tempDeckDatas)
        {
            if (deckData.AllDeckCards().Count == 0)
            {
                DeckDatas.Remove(deckData);
            }
        }

        for (int i = 0; i < DeckDatas.Count; i++)
        {
            //DeckData deckData = new DeckData(DeckData.GetDeckCode(DeckDatas[i].DeckName, DeckData.SortedDeckCardsList(DeckDatas[i].DeckCards()), DeckData.SortedDeckCardsList(DeckDatas[i].DigitamaDeckCards()), DeckDatas[i].KeyCard));

            DeckData deckData = DeckDatas[i];

            DeckDatas[i] = deckData.ModifiedDeckData();
        }

        SaveDeckDatas();
    }

    [Obsolete("This is obsolete, switching to save files")]
    public void SaveDeckDatas()
    {
        PlayerPrefsUtil.SaveList(DeckDatasPlayerPrefsKey, DeckDatas);

        PlayerPrefs.Save();
    }

    public void SaveDeckData(DeckData data)
    {
        string savePath = StreamingAssetsUtility.GetStreamingAssetPath("Decks", false);

        File.WriteAllText($"{savePath}/{data.DeckName}_{data.DeckID}.txt", DeckCodeUtility.GetDeckBuilderFile(data));
    }

    public void RenameDeck(DeckData data, string newName)
    {
        string savePath = StreamingAssetsUtility.GetStreamingAssetPath("Decks", false);
        if (File.Exists($"{savePath}/{data.DeckName}_{data.DeckID}.txt"))
        {
            File.Move($"{savePath}/{data.DeckName}_{data.DeckID}.txt", $"{savePath}/{newName}_{data.DeckID}.txt");
            data.DeckName = newName;
            SaveDeckData(data);
        }
        else
            data.DeckName = newName;
    }

    public void DeleteDeck(DeckData data)
    {
        string filePath = StreamingAssetsUtility.GetStreamingAssetPath("Decks", false);

        if (!Directory.Exists(filePath))
            return;

        if (!File.Exists($"{filePath}/{data.DeckName}_{data.DeckID}.txt"))
            return;

        File.Delete($"{filePath}/{data.DeckName}_{data.DeckID}.txt");
    }

    public void DeleteAllDecks()
    {
        foreach(DeckData data in DeckDatas)
        {
            DeleteDeck(data);
        }
    }

    public void LoadDeckLists()
    {
        string loadPath = StreamingAssetsUtility.GetStreamingAssetPath("Decks", false);

        if (!Directory.Exists(loadPath))
            return;

        string[] deckLists = Directory.GetFiles(loadPath);

        foreach(string deckPath in deckLists)
        {
            string fileName = Path.GetFileNameWithoutExtension(deckPath);

            if (!fileName.Contains("_"))
                continue;

            string deckList = File.ReadAllText(deckPath);

            StreamReader sr = new StreamReader(deckPath);


            string deckName = sr.ReadLine().Replace("Name: ", "");
            int KeyCard = int.Parse(sr.ReadLine().Replace("Key Card: ", ""));
            int SortValue = int.Parse(sr.ReadLine().Replace("Sort Index: ", ""));

            sr.Close();

            string deck = deckList.Substring(deckList.IndexOf("//"));
            //Debug.Log(deckName);

            if(SortValue < 0)
                SortValue = 0;

            CreateDeckFromFile(fileName.Split("_")[1], deckName, KeyCard, deck, SortValue);
        }

        DeckDatas = DeckDatas.OrderBy(x => x.DeckName).ToList();
    }

    private void CreateDeckFromFile(string id, string name, int keyID, string deckCode, int index = 0)
    {
        List<CEntity_Base> AllDeckCards = DeckCodeUtility.GetAllDeckCardsFromDeckBuilderDeckCode(deckCode);

        if (AllDeckCards.Count == 0)
        {
            AllDeckCards = DeckCodeUtility.GetAllDeckCardsFromTTSDeckCode(deckCode);
        }

        List<CEntity_Base> deckCards = new List<CEntity_Base>();
        List<CEntity_Base> digitamaDeckCards = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in AllDeckCards)
        {
            if (cEntity_Base.cardKind == CardKind.DigiEgg)
            {
                digitamaDeckCards.Add(cEntity_Base);
            }

            else
            {
                deckCards.Add(cEntity_Base);
            }
        }
        Debug.Log($"Create Deck From File: {name}");
        DeckData deckData = (new DeckData(DeckData.GetDeckCode(name, deckCards, digitamaDeckCards, null),id)).ModifiedDeckData();

        deckData.KeyCardId = keyID;
        deckData.DeckName = name;
        deckData.SortValue = index;

        DeckDatas.Insert(index, deckData);
    }

    #region Player Name
    string _playerName;
    string _playerNameKey = "PlayerName";
    public string PlayerName
    {
        get
        {
            if (string.IsNullOrEmpty(_playerName))
            {
                return "Player";
            }

            return _playerName;
        }

        set
        {
            _playerName = DeckData.ValidateDeckName(value);
        }
    }

    public void SavePlayerName(string playerName)
    {
        PlayerName = playerName;
        PlayerPrefs.SetString(_playerNameKey, playerName);
        PlayerPrefs.Save();
    }

    public void LoadPlayerName()
    {
        if (PlayerPrefs.HasKey(_playerNameKey))
        {
            PlayerName = PlayerPrefs.GetString(_playerNameKey);
        }


        if (string.IsNullOrEmpty(PlayerName))
        {
            PlayerName = "Player";
        }
    }
    #endregion

    #region number of victories
    public int WinCount { get; set; }
    string _winCountKey = "WinCount";

    public void SaveWinCount()
    {
        PlayerPrefs.SetInt(_winCountKey, WinCount);
        PlayerPrefs.Save();
    }
    public void LoadWinCount()
    {
        if (PlayerPrefs.HasKey(_winCountKey))
        {
            WinCount = PlayerPrefs.GetInt(_winCountKey);
        }

    }
    #endregion

    #region Auto effect order
    [HideInInspector] public bool autoEffectOrder = false;
    string _autoEffectOrderKey = "AutoEffectOrder";

    public void SaveAutoEffectOrder()
    {
        PlayerPrefsUtil.SetBool(_autoEffectOrderKey, autoEffectOrder);
        PlayerPrefs.Save();
    }
    public void LoadAutoEffectOrder()
    {
        autoEffectOrder = PlayerPrefsUtil.GetBool(_autoEffectOrderKey, false);
    }
    #endregion

    #region Auto deck bottom order
    [HideInInspector] public bool autoDeckBottomOrder = false;
    string _autoDeckBottomOrderKey = "AutoDeckBottomOrder";

    public void SaveAutoDeckBottomOrder()
    {
        PlayerPrefsUtil.SetBool(_autoDeckBottomOrderKey, autoDeckBottomOrder);
        PlayerPrefs.Save();
    }
    public void LoadAutoDeckBottomOrder()
    {
        autoDeckBottomOrder = PlayerPrefsUtil.GetBool(_autoDeckBottomOrderKey, false);
    }
    #endregion

    #region Auto deck top order
    [HideInInspector] public bool autoDeckTopOrder = false;
    string _autoDeckTopOrderKey = "AutoDeckTopOrder";

    public void SaveAutoDeckTopOrder()
    {
        PlayerPrefsUtil.SetBool(_autoDeckTopOrderKey, autoDeckTopOrder);
        PlayerPrefs.Save();
    }
    public void LoadAutoDeckTopOrder()
    {
        autoDeckTopOrder = PlayerPrefsUtil.GetBool(_autoDeckTopOrderKey, false);
    }
    #endregion

    #region Auto min digivolution cost
    [HideInInspector] public bool autoMinDigivolutionCost = false;
    string _autoMinDigivolutionCostKey = "AutoMinDigivolutionCost";

    public void SaveAutoMinDigivolutionCost()
    {
        PlayerPrefsUtil.SetBool(_autoMinDigivolutionCostKey, autoMinDigivolutionCost);
        PlayerPrefs.Save();
    }
    public void LoadAutoMinDigivolutionCost()
    {
        autoMinDigivolutionCost = PlayerPrefsUtil.GetBool(_autoMinDigivolutionCostKey, false);
    }
    #endregion

    #region Auto max card count
    [HideInInspector] public bool autoMaxCardCount = false;
    string _autoMaxCardCountKey = "AutoMaxCardCount";

    public void SaveAutoMaxCardCount()
    {
        PlayerPrefsUtil.SetBool(_autoMaxCardCountKey, autoMaxCardCount);
        PlayerPrefs.Save();
    }
    public void LoadAutoMaxCardCount()
    {
        autoMaxCardCount = PlayerPrefsUtil.GetBool(_autoMaxCardCountKey, false);
    }
    #endregion

    #region Auto hatch
    [HideInInspector] public bool autoHatch = false;
    string _autoHatchKey = "AutoHatch";

    public void SaveAutoHatch()
    {
        PlayerPrefsUtil.SetBool(_autoHatchKey, autoHatch);
        PlayerPrefs.Save();
    }
    public void LoadAutoHatch()
    {
        autoHatch = PlayerPrefsUtil.GetBool(_autoHatchKey, false);
    }
    #endregion

    #region Show CutIn Animation
    [HideInInspector] public bool showCutInAnimation = false;
    string _showCutInAnimationKey = "ShowCutInAnimation";

    public void SaveShowCutInAnimation()
    {
        PlayerPrefsUtil.SetBool(_showCutInAnimationKey, showCutInAnimation);
        PlayerPrefs.Save();
    }
    public void LoadShowCutInAnimation()
    {
        //TODO: Setting default to false, to fix animation syncing bug, MB
        showCutInAnimation = false;
        //showCutInAnimation = PlayerPrefsUtil.GetBool(_showCutInAnimationKey, true);
    }
    #endregion

    #region Reverse opponents' cards
    [HideInInspector] public bool reverseOpponentsCards = false;
    string _reverseOpponentsCardsKey = "ReverseOpponentsCards";

    public void SaveReverseOpponentsCards()
    {
        PlayerPrefsUtil.SetBool(_reverseOpponentsCardsKey, reverseOpponentsCards);
        PlayerPrefs.Save();
    }
    public void LoadReverseOpponentsCards()
    {
        reverseOpponentsCards = PlayerPrefsUtil.GetBool(_reverseOpponentsCardsKey, false);
    }
    #endregion

    #region Turn suspended cards
    [HideInInspector] public bool turnSuspendedCards = false;
    string _turnSuspendedCardsKey = "TurnSuspendedCards";

    public void SaveTurnSuspendedCards()
    {
        PlayerPrefsUtil.SetBool(_turnSuspendedCardsKey, turnSuspendedCards);
        PlayerPrefs.Save();
    }
    public void LoadTurnSuspendedCards()
    {
        turnSuspendedCards = PlayerPrefsUtil.GetBool(_turnSuspendedCardsKey, true);
    }
    #endregion

    #region Check before ending selection
    [HideInInspector] public bool checkBeforeEndingSelection = false;
    string _checkBeforeEndingSelectionKey = "CheckBeforeEndingSelection";

    public void SaveCheckBeforeEndingSelection()
    {
        PlayerPrefsUtil.SetBool(_checkBeforeEndingSelectionKey, checkBeforeEndingSelection);
        PlayerPrefs.Save();
    }
    public void LoadCheckBeforeEndingSelection()
    {
        checkBeforeEndingSelection = PlayerPrefsUtil.GetBool(_checkBeforeEndingSelectionKey, true);
    }
    #endregion

    #region Suspended cards' direction is left
    [HideInInspector] public bool suspendedCardsDirectionIsLeft = false;
    string _suspendedCardsDirectionIsLeftKey = "SuspendedCardsDirectionIsLeft";

    public void SaveSuspendedCardsDirectionIsLeft()
    {
        PlayerPrefsUtil.SetBool(_suspendedCardsDirectionIsLeftKey, suspendedCardsDirectionIsLeft);
        PlayerPrefs.Save();
    }
    public void LoadSuspendedCardsDirectionIsLeft()
    {
        suspendedCardsDirectionIsLeft = PlayerPrefsUtil.GetBool(_suspendedCardsDirectionIsLeftKey, true);
    }
    #endregion

    #region Show background particle
    [HideInInspector] public bool showBackgroundParticle = false;
    string _showBackgroundParticleKey = "ShowBackgroundParticle";

    public void SaveShowBackgroundParticle()
    {
        PlayerPrefsUtil.SetBool(_showBackgroundParticleKey, showBackgroundParticle);
        PlayerPrefs.Save();
    }
    public void LoadShowBackgroundParticle()
    {
        showBackgroundParticle = PlayerPrefsUtil.GetBool(_showBackgroundParticleKey, true);
    }
    #endregion

    #region Sound volume
    public float BGMVolume { get; set; }
    public float SEVolume { get; set; }

    public void SetBGMVolume(float BGMVolume)
    {
        this.BGMVolume = BGMVolume;

        PlayerPrefs.SetFloat("BGMVolume", BGMVolume);
        PlayerPrefs.Save();
    }

    public void SetSEVolume(float SEVolume)
    {
        this.SEVolume = SEVolume;

        PlayerPrefs.SetFloat("SEVolume", SEVolume);
        PlayerPrefs.Save();
    }

    public void ChangeBGMVolume(AudioSource audioSource)
    {
        audioSource.volume = BGMVolume * 0.25f * 0.8f;
    }

    public void ChangeSEVolume(AudioSource audioSource)
    {
        audioSource.volume = SEVolume * 0.5f * 0.8f;
    }

    void LoadVolume()
    {
        BGMVolume = 0.5f;
        SEVolume = 0.5f;

        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            BGMVolume = PlayerPrefs.GetFloat("BGMVolume");
        }

        if (PlayerPrefs.HasKey("SEVolume"))
        {
            SEVolume = PlayerPrefs.GetFloat("SEVolume");
        }
    }
    #endregion

    #region Server region
    [HideInInspector] public string serverRegion = "us";
    string _serverRegionKey = "ServerRegion";

    public void SaveServerRegion()
    {
        PlayerPrefs.SetString(_serverRegionKey, serverRegion);
        PlayerPrefs.Save();
    }
    public void LoadServerRegion()
    {
        serverRegion = PlayerPrefs.GetString(_serverRegionKey, "us");
    }
    public string LastConnectServerRegion = "";
    #endregion

    #region Language
    [HideInInspector] public Language language = Language.ENG;
    string _languageKey = "Language";

    public void SaveLanguage()
    {
        PlayerPrefs.SetString(_languageKey, language.ToString());
        PlayerPrefs.Save();
    }
    public void LoadLanguage()
    {
        language = (Language)Enum.Parse(typeof(Language), PlayerPrefs.GetString(_languageKey, "ENG"));
    }
    #endregion

    #region PlaySE(AudioClip clip)
    public SoundObject PlaySE(AudioClip clip)
    {
        SoundObject _soundObject = Instantiate(soundObject);

        _soundObject.PlaySE(clip);

        return _soundObject;
    }
    #endregion

    #region カードIndexからカードを取得
    public CEntity_Base getCardEntityByCardID(int cardIndex)
    {
        //int searchIndex = cardIndex - 1;
        //int count = 0;

        CEntity_Base cEntity_Base = SortedCardList.First(entity => entity.CardIndex == cardIndex);

        return cEntity_Base;

        //TODO: REMOVE IN FUTURE
        /*do
        {
            if (count != 0)
            {
                searchIndex += (int)Math.Pow(-1, count % 2) * count / 2;
            }

            if (0 <= searchIndex)
            {
                if (searchIndex <= SortedCardList.Length - 1)
                {
                    CEntity_Base cEntity_Base = SortedCardList[searchIndex];

                    if (cEntity_Base != null)
                    {
                        if (cEntity_Base.CardIndex == cardIndex)
                        {
                            return cEntity_Base;
                        }
                    }
                }

                else
                {
                    for (int i = 0; i < 300; i++)
                    {
                        CEntity_Base cEntity_Base = SortedCardList[SortedCardList.Length - 1 - i];

                        if (cEntity_Base != null)
                        {
                            if (cEntity_Base.CardIndex == cardIndex)
                            {
                                return cEntity_Base;
                            }
                        }
                    }

                    return null;
                }
            }

            if (count != 0)
            {
                searchIndex -= (int)Math.Pow(-1, count % 2) * count / 2;
            }

            count++;
        }

        while (count <= 20);

        return null;*/
    }
    #endregion

    public Coroutine LoadingTextCoroutine;

    bool _endBattle = false;

    public void EndBattle()
    {
        if (!_endBattle)
        {
            _endBattle = true;
            StartCoroutine(EndBattleCoroutine());
        }
    }
    public IEnumerator EndBattleCoroutine()
    {
        if (Opening.instance == null)
        {
            yield break;
        }

        Opening.instance.openingObject.SetActive(true);

        //yield return StartCoroutine(Opening.instance.LoadingObject_Unload.StartLoading("Now Loading"));

        //Camera camera1 = Camera.main;

        //Destroy(camera1.gameObject);

        //yield return null;

        isAI = false;

        int random = RandomUtility.getRamdom();
        UnityEngine.Random.InitState(random);
        Debug.Log($"random number sequence initialization, InitState:{random}");

        var unload = SceneManager.UnloadSceneAsync("BattleScene");
        yield return unload;

        yield return Resources.UnloadUnusedAssets();

        yield return StartCoroutine(Opening.instance.LoadingObject_Unload.StartLoading("Now Loading"));

        //Opening.instance.MainCamera.gameObject.SetActive(true);

        foreach (Camera camera in Opening.instance.openingCameras)
        {
            camera.gameObject.SetActive(true);
        }

        Opening.instance.LoadingObject_light.gameObject.SetActive(false);
        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.SetPlayerName());

        if (isRandomMatch)
        {
            Debug.Log("Unload from Random Match");
            yield return StartCoroutine(Opening.instance.battle.lobbyManager_RandomMatch.CloseLobbyCoroutine());
            yield return StartCoroutine(Opening.instance.battle.selectBattleMode.SetUpSelectBattleModeCoroutine());
        }

        else
        {
            Debug.Log("Unload from Room Match");
            yield return StartCoroutine(Opening.instance.battle.roomManager.Init(true));
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitWhile(() => GManager.instance != null);

        Opening.instance.LoadingObject.gameObject.SetActive(false);
        yield return StartCoroutine(Opening.instance.LoadingObject_Unload.EndLoading());
        _endBattle = false;

        if (!isRandomMatch)
        {
            Hashtable PlayerProp = PhotonNetwork.LocalPlayer.CustomProperties;

            if (PlayerProp.TryGetValue("isBattle", out object value))
            {
                PlayerProp["isBattle"] = false;
            }

            else
            {
                PlayerProp.Add("isBattle", false);
            }

            PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerProp);
        }

        Scene newScene = SceneManager.GetSceneByName("Opening");
        SceneManager.SetActiveScene(newScene);

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.1f);

            EventSystem.current.SetSelectedGameObject(Opening.instance.battle.selectBattleMode.transform.GetChild(0).gameObject);
        }

        //GUI.UnfocusWindow();

        yield return null;

        //StartCoroutine(DestroyEffectCoroutine());

        if (Opening.instance.OpeningBGM != null)
        {
            if (!Opening.instance.OpeningBGM.isPlaying)
            {
                Opening.instance.OpeningBGM.StartPlayBGM(Opening.instance.bgm);
            }
        }
    }
    private void Update()
    {
#if UNITY_WINDOWS
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
#endif
    }
    int _frameCount = 0;
    int _updateFrame = 40;
    void LateUpdate()
    {
        #region Update only once every few frames
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

        if (PhotonNetwork.InRoom)
        {
            if (!isAI)
            {
                bool notEnterOther = false;

                if (PhotonNetwork.PlayerList.Length == 1)
                {
                    if (GManager.instance != null)
                    {
                        notEnterOther = true;
                    }
                }

                if (notEnterOther)
                {
                    if (PhotonNetwork.CurrentRoom.MaxPlayers != 1)
                    {
                        PhotonNetwork.CurrentRoom.MaxPlayers = 1;
                    }
                }

                else
                {
                    if (PhotonNetwork.CurrentRoom.MaxPlayers != 2)
                    {
                        PhotonNetwork.CurrentRoom.MaxPlayers = 2;
                    }
                }
            }
        }
    }

    public bool isAI { get; set; } = false;

    //Flag that the sharing of the random number sequence is over.
    public bool DoneSetRandom { get; set; } = false;
    public bool CanSetRandom { get; set; } = false;
    [PunRPC]
    public void SetRandom(int random)
    {
        StartCoroutine(SetRandomCoroutine(random));
    }

    IEnumerator SetRandomCoroutine(int random)
    {
        yield return new WaitWhile(() => !CanSetRandom);

        UnityEngine.Random.InitState(random);
        DoneSetRandom = true;

        Debug.Log($"random number sequence initialization,InitState:{random}");
    }


}

#region Manage random numbers
public static class RandomUtility
{
    private static System.Random random;
    public static int getRamdom()
    {
        int _max = 1500000000;

        if (random == null)
        {
            random = new System.Random((int)DateTime.Now.Ticks);
        }

        return random.Next(0, _max);
    }

    #region IsSucceedProbability(float Probability)
    public static bool IsSucceedProbability(float Probability)
    {
        if (Probability >= 1)
        {
            return true;
        }

        if (Probability <= 0)
        {
            return false;
        }

        float random = UnityEngine.Random.Range(0f, 1f);

        if (random <= Probability)
        {
            return true;
        }

        return false;
    }
    #endregion

    #region Shuffle the deck
    public static List<CEntity_Base> ShuffledDeckCards(List<CEntity_Base> DeckCards)
    {
        List<CEntity_Base> CardDatas = new List<CEntity_Base>();
        CardDatas.AddRange(DeckCards);

        // The initial value of the integer n is the number of cards in the deck
        int n = CardDatas.Count;

        while (n > 0)
        {
            n--;

            // Random index from 0 to i (inclusive)
            int k = UnityEngine.Random.Range(0, n + 1);

            // Swap elements at indices i and k
            CEntity_Base temp = CardDatas[n];
            CardDatas[n] = CardDatas[k];
            CardDatas[k] = temp;
        }


        return CardDatas;
    }

    public static List<CardSource> ShuffledDeckCards(List<CardSource> DeckCards)
    {
        List<CardSource> CardDatas = new List<CardSource>();
        CardDatas.AddRange(DeckCards);

        // The initial value of the integer n is the number of cards in the deck
        int n = CardDatas.Count;

        while (n > 0)
        {
            n--;

            // Random index from 0 to i (inclusive)
            int k = UnityEngine.Random.Range(0, n + 1);

            // Swap elements at indices i and k
            CardSource temp = CardDatas[n];

            if (!temp.IsFlipped)
            {
                temp.SetReverse();

                if(temp.Owner.SecurityCards.Contains(temp))
                    GManager.OnSecurityStackChanged?.Invoke(temp.Owner);
            }
                

            CardDatas[n] = CardDatas[k];
            CardDatas[k] = temp;
        }

        return CardDatas;
    }

    #endregion
}
#endregion

#region Manage connections to Photon
public class PhotonUtility
{
    #region Disconnected from Photon
    public static IEnumerator DisconnectCoroutine()
    {
        #region Exit Room
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        yield return new WaitWhile(() => PhotonNetwork.InRoom);
        #endregion

        #region Exit from the lobby
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        yield return new WaitWhile(() => PhotonNetwork.InLobby);
        #endregion

        #region Disconnected from Photon
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        yield return new WaitWhile(() => PhotonNetwork.IsConnected);
        #endregion
    }
    #endregion

    #region Connect to Photon server
    public static IEnumerator ConnectToMasterServerCoroutine()
    {
        if (!PhotonNetwork.IsConnected || ContinuousController.instance.LastConnectServerRegion != ContinuousController.instance.serverRegion)
        {
            if (PhotonNetwork.IsConnected)
            {
                yield return ContinuousController.instance.StartCoroutine(DisconnectCoroutine());

                yield return new WaitWhile(() => PhotonNetwork.IsConnected);
            }

            PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
            PhotonNetwork.ConnectToRegion(ContinuousController.instance.serverRegion);
            PhotonNetwork.NickName = ContinuousController.instance.PlayerName;
            PhotonNetwork.GameVersion = ContinuousController.instance.GameVerString;
            ContinuousController.instance.LastConnectServerRegion = ContinuousController.instance.serverRegion;
        }

        yield return new WaitWhile(() => !PhotonNetwork.IsConnectedAndReady);
    }
    #endregion
    #region Connect to Photon Server and Lobby
    public static IEnumerator ConnectToLobbyCoroutine()
    {
        #region Connect to Photon server
        yield return ContinuousController.instance.StartCoroutine(ConnectToMasterServerCoroutine());
        #endregion

        #region Save player name to custom properties
        yield return ContinuousController.instance.StartCoroutine(SetPlayerName());
        #endregion

        #region Save the number of wins to a custom property
        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

        object value;

        hash = PhotonNetwork.LocalPlayer.CustomProperties;

        if (hash.TryGetValue(ContinuousController.WinCountKey, out value))
        {
            hash[ContinuousController.WinCountKey] = ContinuousController.instance.WinCount;
        }

        else
        {
            hash.Add(ContinuousController.WinCountKey, ContinuousController.instance.WinCount);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        while (true)
        {
            Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (_hash.TryGetValue(ContinuousController.WinCountKey, out value))
            {
                if ((int)value == ContinuousController.instance.WinCount)
                {
                    break;
                }

            }

            yield return null;
        }
        #endregion

        #region Connect to Lobby
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        yield return new WaitWhile(() => !PhotonNetwork.InLobby);

        yield return new WaitUntil(() => PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady);
        #endregion
    }
    #endregion

    #region Save player name to properties
    public static IEnumerator SetPlayerName()
    {
        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

        object value;

        hash = PhotonNetwork.LocalPlayer.CustomProperties;

        if (hash.TryGetValue(ContinuousController.PlayerNameKey, out value))
        {
            hash[ContinuousController.PlayerNameKey] = ContinuousController.instance.PlayerName;
        }

        else
        {
            hash.Add(ContinuousController.PlayerNameKey, ContinuousController.instance.PlayerName);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        while (true)
        {
            Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (_hash.TryGetValue(ContinuousController.PlayerNameKey, out value))
            {
                if ((string)value == ContinuousController.instance.PlayerName)
                {
                    break;
                }
            }

            yield return null;
        }
    }
    #endregion

    #region Save deck data to custom properties
    public static IEnumerator SignUpBattleDeckData()
    {
        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

        if (hash.TryGetValue(ContinuousController.DeckDataPropertyKey, out object value))
        {
            hash[ContinuousController.DeckDataPropertyKey] = ContinuousController.instance.BattleDeckData.GetThisDeckCode();
        }

        else
        {
            hash.Add(ContinuousController.DeckDataPropertyKey, ContinuousController.instance.BattleDeckData.GetThisDeckCode());
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        while (true)
        {
            Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (_hash.TryGetValue(ContinuousController.DeckDataPropertyKey, out value))
            {
                if ((string)value == ContinuousController.instance.BattleDeckData.GetThisDeckCode())
                {
                    break;
                }
            }

            yield return null;
        }
    }
    #endregion

    #region Remove custom properties from deck data
    public static IEnumerator DeleteBattleDeckData()
    {
        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

        if (hash.TryGetValue(ContinuousController.DeckDataPropertyKey, out object value))
        {
            hash.Remove(ContinuousController.DeckDataPropertyKey);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        while (true)
        {
            Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;

            if (!_hash.TryGetValue(ContinuousController.DeckDataPropertyKey, out value))
            {
                break;
            }

            yield return null;
        }
    }
    #endregion
}
#endregion

public enum Language
{
    ENG,
    JPN,
}