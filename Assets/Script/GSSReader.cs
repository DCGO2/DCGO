using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
//you can get the data of Google Spreadsheet
public class GSSReader : MonoBehaviour
{
    string SheetID = "14rwapJWhB0ffMC4aEKCudh5bqsq2a9hkPnrphA8v438";
    string SheetName = "1148909742";//"0";
    public UnityEvent OnLoadEnd;
    public bool IsLoading { get; private set; }
    public string[][] Datas { get; private set; }
    IEnumerator GetFromWeb()
    {
        IsLoading = true;
        var tqx = "tqx=out:csv";
        var url = "https://docs.google.com/spreadsheets/d/" + SheetID + "/gviz/tq?" + tqx + "&sheet=" + SheetName;
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        IsLoading = false;

        if (request.result == UnityWebRequest.Result.ConnectionError )
        {
            Debug.LogError(request.error);
            OnLoadEnd.Invoke();
        }
        else
        {
            Datas = ConvertCSVtoJaggedArray(request.downloadHandler.text);
            OnLoadEnd.Invoke();
        }
    }
    public void Reload() => StartCoroutine(GetFromWeb());
    static string[][] ConvertCSVtoJaggedArray(string t)
    {
        var reader = new StringReader(t);
        reader.ReadLine();  //skipping over headers
        var rows = new List<string[]>();
        while (reader.Peek() >= 0)
        {
            var line = reader.ReadLine();        // Read one line at a time
            var elements = line.Split(',');    // Row cells are separated by ",".
            for (var i = 0; i < elements.Length; i++)
            {
                elements[i] = elements[i].TrimStart('"').TrimEnd('"');
            }
            rows.Add(elements);
        }
        return rows.ToArray();
    }
}