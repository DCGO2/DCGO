using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using DCGO.CardEntities;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using UnityEngine.Networking;
using WebSocketSharp;
using static UnityEngine.ParticleSystem;

namespace DCGO.Tools.Repair{

    [CreateAssetMenu(fileName = "CardEntity_Inconsistency", menuName = "Create Inconsistency Entity")]
    public class InconsistentName : ScriptableObject
    {
        public string stringToFind;
        public string stringToCompare;
    }

    [CustomEditor(typeof(InconsistentName))]
    public class FindInconsistentName : Editor
    {
        InconsistentName _stringValue;
        List<CEntity_Base> _entities;

        public override void OnInspectorGUI()
        {
            _stringValue = target as InconsistentName;
            DrawDefaultInspector();

            if (GUILayout.Button("Find Inconsistencies"))
                EditorCoroutineUtility.StartCoroutine(FindInconsistency(_stringValue), this);

            if (_entities == null)
                return;
        }

        IEnumerator FindInconsistency(InconsistentName value)
        {
            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");
            _entities = new List<CEntity_Base>();

            foreach (CEntity_Base card in List)
            {
                List<string> traits = new List<string>();
                traits.AddRange(card.Attribute_ENG);
                traits.AddRange(card.Type_ENG);

                foreach (string trait in card.Attribute_ENG)
                {
                    if (!trait.Contains(value.stringToFind))
                        continue;

                    if (trait.Equals(value.stringToCompare))
                        continue;

                    card.Attribute_ENG[card.Attribute_ENG.FindIndex(str => str == trait)] = value.stringToCompare;
                    _entities.Add(card);
                }

                EditorUtility.SetDirty(card);
            }

            Debug.Log($"Inconsistency Complete: Found {_entities.Count}");
            yield return null;
        }
    }
}

