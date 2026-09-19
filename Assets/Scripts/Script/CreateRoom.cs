using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class CreateRoom : OffAnimation
{
    [Header("Animator")]
    public Animator anim;

    [Header("RoomManager")]
    public RoomManager roomManager;

    public void SetUpCreateRoom()
    {
        if (this.gameObject.activeSelf)
        {
            return;
        }

        this.gameObject.SetActive(true);

        anim.SetInteger("Open", 1);
        anim.SetInteger("Close", 0);
    }

    public void CloseCreateRoom()
    {
        anim.SetInteger("Open", 0);
        anim.SetInteger("Close", 1);
    }

    public void OnClickCreateRoomButton()
    {
        ContinuousController.instance.isAI = false;
        ContinuousController.instance.isRandomMatch = false;
        roomManager.SetUpRoom();
        CloseCreateRoom();
    }
}
