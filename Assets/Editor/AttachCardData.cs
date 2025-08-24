using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
public class AttachCardData : MonoBehaviour
{
    [MenuItem("Window/Attach/AttachCardData")]
    static void Attach_CardData()
    {
        List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");

        List<string> UnimplementedCardSpriteNames = new List<string>() { "P-147", "BT17-085_P3", "BT15-082_P2", "BT15-084_P2", "BT14-085_P2", "BT13-100_P2", "BT11-086_P2", "BT10-042_P5" };
        /*
         BT17_085 Rika Nonaka
         BT15_082 Sora Takenouchi
         BT15_084 Kari Kamiya
         BT14_085 Mimi Tachikawa
         BT13_100 Yoshino Fujieda
         BT11_086 Mervamon
         BT10_042
         */
        List<string> DebugCardSpriteNames = new List<string>() { };

        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.GetComponent<ContinuousController>() != null)
            {
                ContinuousController CCtrl = obj.GetComponent<ContinuousController>();

                CCtrl.CardList = new CEntity_Base[] { };
                CCtrl.SortedCardList = new CEntity_Base[] { };

                List<CEntity_Base> cEntity_Bases = new List<CEntity_Base>();

                foreach (CEntity_Base cEntity_Base in List)
                {
                    if (UnimplementedCardSpriteNames.Contains(cEntity_Base.CardSpriteName))
                    {
                        Debug.Log(cEntity_Base.CardID);
                        continue;
                    }

                    

                    if (String.IsNullOrEmpty(cEntity_Base.CardSpriteName))
                    {
                        continue;
                    }

                    if (DebugCardSpriteNames.Contains(cEntity_Base.CardSpriteName))
                    {
                        Debug.Log(cEntity_Base.CardID);
                    }

                    cEntity_Bases.Add(cEntity_Base);
                }

                EditorGUI.BeginChangeCheck();

                CCtrl.CardList = DeckData.SortedCardPoolList(cEntity_Bases).ToArray();
                cEntity_Bases.Sort((a, b) => a.CardIndex - b.CardIndex);
                CCtrl.SortedCardList = cEntity_Bases.ToArray();

                if (EditorGUI.EndChangeCheck())
                {
                    var scene = SceneManager.GetActiveScene();
                    EditorSceneManager.MarkSceneDirty(scene);
                }

                return;
            }
        }
    }
}