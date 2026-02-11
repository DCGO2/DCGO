using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Drawing.Text;
using System.Net;
using System.Data;

namespace DCGO.CardEntities
{
    [CustomEditor(typeof(LoadJSON_CardEntity))]
    public class JSONLoader_CardEntity : Editor
    {
        LoadJSON_CardEntity _loadJSON;
        //string baseURL = "https://raw.githubusercontent.com/TakaOtaku/Digimon-Cards/main/src/";
        string baseURL = "https://raw.githubusercontent.com/TakaOtaku/Digimon-Card-App/main/src/";

        public List<CardData> _cardData;

        string cardIDString = "";
        bool onlyAA = false;
        bool updateExisting = false;
        bool debugMode = false;
        bool withoutImages = false;

        public override void OnInspectorGUI()
        {
            _loadJSON = target as LoadJSON_CardEntity;

            GUILayout.Label("**IMPORTANT: DO NOT EDIT**");

            DrawDefaultInspector();

            if (GUILayout.Button("Get JSON Object"))
                EditorCoroutineUtility.StartCoroutine(GetJsonData(_loadJSON), this);

            if (_cardData != null)
            {
                updateExisting = GUILayout.Toggle(updateExisting, "Update Existing Assets");
                onlyAA = GUILayout.Toggle(onlyAA, "AA Only");
                debugMode = GUILayout.Toggle(debugMode, "Run in Debug Mode");
                withoutImages = GUILayout.Toggle(withoutImages, "Create Assets Without Images");

                GUILayout.Label("Card ID: ");
                cardIDString = GUILayout.TextField(cardIDString, 500).ToUpper();

                if (cardIDString != "")
                {
                    if (GUILayout.Button("Create Scriptable Object"))
                    {
                        List<string> list = cardIDString.Split(",").ToList();
                        List<CardData> cards = new List<CardData>();

                        foreach (string str in list)
                            cards.AddRange(_cardData.Where(x => x.cardNumber.Contains(str)).ToList());

                        SetDataToScriptableObject(cards);

                        if(!debugMode)
                            _loadJSON.prevCardIndex = _loadJSON.setCardIndex-1;
                    }
                }
            }
        }

        IEnumerator GetJsonData(LoadJSON_CardEntity loadJSON)
        {
            string url = baseURL + "assets/cardlists/DigimonCards.json";
            UnityWebRequest jsonWebRequest = UnityWebRequest.Get(url);

            yield return jsonWebRequest.SendWebRequest();

            if (jsonWebRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(jsonWebRequest.error);
            }
            else
            {
               
                RootObject root = JsonUtility.FromJson<RootObject>("{\"cards\":" + jsonWebRequest.downloadHandler.text + "}");
                _cardData = root.cards;
            }

            yield return null;
        }

        void SetDataToScriptableObject(List<CardData> data)
        {
            foreach (CardData card in data)
            {
                if(!onlyAA)
                    CreateScriptableObject(card, card.cardNumber);
                
                CreateAA(card);
            }
        }

        string GetImageURL(string url, string ID, List<AlternateArt> AA)
        {
            string[] urlSplit = url.Split(".");
            string startURL = urlSplit[0].Substring(0, urlSplit[0].LastIndexOf("/"));

            if (debugMode)
                Debug.Log($"{ID} - {AA.Count}");

            if (!ID.Contains("Errata") && !ID.Contains("_") && AA.Count > 0)
            {
                AlternateArt errata = AA.Where(x => x.id.StartsWith(ID) && x.id.Contains("Errata")).FirstOrDefault();

                if (errata != null)
                    ID = errata.id;
            }

            return $"{ID}";
        }


