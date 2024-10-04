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
    public class FindInconsistentTrait : Editor
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
                bool edited = false;

                foreach (string trait in card.Type_ENG)
                {
                    if (!trait.Contains(value.stringToFind))
                        continue;

                    if (trait.Equals(value.stringToCompare))
                        continue;

                    card.Type_ENG[card.Type_ENG.FindIndex(str => str == trait)] = value.stringToCompare;
                    _entities.Add(card);
                    edited = true;
                }

                if(edited)
                    EditorUtility.SetDirty(card);
            }

            Debug.Log($"Inconsistency Complete: Found {_entities.Count}");
            yield return null;
        }
    }
}

