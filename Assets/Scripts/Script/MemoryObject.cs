using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
public class MemoryObject : MonoBehaviour
{
    [SerializeField] GameObject CurrentMemoryObject;

    public List<MemoryTab> memoryTabs = new List<MemoryTab>();

    [SerializeField] MemoryPredictionLine memoryPredictionLine;

    int oldMemory = 0;
    public void Init()
    {
        AssignMemoryTab();
        memoryPredictionLine.Init();

        foreach (MemoryTab memoryTab in memoryTabs)
        {
            memoryTab.Init();
        }
    }

    public void AssignMemoryTab()
    {
        bool isMasterClient = GManager.instance.You.PlayerID == 0;

        for (int i = 0; i < memoryTabs.Count; i++)
        {
            if (isMasterClient)
            {
                memoryTabs[i].Memory = i - 10;
            }

            else
            {
                memoryTabs[i].Memory = 10 - i;
            }
        }
    }

    public IEnumerator SetMemory()
    {
        if (oldMemory != GManager.instance.turnStateMachine.gameContext.Memory)
        {
            bool isMasterClient = GManager.instance.You.PlayerID == 0;

            MemoryTab targetMemoryTab = null;
            MemoryTab currentMemoryTab = null;

            foreach (MemoryTab memoryTab in memoryTabs)
            {
                if (memoryTab.Memory == GManager.instance.turnStateMachine.gameContext.Memory)
                {
                    targetMemoryTab = memoryTab;
                }

                if (memoryTab.Memory == oldMemory)
                {
                    currentMemoryTab = memoryTab;
                }
            }

            if (targetMemoryTab != null)
            {
                List<MemoryTab> lightMemoryTabs = new List<MemoryTab>();

                int startIndex = memoryTabs.IndexOf(currentMemoryTab);
                int endIndex = memoryTabs.IndexOf(targetMemoryTab);

                if (startIndex < endIndex)
                {
                    for (int i = 0; i < memoryTabs.Count; i++)
                    {
                        if (startIndex <= i && i <= endIndex)
                        {
                            lightMemoryTabs.Add(memoryTabs[i]);
                        }
                    }
                }

                else
                {
                    for (int i = 0; i < memoryTabs.Count; i++)
                    {
                        if (endIndex <= i && i <= startIndex)
                        {
                            lightMemoryTabs.Add(memoryTabs[i]);
                        }
                    }
                }

                foreach (MemoryTab memoryTab in lightMemoryTabs)
                {
                    if (memoryTab.Light != null)
                    {
                        memoryTab.Light.SetActive(true);
                    }
                }


                #region アニメーション
                bool end = false;
                var sequence = DOTween.Sequence();

                Vector3 TargetPosition = targetMemoryTab.tabObject.transform.localPosition;

                float GoTime = 0.2f;

                sequence
                    .Append(CurrentMemoryObject.transform.DOLocalMove(TargetPosition, GoTime).SetEase(Ease.OutCubic))
                    .AppendCallback(() =>
                    {
                        end = true;
                    });

                sequence.Play();

                yield return new WaitWhile(() => !end);
                end = false;
                #endregion

                #region setting used blitz
                if (GManager.instance.attackProcess.UsedBlitz)
                {
                    if (GManager.instance.turnStateMachine.gameContext.Memory <= 0)
                        GManager.instance.attackProcess.UsedBlitz = false;
                }
                    
                #endregion
            }
        }

        foreach (MemoryTab memoryTab in memoryTabs)
        {
            if (memoryTab.Light != null)
            {
                memoryTab.Light.SetActive(false);
            }
        }

        oldMemory = GManager.instance.turnStateMachine.gameContext.Memory;
    }

    public void ShowMemoryPredictionLine(int nextMemory)
    {
        if (nextMemory >= 10)
        {
            nextMemory = 10;
        }

        else if (nextMemory <= -10)
        {
            nextMemory = -10;
        }

        MemoryTab currentMemoryTab = null;
        MemoryTab nextMemoryTab = null;

        foreach (MemoryTab memoryTab in memoryTabs)
        {
            if (memoryTab.Memory == GManager.instance.turnStateMachine.gameContext.Memory)
            {
                currentMemoryTab = memoryTab;
            }

            if (memoryTab.Memory == nextMemory)
            {
                nextMemoryTab = memoryTab;
            }
        }

        if (currentMemoryTab != null && nextMemoryTab != null)
        {
            memoryPredictionLine.SetMemoryPredictionLine(currentMemoryTab, nextMemoryTab);
        }

        else
        {
            OffMemoryPredictionLine();
        }
    }

    public void OffMemoryPredictionLine()
    {
        memoryPredictionLine.gameObject.SetActive(false);
    }
}

[Serializable]
public class MemoryTab
{
    public int Memory { get; set; }
    public GameObject tabObject;

    public GameObject Light
    {
        get
        {
            if (tabObject != null)
            {
                if (tabObject.transform.childCount >= 2)
                {
                    return tabObject.transform.GetChild(tabObject.transform.childCount - 2).gameObject;
                }
            }

            return null;
        }
    }

    public void Init()
    {
        if (Light != null)
        {
            Light.SetActive(false);
        }
    }
}