        void CreateScriptableObject(CardData card, string imageID)
        {
            Debug.Log($"creating: {card.name.english} - {card.cardType}");
            // CardEntity‚ instance created
            CEntity_Base cardEntity = CreateInstance<CEntity_Base>();

            cardEntity.cardColors = GetCardColors(card.color);

            cardEntity.PlayCost = intParse(card.playCost);
            cardEntity.EvoCosts = GetEvoCosts(card.digivolveCondition,cardEntity.cardColors);

            cardEntity.Level = levelParse(card.cardLv);
            cardEntity.CardName_ENG = card.name.english;

            cardEntity.Form_ENG = GetCardInfo(card.form);
            cardEntity.Attribute_ENG = GetCardInfo(card.attribute);
            cardEntity.Type_ENG = GetCardInfo(card.type, card.rule);

            cardEntity.CardSpriteName = GetImageURL(card.cardImage, imageID, card.AAs);

            cardEntity.cardKind = DictionaryUtility.GetCardKind(card.cardType.Replace("-",""), DataBase.CardKindENNameDictionary);

            cardEntity.EffectDiscription_ENG = GetEffectDescription(card);
            cardEntity.InheritedEffectDiscription_ENG = card.digivolveEffect;
            cardEntity.SecurityEffectDiscription_ENG = card.securityEffect;

            cardEntity.CardEffectClassName = FixCharactersInClassName($"{card.cardNumber}");

            cardEntity.DP = intParse(card.dp);
            cardEntity.rarity = card.rarity.Equals("-") ? Rarity.None : (Rarity)Enum.Parse(typeof(Rarity), card.rarity);
            cardEntity.OverflowMemory = GetOverflowMemory(card.aceEffect);
            cardEntity.CardID = card.id;
            cardEntity.MaxCountInDeck = GetMaxCount(card.restrictions.japanese, card.rule);

            cardEntity.LinkDP = dpParse(card.linkDP);
            cardEntity.LinkEffect = card.linkEffect;
            cardEntity.LinkRequirement = card.linkRequirement;

            cardEntity.name = cardEntity.CardSpriteName.Replace("-Errata","").Replace("-","_");

            if (cardEntity.cardKind == CardKind.DigiEgg && cardEntity.PlayCost == 0)
                cardEntity.PlayCost = -1;

            if (!debugMode)
            {
                if(withoutImages || GetImage(cardEntity.CardSpriteName))
                {
                    Debug.Log($"created: {cardEntity.name}: {cardEntity.CardSpriteName}, {cardEntity.CardEffectClassName}");
                    SaveScriptableObject(cardEntity);
                }
            }
            else
                Debug.Log($"DATA {cardEntity.name}: {cardEntity.CardSpriteName}, {cardEntity.CardEffectClassName}, {GetCardIndex(cardEntity)}");
        }

        void CreateAA(CardData data)
        {
            foreach (AlternateArt AA in data.AAs)
            {
                if (!AA.id.Contains("_"))
                    continue;

                CreateScriptableObject(data, AA.id);
            }
        }

        #region Save Scriptable Object
        void SaveScriptableObject(CEntity_Base entity)
        {
            string fileName = $"{entity.name}.asset";

            string folderName_SetID = $"{entity.SetID}";
            string folderName_CardColor = entity.cardColors.Count > 0 ? $"{DataBase.CardColorNameDictionary[entity.cardColors[0]]}" : "Unknown";
            folderName_CardColor = char.ToUpper(folderName_CardColor[0]) + folderName_CardColor.Substring(1);

            string folderName_CardKind = $"{DataBase.CardKindENNameDictionary[entity.cardKind]}";
            string folderPath = $"Assets/CardBaseEntity/{folderName_SetID}/{folderName_CardColor}/{folderName_CardKind}";
            string filePath = $"{folderPath}/{fileName}".Trim().Replace("\t", "").Replace("\n", "").Replace("\r", "").Replace(" ", "");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if(File.Exists(filePath))
            {
                if (updateExisting)
                {
                    CEntity_Base asset = (CEntity_Base)AssetDatabase.LoadAssetAtPath(filePath, typeof(CEntity_Base));

                    entity.CardIndex = asset.CardIndex;

                    EditorUtility.CopySerialized(entity, asset);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"{entity.name}: Scriptable Object Updated");
                }
            }
            else
            {
                entity.CardIndex = GetCardIndex(entity);
                AssetDatabase.CreateAsset(entity, filePath);
                Debug.Log($"{entity.name}: Scriptable Object Created");
            }

