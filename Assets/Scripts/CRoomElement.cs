using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;

public class CRoomElement : MonoBehaviour
{
    //For Room Information UI display
    public Text PlayerNumber;   //the number of people
    public Text RoomCreator;    //Room Creator Name

    //For storing the roomname of the room entry button
    private string roomname;

    public string RoomName { get => roomname; }

    //Function to set Room information from GetRoomList to RoomElement
    public void SetRoomInfo(string _RoomName, int _PlayerNumber, string _RoomCreator)
    {
        roomname = _RoomName;
        //Obtain roomname for room entry button
        PlayerNumber.text = "人　数：" + _PlayerNumber + "/2";
        RoomCreator.text = "作成者：" + _RoomCreator;
    }

    [HideInInspector] public UnityAction OnClick;

    //Entry button processing
    public void OnJoinRoomButton()
    {
        if (OnClick != null)
        {
            OnClick.Invoke();
        }
    }
}
