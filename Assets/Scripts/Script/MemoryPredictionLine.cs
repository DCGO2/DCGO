using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryPredictionLine : MonoBehaviour
{
    [SerializeField] float p_y = 11f;
    [SerializeField] int vertexCount = 100;
    [SerializeField] float startWidthMultiplier = 0.5f;
    [SerializeField] float width = 2f;

    LineRenderer lineRenderer;

    public void Init()
    {
        lineRenderer = GetComponent<LineRenderer>();
        this.gameObject.SetActive(false);
    }

    public void SetMemoryPredictionLine(MemoryTab currentMemoryTab, MemoryTab nextMemoryTab)
    {
        if(lineRenderer == null || currentMemoryTab == null || nextMemoryTab == null)
        {
            return;
        }

        if(currentMemoryTab == nextMemoryTab)
        {
            this.gameObject.SetActive(false);
            return;
        }

        if(vertexCount <= 0)
        {
            return;
        }

        if(p_y == 0)
        {
            return;
        }

        this.gameObject.SetActive(true);

        lineRenderer.positionCount = vertexCount;

        Vector3 memoryTabPos(MemoryTab memoryTab)
        {
            Vector3 position = new Vector3();

            int index = GManager.instance.memoryObject.memoryTabs.IndexOf(memoryTab) - 10;

            position = new Vector3(9f * index, 0, 3);

            return position;
        }

        float alpha = Mathf.Abs(memoryTabPos(nextMemoryTab).x - memoryTabPos(currentMemoryTab).x) / 2f;
        float p_x = (memoryTabPos(nextMemoryTab).x + memoryTabPos(currentMemoryTab).x) / 2f;
        float a = (p_y - memoryTabPos(nextMemoryTab).z) / Mathf.Pow(alpha, 2);

        Vector3[] positions = new Vector3[vertexCount];

        for(int i=0;i<vertexCount;i++)
        {
            float x = 2 * alpha / vertexCount * i + -alpha + p_x;

            float z = function_z(x);

            positions[i] = new Vector3(x, 0, z);

            float function_z(float x)
            {
                float z = 0;

                z = -a * Mathf.Pow(x - p_x, 2) + p_y;

                return z;
            }
        }

        lineRenderer.SetPositions(positions);

        AnimationCurve animationCurve = new AnimationCurve();

        float a_anim = -4f * startWidthMultiplier + 4;

        for (int i = 0; i < vertexCount; i++)
        {
            float x = 1f / vertexCount * i;

            float y = function_anim_y(x);

            animationCurve.AddKey(x,y);

            float function_anim_y(float x)
            {
                float y = 0;

                y = -a_anim * Mathf.Pow(x - 0.5f, 2) + 1;

                return y;
            }
        }

        lineRenderer.widthCurve = animationCurve;

        lineRenderer.widthMultiplier = width;
    }
}