            AssetDatabase.Refresh();
        }
        #endregion

        #region Parsing Methods

        //Get Card Index
        int GetCardIndex(CEntity_Base entity)
        {
            int index = _loadJSON.setCardIndex;

            /*f (entity.SetID.Equals("P"))
            {
                index = _loadJSON.promoCardIndex;
                _loadJSON.promoCardIndex++;
            }
            else*/
            _loadJSON.setCardIndex++;

            while (CheckIndexIsUsed())
            {
                _loadJSON.setCardIndex++;
            }

            if (!debugMode)
                EditorUtility.SetDirty(_loadJSON);

            return index;
        }

        bool CheckIndexIsUsed()
        {
            return GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/")
                .Any(x => x.CardIndex == _loadJSON.setCardIndex);
        }

        //Parse ScriptableObject Name
        string FixCharactersInName(string str)
        {
            string name = str;

            name = name
                .Replace(" ", "_")
                .Replace(":","")
                .Replace("?","")
                .Replace("!","")
                .Replace("<", "")
                .Replace(">", "");

            return name;
        }

        //Parse ScriptableObject Class Name
        public string FixCharactersInClassName(string str)
        {
            string name = str;

            name = FixCharactersInName(name);

            name = name
                .Replace("-", "_")
                .Replace(".", "")
                .Replace("'", "")
                .Replace("&", "And")
                .Replace("(", "")
                .Replace(")", "");

            return name;
        }

        //Parse effect description
        string GetEffectDescription(CardData card)
        {
            string effect = "-";
            List<string> effects = new List<string>();

            if (card.specialDigivolve != "-")
                effects.Add($"{card.specialDigivolve}");

            if(card.effect != "-")
                effects.Add($"{card.effect}");

            if(card.digiXros != "-")
                effects.Add($"{card.digiXros}");

            if (effects.Count > 0)
            {
                effect = "";

                string lastEffect = effects.Last();
                foreach (string e in effects)
                {
                    effect += $"{e}";

                    if (!e.Equals(lastEffect))
                        effect += "\n\n";
                }
                
            }

            return effect;
        }
        //Parse card info split by "/" to list
        List<string> GetCardInfo(string str, string rule = "")
        {
            List<string> strings = new List<string>();

            foreach (string data in str.Split("/"))
            {
                strings.Add(data.Replace("\n","").Replace("\r", "").Replace("(App Name)", "").Trim());
            }

            if (!String.IsNullOrEmpty(rule))
                strings.AddRange(GetRuleTrait(rule));

            string debugTxt = "";
            foreach (string data in strings)
                debugTxt += data + "\n";

            return strings;
        }

        //Parse RULE text
        List<string> GetRuleTrait(string str)
        {
            List<string> strings = new List<string>();

            if (!str.Contains("[Rule] Trait:"))
                return new List<string>();

            string traitRule = str.Split("[Rule] Trait:")[1];
            int startIndex = traitRule.IndexOf("[");
            traitRule = traitRule.Substring(startIndex, traitRule.LastIndexOf("]") - startIndex)
                .Replace("[","")
                .Replace("]", "");
       
            foreach (string data in traitRule.Split("/"))
                strings.Add(data.Trim().Replace("\n", "").Replace("\r", ""));

            return strings;
        }

        //[Rule] You can include up to 50 copies of cards with this card's card number in your deck.
        int GetRuleMaxCount(string str)
        {
            int totalCount = 4;

            if (!str.Contains("[Rule] You can include up to"))
                return totalCount;

            string splitString = str.Split("[Rule] You can include up to")[1];
            string valueString = splitString.Replace("copies of cards with this card's card number in your deck.", "");
            int value = int.Parse(valueString.Trim());

            return value;
        }

