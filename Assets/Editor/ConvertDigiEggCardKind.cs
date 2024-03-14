using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConvertDigiEggCardKind : MonoBehaviour
{
    [MenuItem("Window/Attach/ConvertDigiEggCardKind")]
    static void Convert_DigiEggCardKind()
    {
        List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");

        foreach (CEntity_Base cEntity_Base in List)
        {
            if (cEntity_Base.cardKind == CardKind.Digimon)
            {
                if (cEntity_Base.Level == 2)
                {
                    cEntity_Base.cardKind = CardKind.DigiEgg;
                    EditorUtility.SetDirty(cEntity_Base);
                    AssetDatabase.SaveAssetIfDirty(cEntity_Base);
                }
            }
        }
    }
}