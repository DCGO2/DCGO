using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(StarterDeck))]
public class ContinuousController : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);
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
    public bool isRanked { get; set; }
    public bool isTournament { get; set; }

    public static bool IsBattleSceneLoaded()
    {
        var scene = SceneManager.GetSceneByName("BattleScene");
        return scene.IsValid() && scene.isLoaded;
    }

    // === DCGO-CUSTOM:friends begin ===
    public bool isFriendDuel { get; set; }
    public int FriendWinsToTake { get; set; } = 1;

    public void ClearFriendDuel()
    {
        isFriendDuel = false;
        FriendWinsToTake = 1;
        FriendServices.Instance?.Director?.ResetDirector();
    }
    // === DCGO-CUSTOM:friends end ===
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
    public CEntity_Base PaishuToken { get; private set; }
    public CEntity_Base KotenkenToken { get; private set; }

    //public CardRestriction BanList { get; private set; } = new CardRestriction(new List<CardLimitCount>(), new List<BannedPair>());
    public BanList BanList { get; private set; } = new BanList();


    async Task LoadBanListOnline()
    {
        string url = "https://www.dcgo.online/Banlist.json";
        UnityWebRequest jsonWebRequest = UnityWebRequest.Get(url);

        UnityWebRequestAsyncOperation operation = jsonWebRequest.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield(); // Keep the method asynchronous without blocking
        }

        if (jsonWebRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(jsonWebRequest.error);
            useBanlist = false;
        }
        else
        {
            BanList = JsonUtility.FromJson<BanList>(jsonWebRequest.downloadHandler.text);
        }
    }

    async Task CreateTokenData()
    {
        DiaboromonToken = ScriptableObject.CreateInstance<CEntity_Base>();

        DiaboromonToken.cardColors = new List<CardColor>() { CardColor.White };
        DiaboromonToken.PlayCost = 14;
        DiaboromonToken.Level = 6;
        DiaboromonToken.CardName_JPN = "ディアボロモン";
        DiaboromonToken.CardName_ENG = "Diaboromon";
        DiaboromonToken.Form_JPN = new List<string>() { "究極体" };
        DiaboromonToken.Form_ENG = new List<string>() { "Mega" };
        DiaboromonToken.Attribute_JPN = new List<string>() { "不明" };
        DiaboromonToken.Attribute_ENG = new List<string>() { "Unknown" };
        DiaboromonToken.Type_JPN = new List<string>() { "種族不明" };
        DiaboromonToken.Type_ENG = new List<string>() { "Unidentified" };
        DiaboromonToken.CardSpriteName = "BT2-082-token";
        DiaboromonToken.cardKind = new List<CardKind> { CardKind.Digimon };
        DiaboromonToken.DP = 3000;

        await DiaboromonToken.GetCardSprite();

        AmonToken = ScriptableObject.CreateInstance<CEntity_Base>();
        AmonToken.cardColors = new List<CardColor>() { CardColor.Red };
        AmonToken.PlayCost = -1;
        AmonToken.Level = 0;
        AmonToken.CardName_JPN = "紅炎のアモン";
        AmonToken.CardName_ENG = "Amon of Crimson Flame";
        AmonToken.Form_JPN = new List<string>();
        AmonToken.Form_ENG = new List<string>();
        AmonToken.Attribute_JPN = new List<string>();
        AmonToken.Attribute_ENG = new List<string>();
        AmonToken.Type_JPN = new List<string>();
        AmonToken.Type_ENG = new List<string>();
        AmonToken.CardSpriteName = "BT14-018-token-red";
        AmonToken.cardKind = new List<CardKind> { CardKind.Digimon };
        AmonToken.DP = 6000;
        AmonToken.CardEffectClassName = "BT4_038";

        await AmonToken.GetCardSprite();

        UmonToken = ScriptableObject.CreateInstance<CEntity_Base>();
        UmonToken.cardColors = new List<CardColor>() { CardColor.Yellow };
        UmonToken.PlayCost = -1;
        UmonToken.Level = 0;
        UmonToken.CardName_JPN = "蒼雷のウモン";
        UmonToken.CardName_ENG = "Umon of Blue Thunder";
        UmonToken.Form_JPN = new List<string>();
        UmonToken.Form_ENG = new List<string>();
        UmonToken.Attribute_JPN = new List<string>();
        UmonToken.Attribute_ENG = new List<string>();
        UmonToken.Type_JPN = new List<string>();
        UmonToken.Type_ENG = new List<string>();
        UmonToken.CardSpriteName = "BT14-018-token-yellow";
        UmonToken.cardKind = new List<CardKind> { CardKind.Digimon };
        UmonToken.DP = 6000;
        UmonToken.CardEffectClassName = "BT1_031";

        await UmonToken.GetCardSprite();

        FujitsumonToken = ScriptableObject.CreateInstance<CEntity_Base>();
        FujitsumonToken.cardColors = new List<CardColor>() { CardColor.Purple };
        FujitsumonToken.PlayCost = -1;
        FujitsumonToken.Level = 0;
        FujitsumonToken.CardName_JPN = "フジツモン";
        FujitsumonToken.CardName_ENG = "Fujitsumon";
        FujitsumonToken.Form_JPN = new List<string>();
        FujitsumonToken.Form_ENG = new List<string>();
        FujitsumonToken.Attribute_JPN = new List<string>();
        FujitsumonToken.Attribute_ENG = new List<string>();
        FujitsumonToken.Type_JPN = new List<string>();
        FujitsumonToken.Type_ENG = new List<string>();
        FujitsumonToken.CardSpriteName = "EX5-058-token";
        FujitsumonToken.cardKind = new List<CardKind> { CardKind.Digimon };
        FujitsumonToken.DP = 3000;
        FujitsumonToken.CardEffectClassName = "EX5_058_token";

        await FujitsumonToken.GetCardSprite();

        GyuukimonToken = ScriptableObject.CreateInstance<CEntity_Base>();
        GyuukimonToken.cardColors = new List<CardColor>() { CardColor.Purple };
        GyuukimonToken.PlayCost = 7;
        GyuukimonToken.Level = 5;
        GyuukimonToken.CardName_JPN = "ギュウキモン";
        GyuukimonToken.CardName_ENG = "Gyuukimon";
        GyuukimonToken.Form_JPN = new List<string>() { "究極の" };
        GyuukimonToken.Form_ENG = new List<string>() { "Ultimate" };
        GyuukimonToken.Attribute_JPN = new List<string>() { "ウイルス" };
        GyuukimonToken.Attribute_ENG = new List<string>() { "Virus" };
        GyuukimonToken.Type_JPN = new List<string>() { "ダークアニマル" };
        GyuukimonToken.Type_ENG = new List<string>() { "Dark Animal" };
        GyuukimonToken.CardSpriteName = "LM-018-token";
        GyuukimonToken.cardKind = new List<CardKind> { CardKind.Digimon };
        GyuukimonToken.DP = 3000;
        GyuukimonToken.CardEffectClassName = "LM_018_token";

        await GyuukimonToken.GetCardSprite();

        KoHagurumonToken = ScriptableObject.CreateInstance<CEntity_Base>();
        KoHagurumonToken.cardColors = new List<CardColor>() { CardColor.Black };
        KoHagurumonToken.PlayCost = -1;
        KoHagurumonToken.Level = 0;
        KoHagurumonToken.CardName_JPN = "";
        KoHagurumonToken.CardName_ENG = "KoHagurumon";
        KoHagurumonToken.Form_JPN = new List<string>();
        KoHagurumonToken.Form_ENG = new List<string>();
        KoHagurumonToken.Attribute_JPN = new List<string>();
        KoHagurumonToken.Attribute_ENG = new List<string>();
        KoHagurumonToken.Type_JPN = new List<string>();
        KoHagurumonToken.Type_ENG = new List<string>();
        KoHagurumonToken.CardSpriteName = "BT16-052-token";
        KoHagurumonToken.cardKind = new List<CardKind> { CardKind.Digimon };
        KoHagurumonToken.DP = 1000;
        KoHagurumonToken.CardEffectClassName = "BT16_052_token";

        await KoHagurumonToken.GetCardSprite();
        
        FamiliarToken = ScriptableObject.CreateInstance<CEntity_Base>();
        FamiliarToken.cardColors = new List<CardColor>() { CardColor.Yellow };
        FamiliarToken.PlayCost = -1;
        FamiliarToken.Level = 0;
        FamiliarToken.CardName_JPN = "";
        FamiliarToken.CardName_ENG = "Familiar";
        FamiliarToken.Form_JPN = new List<string>();
        FamiliarToken.Form_ENG = new List<string>();
        FamiliarToken.Attribute_JPN = new List<string>();
        FamiliarToken.Attribute_ENG = new List<string>();
        FamiliarToken.Type_JPN = new List<string>();
        FamiliarToken.Type_ENG = new List<string>();
        FamiliarToken.CardSpriteName = "EX7-030-token";
        FamiliarToken.cardKind = new List<CardKind> { CardKind.Digimon };
        FamiliarToken.DP = 3000;
        FamiliarToken.CardEffectClassName = "EX7_030_token";

        await FamiliarToken.GetCardSprite();
        
        SelfDeleteFamiliarToken = ScriptableObject.CreateInstance<CEntity_Base>();
        SelfDeleteFamiliarToken.cardColors = new List<CardColor>() { CardColor.Yellow };
        SelfDeleteFamiliarToken.PlayCost = -1;
        SelfDeleteFamiliarToken.Level = 0;
        SelfDeleteFamiliarToken.CardName_JPN = "";
        SelfDeleteFamiliarToken.CardName_ENG = "Familiar";
        SelfDeleteFamiliarToken.Form_JPN = new List<string>();
        SelfDeleteFamiliarToken.Form_ENG = new List<string>();
        SelfDeleteFamiliarToken.Attribute_JPN = new List<string>();
        SelfDeleteFamiliarToken.Attribute_ENG = new List<string>();
        SelfDeleteFamiliarToken.Type_JPN = new List<string>();
        SelfDeleteFamiliarToken.Type_ENG = new List<string>();
        SelfDeleteFamiliarToken.CardSpriteName = "EX7-030-token";
        SelfDeleteFamiliarToken.cardKind = new List<CardKind> { CardKind.Digimon };
        SelfDeleteFamiliarToken.DP = 3000;
        SelfDeleteFamiliarToken.CardEffectClassName = "P_165_token";

        await SelfDeleteFamiliarToken.GetCardSprite();

        VoleeZerdruckenToken = ScriptableObject.CreateInstance<CEntity_Base>();
        VoleeZerdruckenToken.cardColors = new List<CardColor>() { CardColor.Purple };
        VoleeZerdruckenToken.PlayCost = -1;
        VoleeZerdruckenToken.Level = 4;
        VoleeZerdruckenToken.CardName_JPN = "";
        VoleeZerdruckenToken.CardName_ENG = "Volée & Zerdrücken";
        VoleeZerdruckenToken.Form_JPN = new List<string>();
        VoleeZerdruckenToken.Form_ENG = new List<string>();
        VoleeZerdruckenToken.Attribute_JPN = new List<string>();
        VoleeZerdruckenToken.Attribute_ENG = new List<string>();
        VoleeZerdruckenToken.Type_JPN = new List<string>();
        VoleeZerdruckenToken.Type_ENG = new List<string>();
        VoleeZerdruckenToken.CardSpriteName = "EX7-058-token";
        VoleeZerdruckenToken.cardKind = new List<CardKind> { CardKind.Digimon };
        VoleeZerdruckenToken.DP = 5000;
        VoleeZerdruckenToken.CardEffectClassName = "EX7_058_token";

        await VoleeZerdruckenToken.GetCardSprite();

        UkaNoMitamaToken = ScriptableObject.CreateInstance<CEntity_Base>();
        UkaNoMitamaToken.cardColors = new List<CardColor>() { CardColor.Yellow };
        UkaNoMitamaToken.PlayCost = -1;
        UkaNoMitamaToken.Level = 0;
        UkaNoMitamaToken.CardName_JPN = "";
        UkaNoMitamaToken.CardName_ENG = "Uka-no-Mitama";
        UkaNoMitamaToken.Form_JPN = new List<string>();
        UkaNoMitamaToken.Form_ENG = new List<string>();
        UkaNoMitamaToken.Attribute_JPN = new List<string>();
        UkaNoMitamaToken.Attribute_ENG = new List<string>();
        UkaNoMitamaToken.Type_JPN = new List<string>();
        UkaNoMitamaToken.Type_ENG = new List<string>();
        UkaNoMitamaToken.CardSpriteName = "EX8-037-token";
        UkaNoMitamaToken.cardKind = new List<CardKind> { CardKind.Digimon };
        UkaNoMitamaToken.DP = 9000;
        UkaNoMitamaToken.CardEffectClassName = "EX8_037_token";

        await UkaNoMitamaToken.GetCardSprite();

        WarGrowlmonToken = ScriptableObject.CreateInstance<CEntity_Base>();
        WarGrowlmonToken.cardColors = new List<CardColor>() { CardColor.Red };
        WarGrowlmonToken.PlayCost = -1;
        WarGrowlmonToken.Level = 0;
        WarGrowlmonToken.CardName_JPN = "";
        WarGrowlmonToken.CardName_ENG = "WarGrowlmon";
        WarGrowlmonToken.Form_JPN = new List<string>();
        WarGrowlmonToken.Form_ENG = new List<string>();
        WarGrowlmonToken.Attribute_JPN = new List<string>();
        WarGrowlmonToken.Attribute_ENG = new List<string>();
        WarGrowlmonToken.Type_JPN = new List<string>();
        WarGrowlmonToken.Type_ENG = new List<string>();
        WarGrowlmonToken.CardSpriteName = "BT19-091-token-red";
        WarGrowlmonToken.cardKind = new List<CardKind> { CardKind.Digimon };
        WarGrowlmonToken.DP = 6000;

        await WarGrowlmonToken.GetCardSprite();

        TaomonToken = ScriptableObject.CreateInstance<CEntity_Base>();
        TaomonToken.cardColors = new List<CardColor>() { CardColor.Yellow };
        TaomonToken.PlayCost = -1;
        TaomonToken.Level = 0;
        TaomonToken.CardName_JPN = "";
        TaomonToken.CardName_ENG = "Taomon";
        TaomonToken.Form_JPN = new List<string>();
        TaomonToken.Form_ENG = new List<string>();
        TaomonToken.Attribute_JPN = new List<string>();
        TaomonToken.Attribute_ENG = new List<string>();
        TaomonToken.Type_JPN = new List<string>();
        TaomonToken.Type_ENG = new List<string>();
        TaomonToken.CardSpriteName = "BT19-091-token-yellow";
        TaomonToken.cardKind = new List<CardKind> { CardKind.Digimon };
        TaomonToken.DP = 6000;

        await TaomonToken.GetCardSprite();
        
        RapidmonToken = ScriptableObject.CreateInstance<CEntity_Base>();
        RapidmonToken.cardColors = new List<CardColor>() { CardColor.Green };
        RapidmonToken.PlayCost = -1;
        RapidmonToken.Level = 0;
        RapidmonToken.CardName_JPN = "";
        RapidmonToken.CardName_ENG = "Rapidmon";
        RapidmonToken.Form_JPN = new List<string>();
        RapidmonToken.Form_ENG = new List<string>();
        RapidmonToken.Attribute_JPN = new List<string>();
        RapidmonToken.Attribute_ENG = new List<string>();
        RapidmonToken.Type_JPN = new List<string>();
        RapidmonToken.Type_ENG = new List<string>();
        RapidmonToken.CardSpriteName = "BT19-091-token-green";
        RapidmonToken.cardKind = new List<CardKind> { CardKind.Digimon };
        RapidmonToken.DP = 6000;

        await RapidmonToken.GetCardSprite();

        PipeFoxToken = ScriptableObject.CreateInstance<CEntity_Base>();
        PipeFoxToken.cardColors = new List<CardColor>() { CardColor.Yellow };
        PipeFoxToken.PlayCost = -1;
        PipeFoxToken.Level = 0;
        PipeFoxToken.CardName_JPN = "";
        PipeFoxToken.CardName_ENG = "Pipe-Fox";
        PipeFoxToken.Form_JPN = new List<string>();
        PipeFoxToken.Form_ENG = new List<string>();
        PipeFoxToken.Attribute_JPN = new List<string>();
        PipeFoxToken.Attribute_ENG = new List<string>();
        PipeFoxToken.Type_JPN = new List<string>();
        PipeFoxToken.Type_ENG = new List<string>();
        PipeFoxToken.CardSpriteName = "BT19-040-token";
        PipeFoxToken.cardKind = new List<CardKind> { CardKind.Digimon };
        PipeFoxToken.DP = 6000;
        PipeFoxToken.CardEffectClassName = "BT19_040_token";

        await PipeFoxToken.GetCardSprite();

        AthoRenePorToken = ScriptableObject.CreateInstance<CEntity_Base>();
        AthoRenePorToken.cardColors = new List<CardColor>() { CardColor.White };
        AthoRenePorToken.PlayCost = -1;
        AthoRenePorToken.Level = 0;
        AthoRenePorToken.CardName_JPN = "";
        AthoRenePorToken.CardName_ENG = "Atho, René & Por";
        AthoRenePorToken.Form_JPN = new List<string>();
        AthoRenePorToken.Form_ENG = new List<string>();
        AthoRenePorToken.Attribute_JPN = new List<string>();
        AthoRenePorToken.Attribute_ENG = new List<string>();
        AthoRenePorToken.Type_JPN = new List<string>();
        AthoRenePorToken.Type_ENG = new List<string>();
        AthoRenePorToken.CardSpriteName = "BT20-017-token";
        AthoRenePorToken.cardKind = new List<CardKind> { CardKind.Digimon };
        AthoRenePorToken.DP = 6000;
        AthoRenePorToken.CardEffectClassName = "BT20_017_token";

        await AthoRenePorToken.GetCardSprite();

        HinukamuyToken = ScriptableObject.CreateInstance<CEntity_Base>();
        HinukamuyToken.cardColors = new List<CardColor>() { CardColor.White };
        HinukamuyToken.PlayCost = -1;
        HinukamuyToken.Level = 0;
        HinukamuyToken.CardName_JPN = "";
        HinukamuyToken.CardName_ENG = "HinukamuyToken";
        HinukamuyToken.Form_JPN = new List<string>();
        HinukamuyToken.Form_ENG = new List<string>();
        HinukamuyToken.Attribute_JPN = new List<string>();
        HinukamuyToken.Attribute_ENG = new List<string>();
        HinukamuyToken.Type_JPN = new List<string>();
        HinukamuyToken.Type_ENG = new List<string>();
        HinukamuyToken.CardSpriteName = "BT23-057-token";
        HinukamuyToken.cardKind = new List<CardKind> { CardKind.Digimon };
        HinukamuyToken.DP = 6000;
        HinukamuyToken.CardEffectClassName = "BT23_057_token";

        await HinukamuyToken.GetCardSprite();

        PetrificationToken = ScriptableObject.CreateInstance<CEntity_Base>();
        PetrificationToken.cardColors = new List<CardColor>() { CardColor.White };
        PetrificationToken.PlayCost = -1;
        PetrificationToken.Level = 0;
        PetrificationToken.CardName_JPN = "";
        PetrificationToken.CardName_ENG = "Petrification";
        PetrificationToken.Form_JPN = new List<string>();
        PetrificationToken.Form_ENG = new List<string>();
        PetrificationToken.Attribute_JPN = new List<string>();
        PetrificationToken.Attribute_ENG = new List<string>();
        PetrificationToken.Type_JPN = new List<string>();
        PetrificationToken.Type_ENG = new List<string>();
        PetrificationToken.CardSpriteName = "BT21-029-token";
        PetrificationToken.cardKind = new List<CardKind> { CardKind.Digimon };
        PetrificationToken.DP = 3000;
        PetrificationToken.CardEffectClassName = "BT21_029_token";

        await PetrificationToken.GetCardSprite();

        PaishuToken = ScriptableObject.CreateInstance<CEntity_Base>();
        PaishuToken.cardColors = new List<CardColor>() { CardColor.Yellow };
        PaishuToken.PlayCost = -1;
        PaishuToken.Level = 0;
        PaishuToken.CardName_JPN = "";
        PaishuToken.CardName_ENG = "Paishu";
        PaishuToken.Form_JPN = new List<string>();
        PaishuToken.Form_ENG = new List<string>();
        PaishuToken.Attribute_JPN = new List<string>();
        PaishuToken.Attribute_ENG = new List<string>();
        PaishuToken.Type_JPN = new List<string>();
        PaishuToken.Type_ENG = new List<string>();
        PaishuToken.CardSpriteName = "EX12-057-token";
        PaishuToken.cardKind = new List<CardKind> { CardKind.Digimon };
        PaishuToken.DP = 6000;
        PaishuToken.CardEffectClassName = "EX12_057_token";

        await PaishuToken.GetCardSprite();

        KotenkenToken = ScriptableObject.CreateInstance<CEntity_Base>();
        KotenkenToken.cardColors = new List<CardColor>() { CardColor.Black };
        KotenkenToken.PlayCost = -1;
        KotenkenToken.Level = 0;
        KotenkenToken.CardName_JPN = "";
        KotenkenToken.CardName_ENG = "Kotenken";
        KotenkenToken.Form_JPN = new List<string>();
        KotenkenToken.Form_ENG = new List<string>();
        KotenkenToken.Attribute_JPN = new List<string>();
        KotenkenToken.Attribute_ENG = new List<string>();
        KotenkenToken.Type_JPN = new List<string>();
        KotenkenToken.Type_ENG = new List<string>();
        KotenkenToken.CardSpriteName = "EX12-034-token";
        KotenkenToken.cardKind = new List<CardKind> { CardKind.Digimon };
        KotenkenToken.DP = 9000;
        KotenkenToken.CardEffectClassName = "EX12_034_token";

        await KotenkenToken.GetCardSprite();
    }

    public static ContinuousController instance = null;

    private void Awake()
    {
        instance = this;
    }

    public async void Init()
    {
        Application.targetFrameRate = 60;
        long random = RandomUtility.GetSecureRandom();
        GameRandom.Seed(random);
        Debug.Log($"Game Initialize - random number sequence initialization, GameRandom.Seed:{random}");

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

        await LoadBanListOnline();

        // deck data
        //DeckDatas = PlayerPrefsUtil.LoadList<DeckData>(DeckDatasPlayerPrefsKey);
        LoadDeckLists();
        GetComponent<StarterDeck>().SetStarterDecks();

        // player data
        LoadPlayerName();
        LoadWinCount();

        // === DCGO-CUSTOM:friends begin ===
        RankedServices.EnsureExists();
        FriendServices.EnsureExists();
        OnlinePlayerCountService.EnsureExists();
        // === DCGO-CUSTOM:friends end ===

        // game play
        LoadAutoEffectOrder();
        LoadAutoDeckBottomOrder();
        LoadAutoDeckTopOrder();
        LoadAutoMinDigivolutionCost();
        LoadAutoMaxCardCount();
        LoadAutoHatch();
        //LoadUseBanlist();
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

            string deck = deckList[deckList.IndexOf("//")..];
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
            if (cEntity_Base.cardKind.Contains(CardKind.DigiEgg))
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

    #region Use Banlist
    public bool useBanlist = true;
    string _useBanlistKey = "UseBanlist";

    public void SaveUseBanlist()
    {
        PlayerPrefsUtil.SetBool(_useBanlistKey, useBanlist);
        PlayerPrefs.Save();
    }
    public void LoadUseBanlist()
    {
        useBanlist = PlayerPrefsUtil.GetBool(_useBanlistKey, false);
    }
    #endregion

    #region Show CutIn Animation
    public bool showCutInAnimation = false;
    string _showCutInAnimationKey = "ShowCutInAnimation";

    public void SaveShowCutInAnimation()
    {
        PlayerPrefsUtil.SetBool(_showCutInAnimationKey, showCutInAnimation);
        PlayerPrefs.Save();
    }
    public void LoadShowCutInAnimation()
    {
        showCutInAnimation = PlayerPrefsUtil.GetBool(_showCutInAnimationKey, false);
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
        //serverRegion = PlayerPrefs.GetString(_serverRegionKey, "us");
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
        // === DCGO-CUSTOM:friends begin ===
        FriendServices.Instance?.Director?.CancelAutoAdvanceFromResult();
        // === DCGO-CUSTOM:friends end ===
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
        // === DCGO-CUSTOM:friends begin ===
        bool wasFriendDuel = isFriendDuel || FriendKeys.IsInFriendDuelRoom();
        if (wasFriendDuel)
        {
            isFriendDuel = true;
            Opening.instance.battle?.roomManager?.Off();
        }

        var friendDirector = wasFriendDuel ? FriendServices.EnsureExists().Director : null;
        bool friendNextGame = wasFriendDuel && friendDirector != null && friendDirector.ShouldReloadNextGame;
        // === DCGO-CUSTOM:friends end ===

        long random = RandomUtility.GetSecureRandom();
        GameRandom.Seed(random);
        Debug.Log($"random number sequence initialization, GameRandom.Seed:{random}");

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

        if (wasFriendDuel || FriendKeys.IsInFriendDuelRoom())
        {
            // === DCGO-CUSTOM:friends begin ===
            wasFriendDuel = true;
            isFriendDuel = true;
            if (friendDirector == null)
            {
                friendDirector = FriendServices.EnsureExists().Director;
            }

            friendNextGame = friendDirector != null && friendDirector.ShouldReloadNextGame;
            Debug.Log(friendNextGame
                ? "Unload from Friend Duel (next game)"
                : "Unload from Friend Duel (series end)");
            if (friendNextGame)
            {
                _endBattle = false;
                yield return friendDirector.StartNextGameCoroutine();
                yield break;
            }

            if (PhotonNetwork.InRoom)
            {
                Hashtable endSeriesProp = PhotonNetwork.LocalPlayer.CustomProperties ?? new Hashtable();
                endSeriesProp["isBattle"] = false;
                PhotonNetwork.LocalPlayer.SetCustomProperties(endSeriesProp);
            }

            Scene openingScene = SceneManager.GetSceneByName("Opening");
            if (openingScene.IsValid())
            {
                SceneManager.SetActiveScene(openingScene);
            }

            _endBattle = false;
            yield return friendDirector.EndSeriesToHomeCoroutine();
            Opening.instance.LoadingObject.gameObject.SetActive(false);
            yield return StartCoroutine(Opening.instance.LoadingObject_Unload.EndLoading());
            yield break;
            // === DCGO-CUSTOM:friends end ===
        }
        else if (isRandomMatch)
        {
            Debug.Log("Unload from Random Match");
            yield return StartCoroutine(Opening.instance.battle.lobbyManager_RandomMatch.CloseLobbyCoroutine());
            yield return StartCoroutine(Opening.instance.battle.selectBattleMode.SetUpSelectBattleModeCoroutine());
        }

        else
        {
            Debug.Log("Unload from Room Match");
            yield return StartCoroutine(Opening.instance.battle.roomManager.Init(true));
            yield return _waitForSeconds0_1;
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
            yield return _waitForSeconds0_1;

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
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _showPhotonDebug = !_showPhotonDebug;
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
    public void SetRandom(long random)
    {
        StartCoroutine(SetRandomCoroutine(random));
    }

    IEnumerator SetRandomCoroutine(long random)
    {
        yield return new WaitWhile(() => !CanSetRandom);

        GameRandom.Seed(random);
        DoneSetRandom = true;

        Debug.Log($"random number sequence initialization, GameRandom.Seed:{random}");
    }

#if UNITY_EDITOR
    #region Photon Debug HUD
    bool _showPhotonDebug = false;
    Photon.Realtime.IConnectionCallbacks _connectionCallbacks;
    Photon.Realtime.IMatchmakingCallbacks _matchmakingCallbacks;
    Photon.Realtime.ILobbyCallbacks _lobbyCallbacks;

    void OnEnable()
    {
        var listener = new PhotonDebugListener();
        _connectionCallbacks = listener;
        _matchmakingCallbacks = listener;
        _lobbyCallbacks = listener;
        PhotonNetwork.AddCallbackTarget(listener);
    }

    void OnDisable()
    {
        if (_connectionCallbacks != null)
            PhotonNetwork.RemoveCallbackTarget(_connectionCallbacks);
    }

    class PhotonDebugListener :
        Photon.Realtime.IConnectionCallbacks,
        Photon.Realtime.IMatchmakingCallbacks,
        Photon.Realtime.ILobbyCallbacks
    {
        public void OnDisconnected(Photon.Realtime.DisconnectCause cause)
        {
            Debug.LogWarning($"[Photon Debug] Disconnected: {cause}");
        }

        public void OnConnected() { }
        public void OnConnectedToMaster()
        {
            Debug.Log("[Photon Debug] Connected to Master");
        }
        public void OnRegionListReceived(Photon.Realtime.RegionHandler handler) { }
        public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            Debug.LogError($"[Photon Debug] Auth failed: {debugMessage}");
        }

        public void OnJoinedLobby()
        {
            Debug.Log("[Photon Debug] Joined Lobby");
        }
        public void OnLeftLobby()
        {
            Debug.Log("[Photon Debug] Left Lobby");
        }
        public void OnLobbyStatisticsUpdate(List<Photon.Realtime.TypedLobbyInfo> lobbyStatistics) { }
        public void OnRoomListUpdate(List<Photon.Realtime.RoomInfo> roomList) { }

        public void OnJoinedRoom()
        {
            Debug.Log($"[Photon Debug] Joined Room: {PhotonNetwork.CurrentRoom?.Name}");
        }
        public void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[Photon Debug] Join Room FAILED: [{returnCode}] {message}");
        }
        public void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.LogError($"[Photon Debug] Join Random FAILED: [{returnCode}] {message}");
        }
        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[Photon Debug] Create Room FAILED: [{returnCode}] {message}");
        }
        public void OnCreatedRoom()
        {
            Debug.Log("[Photon Debug] Created Room");
        }
        public void OnLeftRoom()
        {
            Debug.Log("[Photon Debug] Left Room");
        }
        public void OnFriendListUpdate(List<Photon.Realtime.FriendInfo> friendList) { }
    }

    void OnGUI()
    {
        if (!string.IsNullOrEmpty(PhotonUtility.RetryStatus))
        {
            GUIStyle retryStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                richText = true,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            float boxW = 500;
            float boxH = 40;
            float x = (Screen.width - boxW) / 2;
            float y = (Screen.height - boxH) / 2;

            GUI.Box(new Rect(x - 10, y - 10, boxW + 20, boxH + 20), "");
            GUI.Label(new Rect(x, y, boxW, boxH), $"<color=#FFAA00>{PhotonUtility.RetryStatus}</color>", retryStyle);
        }

        if (!_showPhotonDebug) return;

        string status;

        if (!PhotonNetwork.IsConnected)
        {
            status = "<color=#FF4444>[Photon] Disconnected</color>";
        }
        else if (PhotonNetwork.InRoom)
        {
            string roomName = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : "?";
            int players = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
            status = $"<color=#44FF44>[Photon] Connected | Region: {PhotonNetwork.CloudRegion} | Room: {roomName} ({players} players)</color>";
        }
        else if (PhotonNetwork.InLobby)
        {
            status = $"<color=#44FF44>[Photon] Connected | Region: {PhotonNetwork.CloudRegion} | In Lobby</color>";
        }
        else
        {
            status = $"<color=#FFAA00>[Photon] Connected | Region: {PhotonNetwork.CloudRegion} | Not in Lobby/Room</color>";
        }

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            richText = true,
            fontStyle = FontStyle.Bold
        };

        GUI.Label(new Rect(10, 10, 800, 30), status, style);
    }
    #endregion