        //Parse digivolve conditions to list
        List<EvoCost> GetEvoCosts(List<DigivolveCondition> digivolveConditions, List<CardColor> cardColors)
        {
            List<EvoCost> evoCosts = new List<EvoCost>();

            foreach (DigivolveCondition digivolveCondition in digivolveConditions)
            {
                if (digivolveCondition.color.ToLower() == "all")
                {
                    if (int.TryParse(digivolveCondition.level, out _))
                    {
                        foreach (CardColor color in DataBase.cardColors)
                        {
                            if (color == CardColor.White || color == CardColor.None)
                                continue;

                            EvoCost evoCost = new EvoCost();

                            evoCost.CardColor = color;

                            evoCost.Level = int.Parse(digivolveCondition.level);
                            evoCost.MemoryCost = int.Parse(digivolveCondition.cost);

                            evoCosts.Add(evoCost);
                        }
                    }
                }
                else
                {
                    EvoCost evoCost = new EvoCost();

                    if (digivolveCondition.color.ToLower() == "")
                        evoCost.CardColor = cardColors[0];
                    else
                        evoCost.CardColor = DictionaryUtility.GetCardColor(digivolveCondition.color.ToLower(), DataBase.CardColorNameDictionary);

                    evoCost.MemoryCost = int.Parse(digivolveCondition.cost);

                    if (digivolveCondition.level != "Tamer")
                    {
                        evoCost.Level = int.Parse(digivolveCondition.level);
                        evoCosts.Add(evoCost);
                    }
                }
            }

            return evoCosts;
        }

        //Parse card colors to list
        List<CardColor> GetCardColors(string colors)
        {
            List<CardColor> cardColors = new List<CardColor>();

            foreach (string cardColorName in colors.Split("/"))
            {
                foreach (string cardColorNameValues in DataBase.CardColorNameDictionary.Values)
                {
                    if (cardColorName.ToLower().Trim() == cardColorNameValues)
                    {
                        cardColors.Add(DictionaryUtility.GetCardColor(cardColorName.ToLower().Trim(), DataBase.CardColorNameDictionary));
                    }
                }
            }

            return cardColors;
        }

        //Parse overflow memory
        int GetOverflowMemory(string aceEffect)
        {
            int value = 0;

            if(aceEffect != "-")
            {
                int startIndex = aceEffect.IndexOf("\uff1c") + 1;
                int endIndex = aceEffect.IndexOf("\uff1e") - startIndex;
                string overflowMemory = aceEffect.Substring(startIndex, endIndex);

                if (int.TryParse(overflowMemory, out value))
                    return Math.Abs(value);
            }

            return value;
        }

        //Parse Max count in deck
        int GetMaxCount(string restriction, string rule)
        {
            int value = 4;

            if (!String.IsNullOrEmpty(rule))
                value = GetRuleMaxCount(rule);

            if (restriction == "Banned")
                value = 0;

            if(restriction.StartsWith("Restricted"))
                value = int.Parse(restriction.Substring(restriction.Length-1));

            return value;
        }

        //Parse Link Cost
        int GetLinkCost(string requirement)
        {
            int value = 0;

            if (requirement.Equals("-"))
                return value;

            string costString = requirement.Split("Cost")[1].Trim();

            if (int.TryParse(costString, out value))
                return value;

            return value;
        }

        int intParse(string str)
        {
            int value = 0;

            if (int.TryParse(str, out value))
                return value;

            return value;
        }

        int dpParse(string str)
        {
            int value = 0;

            if (str.Equals("-"))
                return value;

            if (int.TryParse(str.Substring(1, str.Length - 4), out value))
                return value;

            if (str.StartsWith("-"))
                value *= -1;

            return value;
        }

        int levelParse(string str)
        {
            int value = 0;

            if (str.Contains("Lv") && int.TryParse(str.Substring(3, 1), out value))
                return value;

            return value;
        }
        #endregion

        #region Attempt to load Image
        bool GetImage(string ID)
        {
            string imgURL = $"{baseURL}assets/images/cards/{ID}.webp";

            Debug.Log(imgURL);
            try
            {
                WebClient wc = new WebClient();
                string HTMLSource = wc.DownloadString(imgURL);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
                return false;
            }
        }
        #endregion
    }
}