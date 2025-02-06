using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace DCGO.Tools.Repair
{
    public class MatchClassNames : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/MatchClassNames")]
        static void FixEntityCardIndex()
        {
            Dictionary<string,string> classNameDictionary = new Dictionary<string,string>();
            string path = "Assets/CardBaseEntity/";

            if (Selection.assetGUIDs.Length != 0)
                path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            Debug.Log($"ASSET PATH: {path}");

            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>(path)
                .OrderBy(x => x.name.Substring(x.name.LastIndexOf("-")+1, 2)).ToList();

            string className = "";
            string editList = "Asset Name - Original Class - New Class \n";
            int foundCount = 0;

            //Locate All mismatched classNames
            foreach (CEntity_Base card in List)
            {
                className = "";

                if (String.IsNullOrEmpty(card.CardEffectClassName))
                {
                    Debug.Log($"Class Name Empty: {card.name}");
                    continue;
                }

                if (!card.CardEffectClassName.Contains(card.CardID.Replace("-", "_")))
                {
                    Debug.Log($"Using Alternate Card: {card.name}");
                    continue;
                }

                className = FixCharactersInClassName($"{card.CardID}");

                if (className == card.CardEffectClassName)
                {
                    Debug.Log($"Matches already: {card.name}");
                    continue;
                }

                editList += $"{card.name} - {card.CardEffectClassName} - {className}\n";

                if (!classNameDictionary.ContainsKey(className))
                    classNameDictionary.Add(className, card.CardEffectClassName);

                /*if (!card.name.Contains("_P"))
                {
                    if (String.IsNullOrEmpty(card.CardEffectClassName))
                        continue;

                    if (!card.CardEffectClassName.Contains(card.CardID.Replace("-", "_")))
                        continue;

                    className = card.CardEffectClassName;
                    continue;
                }
                

                if (!card.CardEffectClassName.Equals(className))
                {
                    if (!classNameDictionary.ContainsKey(className))
                        classNameDictionary.Add(className, card.CardEffectClassName);

                    //Debug.Log($"{card.name}: {card.CardIndex} - {card.CardEffectClassName} != {className}");
                }*/


                //card.CardEffectClassName = className;
            }



            /*foreach (string key in classNameDictionary.Keys)
            {
                List<CEntity_Base> filtered = List.Filter(x => x.CardEffectClassName == key).ToList();

                foreach (CEntity_Base card in filtered)
                {
                    
                    //card.CardEffectClassName = classNameDictionary[key];
                    //EditorUtility.SetDirty(card);
                }
                    

                Debug.Log($"Filtered Count: {filtered.Count}");
            }*/
            //Debug.Log($"{key} - {classNameDictionary[key]}");
            Debug.Log(editList);
            Debug.Log($"DONE: found {classNameDictionary.Keys.Count}");
            return;

            string FixCharactersInClassName(string str)
            {
                string name = str;

                name = FixCharactersInName(name);

                name = name
                    .Replace("-", "_")
                    .Replace(".", "")
                    .Replace("'", "")
                    .Replace(",","")
                    .Replace("&", "And")
                    .Replace("(", "")
                    .Replace(")", "");

                return name;
            }

            //Parse ScriptableObject Name
            string FixCharactersInName(string str)
            {
                string name = str;

                name = name
                    .Replace(" ", "")
                    .Replace(":", "")
                    .Replace("?", "")
                    .Replace("!", "")
                    .Replace("<", "")
                    .Replace(">", "");

                return name;
            }
        }
    }
}