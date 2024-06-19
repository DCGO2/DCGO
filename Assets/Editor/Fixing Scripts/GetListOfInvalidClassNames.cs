using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using DCGO.CardEntities;

namespace DCGO.Tools.Repair 
{
    public class GetListOfInvalidClassNames : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/Get List of Invalid Class Names")]
        static void FixEntityClassNames()
        {
            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");
            List<string> InvalidClassNames = new List<string>();

            foreach (CEntity_Base card in Entities)
            {
                if (CanAttachEffectComponent(card.SetID, card.CardEffectClassName))
                    continue;

                if (card.EffectDiscription_ENG.Equals("")
                    && card.InheritedEffectDiscription_ENG.Equals("")
                    && card.SecurityEffectDiscription_ENG.Equals(""))
                    continue;

                if (card.EffectDiscription_ENG.Equals("-") 
                    && card.InheritedEffectDiscription_ENG.Equals("-")
                    && card.SecurityEffectDiscription_ENG.Equals("-"))
                    continue;

                if(InvalidClassNames.Contains(card.CardEffectClassName))
                    continue;

                InvalidClassNames.Add(card.CardEffectClassName);
                Debug.LogError($"Invalid Class Name: {card.name} - {card.CardEffectClassName}");
            }

            Debug.Log($"Total Invalid Class Names: {InvalidClassNames.Count}");

            bool CanAttachEffectComponent(string ID, string ClassName)
            {
                if (string.IsNullOrEmpty(ClassName))
                    return false;

                if (Type.GetType(ClassName + ",Assembly-CSharp") == null)
                {
                    if (!ClassName.Contains("token"))
                    {
                        if (Type.GetType($"DCGO.CardEffects.{ID}.{ClassName},Assembly-CSharp") == null)
                            return false;
                    }
                    else
                    {
                        if (Type.GetType($"DCGO.CardEffects.Tokens.{ClassName},Assembly-CSharp") == null)
                            return false;
                    }
                }
                
                return true;
            }
        }
    }
}