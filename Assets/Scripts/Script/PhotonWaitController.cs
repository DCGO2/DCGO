using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;

public class PhotonWaitController : MonoBehaviour
{
    int waitCount = 0;
    public Text SyncText;

    private void Start()
    {
        OffSyncText();
    }

    void OffSyncText()
    {
        if (SyncText != null)
        {
            SyncText.gameObject.SetActive(false);
        }
    }

    IEnumerator SetWaitingText(string defultString)
    {
        float waitTime = 0.18f;

        int count = 0;

        if (SyncText != null)
        {
            SyncText.gameObject.SetActive(true);

            while (SyncText.gameObject.activeSelf)
            {
                count++;

                if (count >= 4)
                {
                    count = 0;
                }

                SyncText.text = defultString;

                for (int i = 0; i < count; i++)
                {
                    SyncText.text += ".";
                }

                yield return new WaitForSeconds(waitTime);
            }
        } 
    }

    public void SetWaiting(string key, bool isGo, bool isAdd)
    {
        Hashtable PlayerProp = PhotonNetwork.LocalPlayer.CustomProperties;

        object value;

        if (isAdd)
        {
            if (PlayerProp.TryGetValue(key, out value))
            {
                if ((bool)PlayerProp[key] && !isGo)
                {
                    return;
                }

                PlayerProp[key] = isGo;
            }

            else
            {
                PlayerProp.Add(key, isGo);
            }
        }

        else
        {
            if (PlayerProp.TryGetValue(key, out value))
            {
                PlayerProp.Remove(key);
            }
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerProp);
    }


    public static bool isWaiting(string key, Photon.Realtime.Player player)
    {
        Hashtable PlayerProp = PhotonNetwork.LocalPlayer.CustomProperties;

        object value;

        if (PlayerProp.TryGetValue(key, out value))
        {
            if ((bool)value)
            {
                return false;
            }

            else
            {
                return true;
            }
        }

        return true;
    }

    public static bool AllIsWaiting(string key)//Returns true if everyone has key false
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            Hashtable PlayerProp = player.CustomProperties;

            object value;

            if (PlayerProp.TryGetValue(key, out value))
            {
                if ((bool)value)
                {
                    return false;
                }
            }

            else
            {
                return false;
            }
        }

        return true;
    }

    bool RoomHasTrueKey(string key)
    {
        Hashtable roomHash = PhotonNetwork.CurrentRoom.CustomProperties;
        object value;

        if(roomHash != null)
        {
            if (roomHash.TryGetValue(key, out value))
            {
                if ((bool)value)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    void SetRoomTrueKey(string key)
    {
        Hashtable roomHash = PhotonNetwork.CurrentRoom.CustomProperties;
        object value;

        if(roomHash == null)
        {
            roomHash = new Hashtable();
        }

        if (roomHash.TryGetValue(key, out value))
        {
            roomHash[key] = true;//if true, it can go through
        }

        else
        {
            roomHash.Add(key, true);
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomHash);
    }

    public IEnumerator Wait(string key)
    {
        StartCoroutine(SetWaitingText("Syncing"));

        Debug.Log($"Start Waiting:{key}");

        while (isWaiting(key, PhotonNetwork.LocalPlayer))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (AllIsWaiting(key))
                {
                    yield return new WaitForSeconds(Time.deltaTime);

                    SetRoomTrueKey(key);
                }
            }

            if (RoomHasTrueKey(key))
            {
                SetWaiting(key, true, false);
                break;
            }

            yield return null;
        }

        yield return null;

        OffSyncText();

        Debug.Log($"End Waiting:{key}");
    }

    public Coroutine StartWait(string key)
    {
        waitCount++;
        key += "_" + waitCount.ToString();

        if(PhotonNetwork.InRoom)
        {
            key += "_" + PhotonNetwork.CurrentRoom.Name;
        }
        //TODO Currently Disabled For Alpha Testing To Fix Xross Issues
        key += "_" + UnityEngine.Random.Range(0, 9999999).ToString();
        //int synchronizedSeed = PhotonNetwork.CurrentRoom.Name.GetHashCode() + waitCount;
        //System.Random random = new System.Random(synchronizedSeed);
        //key += "_" + random.Next(0, 9999999).ToString();
        
        keys.Add(key);

        if (!GManager.instance.IsAI)
        {
            if (!RoomHasTrueKey(key))
            {
                SetWaiting(key, false, true);

                return StartCoroutine(Wait(key));
            }
        }
        
        return null;
    }

    public List<string> keys = new List<string>();

    public void ResetKeys()
    {
        Hashtable PlayerProp = PhotonNetwork.LocalPlayer.CustomProperties;

        foreach(string key in keys)
        {
            PlayerProp.Remove(key);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerProp);
    }
}
