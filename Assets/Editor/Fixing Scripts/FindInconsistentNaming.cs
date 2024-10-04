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
        public enum DataType
        {
            name,
            text,
            attribute,
            type
        }

        public DataType dataType;
        public string stringToFind;
        public string stringToCompare;
    }

    public class TrackedData
    {
        public CEntity_Base entity;
        public string name;
        public List<string> tags = new List<string>();
    }

    [CustomEditor(typeof(InconsistentName))]
    public class FindInconsistentNaming : Editor
    {
        InconsistentName _stringValue;
        List<TrackedData> _entities;

        public override void OnInspectorGUI()
        {
            _stringValue = target as InconsistentName;
            DrawDefaultInspector();

            if (GUILayout.Button("Find Inconsistencies"))
                EditorCoroutineUtility.StartCoroutine(FindInconsistency(_stringValue), this);

            if (_entities == null)
                return;

            GUILayout.Space(30);

            GUILayout.Label($"Inconsistencies Found: {_entities.Count}");

            GUILayout.Space(10);

            if (GUILayout.Button("Fix Inconsistencies"))
                UpdateInconsistencies(_stringValue);

            
            GUILayout.BeginVertical();

            foreach (TrackedData data in _entities)
            {
                if (GUILayout.Button($"Card ID: {data.entity.CardID}"))
                    Selection.SetActiveObjectWithContext(data.entity, null);
            }

            GUILayout.EndVertical();
        }

        IEnumerator FindInconsistency(InconsistentName value)
        {
            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");
            _entities = new List<TrackedData>();

            foreach (CEntity_Base card in List)
            {
                TrackedData data = new TrackedData();

                switch (value.dataType)
                {
                    case InconsistentName.DataType.name:
                        if (!card.CardName_ENG.Contains(value.stringToFind))
                            continue;

                        data.entity = card;
                        data.name = card.CardName_ENG.Replace(value.stringToFind,value.stringToCompare);
                        _entities.Add(data);
                        break;
                    case InconsistentName.DataType.text:

                        break;
                    case InconsistentName.DataType.attribute:
                        List<string> attributes = card.Attribute_ENG.Clone().ToList();

                        for (int i = 0; i < attributes.Count; i++)
                        {
                            if (!attributes[i].Contains(value.stringToFind))
                                continue;

                            if (attributes[i].Equals(value.stringToCompare))
                                continue;

                            attributes[i] = value.stringToCompare;
                            data.entity = card;
                            data.tags = attributes;

                            _entities.Add(data);
                        }
                        break;
                    case InconsistentName.DataType.type:
                        List<string> types = card.Type_ENG.Clone().ToList();

                        for (int i = 0; i < types.Count; i++)
                        {
                            if (!types[i].Contains(value.stringToFind))
                                continue;

                            if (types[i].Equals(value.stringToCompare))
                                continue;

                            types[i] = value.stringToCompare;
                            data.entity = card;
                            data.tags = types;

                            _entities.Add(data);
                        }
                        break;
                }
                
            }

            Debug.Log($"Inconsistency Complete: Found {_entities.Count}");
            yield return null;
        }
        void UpdateInconsistencies(InconsistentName value)
        {
            
            foreach (TrackedData data in _entities)
            {
                switch (value.dataType)
                {
                    case InconsistentName.DataType.name:
                        data.entity.CardName_ENG = data.name;
                        break;
                    case InconsistentName.DataType.text:

                        break;
                    case InconsistentName.DataType.attribute:
                        data.entity.Attribute_ENG = data.tags;
                        break;
                    case InconsistentName.DataType.type:
                        data.entity.Type_ENG = data.tags;
                        break;
                }

                EditorUtility.SetDirty(data.entity);
            }
        }
    }
}

