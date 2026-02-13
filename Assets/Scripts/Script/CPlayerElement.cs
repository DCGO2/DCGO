using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPlayerElement : MonoBehaviour
{
    //For Room Information UI display
    public Text PlayerName;   //PlayerName
    public Text IsReady; //standby state

    //For storing the roomname of the room entry button
    private string playername;

    //Function to set Room information from GetRoomList to RoomElement
    public void SetPlayerInfo(string _PlayerName, bool _IsReady)
    {
        //Obtain roomname for room entry button
        playername = _PlayerName;
        PlayerName.text = _PlayerName;

        if (_IsReady)
        {
            IsReady.text = LocalizeUtility.
            GetLocalizedString(
                EngMessage: "Ready",
                JpnMessage: "準備完了"
            );

            IsReady.color = new Color32(53, 255, 4, 255);
        }

        else
        {
            IsReady.text = LocalizeUtility.
            GetLocalizedString(
                EngMessage: "Not Ready",
                JpnMessage: "準備中"
            );

            IsReady.color = Color.red;
        }
    }
}