#endif
}

#region Manage random numbers
public static class RandomUtility
{
    /// <summary>
    /// Generates a cryptographically secure 64-bit random seed.
    /// Uses OS entropy pool via System.Security.Cryptography.
    /// </summary>
    public static long GetSecureRandom()
    {
        byte[] bytes = new byte[8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return BitConverter.ToInt64(bytes, 0);
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

        float random = GameRandom.Range(0f, 1f);

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

        // Fisher-Yates shuffle using GameRandom (Xoshiro256**)
        int n = CardDatas.Count;

        while (n > 0)
        {
            n--;

            // Random index from 0 to n (inclusive) — Range takes exclusive max
            int k = GameRandom.Range(0, n + 1);

            // Swap elements at indices n and k
            (CardDatas[k], CardDatas[n]) = (CardDatas[n], CardDatas[k]);
        }


        return CardDatas;
    }

    public static List<CardSource> ShuffledDeckCards(List<CardSource> DeckCards)
    {
        List<CardSource> CardDatas = new List<CardSource>();
        CardDatas.AddRange(DeckCards);

        // Fisher-Yates shuffle using GameRandom (Xoshiro256**)
        int n = CardDatas.Count;

        while (n > 0)
        {
            n--;

            // Random index from 0 to n (inclusive) — Range takes exclusive max
            int k = GameRandom.Range(0, n + 1);

            // Swap elements at indices n and k
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
    public static string RetryStatus { get; set; } = null;

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
        int maxRetries = 5;
        float retryDelay = 3f;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
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

            yield return new WaitUntil(() =>
                PhotonNetwork.IsConnectedAndReady ||
                PhotonNetwork.NetworkingClient.State == Photon.Realtime.ClientState.Disconnected);

            if (PhotonNetwork.IsConnectedAndReady)
            {
                RetryStatus = null;
                yield break;
            }

            var cause = PhotonNetwork.NetworkingClient.DisconnectedCause;
            Debug.LogWarning($"[Photon] Connection failed: {cause} (attempt {attempt + 1}/{maxRetries + 1})");

            if (cause == Photon.Realtime.DisconnectCause.MaxCcuReached && attempt < maxRetries)
            {
                RetryStatus = LocalizeUtility.GetLocalizedString(
                    EngMessage: $"Server full. Retrying... ({attempt + 1}/{maxRetries})",
                    JpnMessage: $"サーバーが満員です。再接続中... ({attempt + 1}/{maxRetries})"
                );
                Debug.Log($"[Photon] Server full, retrying in {retryDelay}s...");
                yield return new WaitForSeconds(retryDelay);
                retryDelay += 2f;
                continue;
            }

            RetryStatus = LocalizeUtility.GetLocalizedString(
                EngMessage: "Connection failed. Please try again later.",
                JpnMessage: "接続に失敗しました。後でもう一度お試しください。"
            );
            Debug.LogError($"[Photon] Connection failed permanently: {cause}");
            yield break;
        }
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

        if (hash.TryGetValue(ContinuousController.WinCountKey, out object value))
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

        if (hash.TryGetValue(ContinuousController.PlayerNameKey, out object value))
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

    // === DCGO-CUSTOM:ranked begin ===
    #region Ranked player properties (PlayFabId + MMR)
    public static IEnumerator SetRankedPlayerProperties()
    {
        var ranked = RankedServices.EnsureExists();
        if (ranked.Auth == null || !ranked.Auth.IsLoggedIn)
        {
            yield break;
        }

        int mmr = ranked.Profile.Cached?.mmr ?? RankedRating.DefaultMmr;
        string playFabId = ranked.Auth.PlayFabId;

        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

        if (hash.ContainsKey(RankedKeys.MmrProperty))
        {
            hash[RankedKeys.MmrProperty] = mmr;
        }
        else
        {
            hash.Add(RankedKeys.MmrProperty, mmr);
        }

        if (hash.ContainsKey(RankedKeys.PlayFabIdProperty))
        {
            hash[RankedKeys.PlayFabIdProperty] = playFabId;
        }
        else
        {
            hash.Add(RankedKeys.PlayFabIdProperty, playFabId);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        while (true)
        {
            Hashtable _hash = PhotonNetwork.LocalPlayer.CustomProperties;
            bool mmrOk = _hash.TryGetValue(RankedKeys.MmrProperty, out object mmrVal) && Convert.ToInt32(mmrVal) == mmr;
            bool idOk = _hash.TryGetValue(RankedKeys.PlayFabIdProperty, out object idVal) && (string)idVal == playFabId;
            if (mmrOk && idOk)
            {
                break;
            }

            yield return null;
        }
    }
    #endregion
    // === DCGO-CUSTOM:ranked end ===
}
#endregion

public enum Language
{
    ENG,
    JPN,
}
