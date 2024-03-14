using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(MaskableGraphic))]
[ExecuteInEditMode]
public class DeformationImage : BaseMeshEffect
{
    public DeformationImageVertex[] vertsNodeList;
    public bool isClose = true;
    public RectTransform rectTransform { get { return graphic.rectTransform; } }
    public Color GraphicColor {get{return graphic.color;}}

    List<UIVertex> vertices = new List<UIVertex>();

    void Update()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            vertsNodeList = GetComponentsInChildren<DeformationImageVertex>(true);
        }
#endif
        graphic?.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        vertices.Clear();
        vh.GetUIVertexStream(vertices);
        ApplyVerts(vertices);
        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
    }

    protected void ApplyVerts(List<UIVertex> verts)
    {
        verts.Clear();
        for (int i = 0; i < vertsNodeList.Length - 2; i++)
        {
            AddVerts(verts, vertsNodeList[0].vertex, vertsNodeList[i + 1].vertex, vertsNodeList[i + 2].vertex);
        }

        if (isClose && vertsNodeList.Length > 3)
        {
            AddVerts(verts, vertsNodeList[0].vertex, vertsNodeList[1].vertex, vertsNodeList[vertsNodeList.Length - 1].vertex);
        }
    }

    //ポリゴンが表を向くように計算してAdd
    protected void AddVerts(List<UIVertex> verts, UIVertex v1, UIVertex v2, UIVertex v3)
    {
        var d1 = v1.position - v2.position;
        var d2 = v2.position - v3.position;
        var cross = d1.x * d2.y - d1.y * d2.x;
        if (cross < 0)
        {
            verts.Add(v1);
            verts.Add(v2);
            verts.Add(v3);
        }
        else
        {
            verts.Add(v1);
            verts.Add(v3);
            verts.Add(v2);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DeformationImage))]
public class DeformationImageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var di = target as DeformationImage;

        base.OnInspectorGUI();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("頂点追加"))
        {
            var newVertex = new GameObject();
            newVertex.name = $"v{di.vertsNodeList.Length}";
            var vertex = newVertex.AddComponent<DeformationImageVertex>();
            vertex.Init(di);

            var rectTransform = newVertex.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(10, 10);
            rectTransform.position = di.transform.position;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.SetParent(di.transform);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("頂点削除"))
        {
            if(di.vertsNodeList.Length > 0)
            {
                DestroyImmediate(di.vertsNodeList[di.vertsNodeList.Length - 1].gameObject);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("表示"))
        {
            foreach (var v in di.vertsNodeList)
            {
                v.gameObject.hideFlags = HideFlags.None;
            }
        }

        if (GUILayout.Button("非表示"))
        {
            foreach (var v in di.vertsNodeList)
            {
                v.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif
