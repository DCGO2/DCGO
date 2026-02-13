using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class OpenURL : MonoBehaviour
{
    public string URL;

    public List<string> HashTacgs;

    public void Open()
    {
        Application.OpenURL(URL);
    }

    public void OpenTweetURL()
    {
        List<string> tags = new List<string>();

        foreach(string HashTag in HashTacgs)
        {
            string tag = UnityEngine.Networking.UnityWebRequest.EscapeURL(HashTag);

            tags.Add(tag);
        }

        string _URL = "https://twitter.com/intent/tweet?text=";

        foreach(string tag in tags)
        {
            _URL += $"&hashtags={tag}";
        }

        Application.OpenURL(_URL);
    }
}
