using System;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
namespace TweenableObject.Networking
{
    public class PhotonTester : MonoBehaviour
    {
        public void Awake()
        {
            //ConnectToRegion("sa");
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
              //ConnectToRegion("usw");
              if (PhotonNetwork.IsConnected)
              {
                  Debug.Log(PhotonNetwork.CloudRegion);
              }
              else
              {
                  PhotonNetwork.ConnectUsingSettings();
              }
            }
        }

        public void ConnectToRegion(string region)
        {
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log(PhotonNetwork.CloudRegion);
            }
            else
            {
                //Move this to a function at the start of the game so we always get the best ping every time we launch 
                ServerSettings.ResetBestRegionCodeInPreferences();
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.ConnectToRegion(region);
            }
        }

        public void ResetConnection()
        {
            ServerSettings.ResetBestRegionCodeInPreferences();
        }
    }
}