using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterDeck : MonoBehaviour
{
    public List<StarterDeckData> starterDeckDatas = new List<StarterDeckData>();

    public void SetStarterDecks()
    {
        if (ContinuousController.instance.DeckDatas.Count == 0)
        {
            foreach (StarterDeckData starterDeckData in starterDeckDatas)
            {
                starterDeckData.AddDeckData();
            }
        }

        else
        {
            foreach(StarterDeckData starterDeckData in starterDeckDatas)
            {
                if(!starterDeckData.HasPlayerPrefs())
                {
                    starterDeckData.AddDeckData();
                }
            }
        }
    }
}

[System.Serializable]
public class StarterDeckData
{
    public string Key;
    public string DeckCode;

    public void AddDeckData()
    {
        string deckCode = ContinuousController.instance.ShuffleDeckCode.GetDeckCode(DeckCode);
        ContinuousController.instance.DeckDatas.Add(new DeckData(deckCode));
        PlayerPrefs.SetInt(Key, 2);
    }

    public bool HasPlayerPrefs()
    {
        if(PlayerPrefs.HasKey(Key))
        {
            if(PlayerPrefs.GetInt(Key) == 2)
            {
                return true;
            }
        }

        return false;
    }
}