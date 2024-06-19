using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using DCGO.CardEntities;

namespace DCGO.Tools.Repair{
    public class CleanUpClassName : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/Fix Entity Class Names")]
        static void FixEntityClassNames()
        {
            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");

            foreach (CEntity_Base card in List)
            {
                card.CardEffectClassName = FixCharactersInClassName(card.CardEffectClassName);
                EditorUtility.SetDirty(card);
            }
                

            Debug.Log("Fixed all class names in CardBaseEntity");
            return;

            //Parse ScriptableObject Class Name
            string FixCharactersInClassName(string str)
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

            //Parse ScriptableObject Name
            string FixCharactersInName(string str)
            {
                string name = str;

                name = name
                    .Replace(" ", "_")
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

