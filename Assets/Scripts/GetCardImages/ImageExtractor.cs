using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using WebSocketSharp;
using System.IO;
using UnityEngine.UI;

public class ImageExtractor : MonoBehaviour
{
    public TMP_InputField setIDField;
    public GameObject downloadButton;
    public GameObject stopButton;
    public TMP_Text downloadStatusText;
    public Image _img;


    private string _setID;
    private List<string> _setList = new List<string>(new string[]
    {
        "ST1",
        "ST2",
        "ST3",
        "ST4",
        "ST5",
        "ST6",
        "ST7",
        "ST8",
        "ST9",
        "ST10",
        "ST12",
        "ST13",
        "ST14",
        "ST15",
        "ST16",
        "ST17",
        "BT1",
        "BT2",
        "BT3",
        "BT4",
        "BT5",
        "BT6",
        "BT7",
        "BT8",
        "BT9",
        "BT10",
        "BT11",
        "BT12",
        "BT13",
        "BT14",
        "BT15",
        "BT16",
        "EX1",
        "EX2",
        "EX3",
        "EX4",
        "EX5",
        "RB1",
        "LM",
        "P"
    });

    private List<string> _downloadList = new List<string>();

    private int _cardID = 0;
    private string _cardIDString;
    private bool _gatheringSet = false;
    
    private bool _gatheringParrallel = false;
    private int _parallelID = 0;

    public void OnSetIDChanged(string value)
    {
        _setID = value.ToUpper();
    }

    public void OnClickGetEnglishCardImageButton()
    {
        StopGetCardImages();
        _downloadList.Clear();


        if (_setID.IsNullOrEmpty())
        {
            foreach (string set in _setList)
                _downloadList.Add(set);
        }
        else
        {
            _downloadList.Add(_setID);
        }

        StartCoroutine(GetSetImages());
    }

    IEnumerator GetSetImages()
    {
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            UpdateStatus("Waiting for Internet connection....");
            yield return new WaitForSeconds(5f);
        }

        foreach (string SetID in _downloadList)
        {
            UpdateStatus($"Getting {SetID} Card Images....");

            _cardID = 0;

            _gatheringSet = true;
            while (_gatheringSet)
            {
                _cardID++;

                _cardIDString = string.Format("{0:000}", _cardID);

                if (SetID.Contains("ST"))
                    _cardIDString = string.Format("{0:00}", _cardID);

                string cardImageURL = $"{SetID}-{_cardIDString}";

                yield return StartCoroutine(GetCardImage(cardImageURL));

                _parallelID = 0;
                _gatheringParrallel = true;
                while (_gatheringParrallel)
                {
                    _parallelID++;
                    yield return StartCoroutine(GetCardImage(cardImageURL + $"_P{_parallelID}"));
                }
            }

            UpdateStatus($"Completed {SetID} Images");
        }

        OnCompleteImages();
        UpdateStatus($"Completed ALL Images");

        yield return null;
    }



    IEnumerator GetCardImage(string cardID)
    {
        string picsURL = $"https://world.digimoncard.com/images/cardlist/card/{cardID}.png";
        string ImagePath = Path.GetFullPath(Path.Combine(Application.dataPath, @"..\..\Textures\Card\", $"{cardID}.png"));

        if(File.Exists(ImagePath))
        {
            Debug.Log($"File Exists: {cardID}");
            yield break;
        }

        UnityWebRequest webReq_CardImage = UnityWebRequestTexture.GetTexture(picsURL);
        yield return webReq_CardImage.SendWebRequest();

        if (webReq_CardImage.result == UnityWebRequest.Result.Success)
        {
            try
            {
                UpdateStatus($"Image Complete: {ImagePath}");
                
                File.WriteAllBytes(ImagePath, webReq_CardImage.downloadHandler.data);
                Texture2D texture = DownloadHandlerTexture.GetContent(webReq_CardImage);

                Sprite s = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                     Vector2.zero, 1f);
                _img.sprite = s;
            }

            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
        else
        {
            //failed to load
            if (webReq_CardImage.error.Contains("404"))
            {
                if (cardID.Contains("_P"))
                    _gatheringParrallel = false;
                else
                    _gatheringSet = false;

                Debug.Log("Card does not exsist....moving on");
                yield break;
            }
         
            Debug.Log($"Failed to load: {webReq_CardImage.error}");
        }

        yield return null;
    }


    private void UpdateStatus(string status)
    {
        downloadStatusText.text = status;
    }
    private void OnCompleteImages()
    {
        downloadButton.SetActive(true);
        stopButton.SetActive(false);
        _img.sprite = null;
        UpdateStatus("");
    }
    public void StopGetCardImages()
    {
        StopAllCoroutines();
        _img.sprite = null;
        UpdateStatus("");
    }
}
