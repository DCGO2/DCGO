using System.Collections;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
public class TestWWW : MonoBehaviour
{
    [SerializeField] string SetID;
    [SerializeField] int minCardID;
    [SerializeField] int maxCardID;
    [SerializeField] int minParallelID;
    [SerializeField] int maxParallelID;

    List<int> cardIDs
    {
        get
        {
            List<int> cardIDs = new List<int>();

            for (int i = minCardID; i < maxCardID + 1; i++)
            {
                cardIDs.Add(i);
            }

            return cardIDs;
        }
    }

    public void OnClickGetJPNCardListText()
    {
        StopGetCardImages();

        StartCoroutine(GetCardListTexts(false));
    }

    public void OnClickGetENGCardListText()
    {
        StopGetCardImages();

        StartCoroutine(GetCardListTexts(true));
    }

    IEnumerator GetCardListTexts(bool isEnglish)
    {
        Debug.Log("テキストファイル取得開始");

        yield return StartCoroutine(getCardListText(SetID));

        IEnumerator getCardListText(string SetID)
        {
            List<string> CardListIDs = DataBase.CardListIDs(SetID, isEnglish);

            foreach (string CardListID in CardListIDs)
            {
                string url = "";

                if (isEnglish)
                {
                    url = $"https://world.digimoncard.com/cardlist/index.php?search=true&category={CardListID}";
                }

                else
                {
                    url = $"https://digimoncard.com/cardlist/index.php?search=true&category={CardListID}";
                }

                WebClient wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                string resultText = wc.DownloadString(url);
                List<string> parseByEnter = resultText.Split('\n').ToList();

                int startIndex = 0;

                for (int i = 0; i < parseByEnter.Count; i++)
                {
                    if (!string.IsNullOrEmpty(parseByEnter[i]))
                    {
                        bool replaceEmpty = true;

                        if (isEnglish)
                        {
                            if (i >= 1)
                            {
                                if (!string.IsNullOrEmpty(parseByEnter[i]) && !string.IsNullOrEmpty(parseByEnter[i - 1]))
                                {
                                    if (parseByEnter[i - 1].Contains("Digivolveeffect") && parseByEnter[i - 1].Contains("<dt>") && parseByEnter[i - 1].Contains("</dt>"))
                                    {
                                        replaceEmpty = false;
                                    }

                                    if (parseByEnter[i - 1].Contains("Securityeffect") && parseByEnter[i - 1].Contains("<dt>") && parseByEnter[i - 1].Contains("</dt>"))
                                    {
                                        replaceEmpty = false;
                                    }

                                    if (parseByEnter[i - 1].Contains("Effect") && parseByEnter[i - 1].Contains("<dt>") && parseByEnter[i - 1].Contains("</dt>"))
                                    {
                                        replaceEmpty = false;
                                    }
                                }
                            }
                        }

                        parseByEnter[i] = parseByEnter[i].Replace("\t", "").Replace("\n", "").Trim();

                        if (replaceEmpty)
                        {
                            parseByEnter[i] = parseByEnter[i].Replace(" ", "").Trim();
                        }

                        else
                        {
                            Debug.Log($"!replaceEmpty:{parseByEnter[i]}");
                        }

                        if (parseByEnter[i].Contains($"<liclass=\"image_lists_itemdatapage-1\">"))
                        {
                            startIndex = i;
                            break;
                        }
                    }
                }

                parseByEnter = parseByEnter.GetRange(startIndex, parseByEnter.Count - startIndex);

                int endIndex = 0;

                for (int i = 0; i < parseByEnter.Count; i++)
                {
                    if (!string.IsNullOrEmpty(parseByEnter[i]))
                    {
                        bool replaceEmpty = true;

                        if (isEnglish)
                        {
                            if (i >= 1)
                            {
                                if (!string.IsNullOrEmpty(parseByEnter[i]) && !string.IsNullOrEmpty(parseByEnter[i - 1]))
                                {
                                    if (parseByEnter[i - 1].Contains("Digivolveeffect") && parseByEnter[i - 1].Contains("<dt>") && parseByEnter[i - 1].Contains("</dt>"))
                                    {
                                        replaceEmpty = false;
                                    }

                                    if (parseByEnter[i - 1].Contains("Securityeffect") && parseByEnter[i - 1].Contains("<dt>") && parseByEnter[i - 1].Contains("</dt>"))
                                    {
                                        replaceEmpty = false;
                                    }

                                    if (parseByEnter[i - 1].Contains("Effect") && parseByEnter[i - 1].Contains("<dt>") && parseByEnter[i - 1].Contains("</dt>"))
                                    {
                                        replaceEmpty = false;
                                    }
                                }
                            }
                        }

                        parseByEnter[i] = parseByEnter[i].Replace("\t", "").Replace("\n", "").Trim();

                        if (replaceEmpty)
                        {
                            parseByEnter[i] = parseByEnter[i].Replace(" ", "").Trim();
                        }

                        if (parseByEnter[i].Contains($"clearfix"))
                        {
                            for (int j = 0; j < 7; j++)
                            {
                                if (!string.IsNullOrEmpty(parseByEnter[i - j]))
                                {
                                    if (parseByEnter[i - j].Contains($"</ul>"))
                                    {
                                        if (parseByEnter[i - j].Contains($"</li>"))
                                        {
                                            endIndex = i - j;
                                        }

                                        else
                                        {
                                            endIndex = i - j - 1;
                                        }

                                        break;
                                    }
                                }
                            }

                        }
                    }
                }

                parseByEnter = parseByEnter.GetRange(0, endIndex + 1);

                parseByEnter[parseByEnter.Count - 1] = parseByEnter[parseByEnter.Count - 1].Replace("</ul>", "");

                string savingText = "";

                for (int i = 0; i < parseByEnter.Count; i++)
                {
                    savingText += $"{parseByEnter[i]}\n";
                }

                StreamWriter sw = new StreamWriter(Application.dataPath + $"/TextAsset/{CardListID}.txt", false);// 指定されたファイル名のテキストファイルを新規で用意
                sw.WriteLine(savingText);// ファイルに書き出したあと改行
                sw.Flush();// StreamWriterのバッファに書き出し残しがないか確認
                sw.Close();// ファイルを閉じる
                Debug.Log(Application.dataPath + $"/TextAsset/{CardListID}.txt");
                Debug.Log($"保存:{SetID},{CardListID}");

                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public void StopGetCardImages()
    {
        StopAllCoroutines();
    }

    public void OnClickGetEnglishCardImageButton()
    {
        StopGetCardImages();
        StartCoroutine(GetCardImages(cardIDs, true));
    }

    public void OnClickGetJapaneseCardImageButton()
    {
        StopGetCardImages();
        StartCoroutine(GetCardImages(cardIDs, false));
    }

    IEnumerator GetCardImages(List<int> cardIDs, bool isEnglish)
    {
        picsCount = 0;

        Debug.Log("カード画像保存開始");

        if (minParallelID < 0)
        {
            minParallelID = 0;
        }

        for (int i = minParallelID; i < maxParallelID + 1; i++)
        {
            foreach (int cardID in cardIDs)
            {
                yield return StartCoroutine(GetCardImage(cardID, i, isEnglish));

                yield return new WaitForSeconds(0.1f);
            }
        }

        Debug.Log("カード画像保存終了");
    }

    int picsCount = 0;

    IEnumerator GetCardImage(int cardID, int ParallelID, bool isEnglish)
    {
        bool Parallel = ParallelID >= 1;

        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("ネットワークに接続されていない");
            yield return new WaitForSeconds(5f);
        }

        string cardIDString = string.Format("{0:000}", cardID);

        if (SetID.Contains("ST"))
        {
            cardIDString = string.Format("{0:00}", cardID);
        }

        string cardImageURL = $"{SetID}-{cardIDString}";

        if (Parallel)
        {
            cardImageURL += $"_P{ParallelID}";
        }

        string picsURL = $"";

        if (isEnglish)
        {
            picsURL = $"https://world.digimoncard.com/images/cardlist/card/{cardImageURL}.png";
        }

        else
        {
            picsURL = $"https://digimoncard.com/images/cardlist/card/{cardImageURL}.png";
        }

        UnityWebRequest webReq_CardImage = UnityWebRequestTexture.GetTexture(picsURL);
        yield return webReq_CardImage.SendWebRequest();

        if (webReq_CardImage.result == UnityWebRequest.Result.ProtocolError || webReq_CardImage.result == UnityWebRequest.Result.ConnectionError)
        {

        }

        else
        {
            try
            {
                string fileName = $"{cardImageURL}.png";

                if (isEnglish)
                {
                    File.WriteAllBytes(@$"C:\Users\USER\Pictures\DigimonCard/Card_ENG/{fileName}", webReq_CardImage.downloadHandler.data);
                }

                else
                {
                    File.WriteAllBytes(@$"C:\Users\USER\Pictures\DigimonCard/Card_JPN/{fileName}", webReq_CardImage.downloadHandler.data);
                }

                picsCount++;

                if (picsCount % 8 == 1)
                {
                    Debug.Log($"{fileName}のカード画像保存完了");
                }
            }

            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        yield return null;
    }
}
