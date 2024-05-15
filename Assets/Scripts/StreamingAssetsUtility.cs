
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEditor.PackageManager;
using System.Security.Policy;
using WebP;
using UnityEngine.Rendering;
using System.Data;
using UnityEngine.XR;

public class StreamingAssetsUtility
{
    public static async Task<byte[]> ReadFile(string path)
    {
        using (FileStream fileStream = new FileStream(
            path, FileMode.Open, FileAccess.Read))
        {
            var resultBytes = new byte[fileStream.Length];
            await fileStream.ReadAsync(resultBytes, 0, (int)fileStream.Length);
            return resultBytes;
        }
    }

    #region 画像の取得
    public static Texture2D BinaryToTexture(byte[] bytes)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);
        return texture;
    }

    public static async Task<Sprite> GetSprite(string fileName, bool isCard = false, bool isLauncher = false)
    {
        string path = "";

        if (isCard)
        {
            if (fileName.Contains("-token"))
            {
                return await GetTokenImageData(Path.Combine(GetStreamingAssetPath(isLauncher), $"Card/{fileName}.png").Replace("\\", "/"));
            }
            else
            {
                path = Path.Combine(GetStreamingAssetPath(isLauncher), $"Card/{fileName}.webp").Replace("\\", "/");

                if (!File.Exists(path))
                {
                    return await GetCardImageData(fileName, path);
                }
                else 
                {
                    return await GetCardImageDataLocal(path);
                }
            }
        }
        else
        {
            return await GetSpriteImage(fileName, isLauncher);
        }

        /*    if (isCard)
        {
            if (fileName.Contains("-token"))
            {
                return await GetTokenImageData(Path.Combine(GetStreamingAssetPath(isLauncher), $"Card/{fileName}.png").Replace("\\", "/"));
            }
            else
            {
                
            }            
        }
        else
        {
            path = Path.Combine(GetStreamingAssetPath(isLauncher), $"{fileName}.jpg").Replace("\\", "/");

            if (!File.Exists(path))
                path = Path.Combine(GetStreamingAssetPath(isLauncher), $"{fileName}.png").Replace("\\", "/");

            if (File.Exists(path))
            {
                byte[] imageBuff = await ReadFile(path);
                Texture2D tex = BinaryToTexture(imageBuff);

                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);

                return sprite;
            }
        }
        
        await Task.Yield();

        if (File.Exists(path))
        {
            byte[] imageBuff = await ReadFile(path);
            Texture2D tex;
            Sprite sprite = null;

            if (isCard)
            {
                tex = Texture2DExt.CreateTexture2DFromWebP(imageBuff, lMipmaps: true, lLinear: false, lError: out WebP.Error lError);

                if (lError == WebP.Error.Success)
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                }
                else
                {
                    Debug.LogError("Webp Load Error : " + lError.ToString());
                }
            }
            else
            {
                tex = BinaryToTexture(imageBuff);
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            }

            return sprite;
        }*/

        return null;
    }

    public static async Task<Sprite> GetSpriteImage(string fileName, bool isLauncher = false)
    {
        string path = Path.Combine(GetStreamingAssetPath(isLauncher), $"{fileName}.jpg").Replace("\\", "/");

        if(!File.Exists(path))
            path = Path.Combine(GetStreamingAssetPath(isLauncher), $"{fileName}.png").Replace("\\", "/");

        if (File.Exists(path))
        {
            byte[] imageBuff = await ReadFile(path);
            Texture2D tex = BinaryToTexture(imageBuff);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);

            return sprite;
        }

        return null;
    }

    public static async Task<Sprite> GetTokenImageData(string path)
    {
        if (File.Exists(path))
        {
            byte[] imageBuff = await ReadFile(path);
            Texture2D tex = BinaryToTexture(imageBuff);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            return sprite;
        }

        return null;
    }

    public static async Task<Sprite> GetCardImageDataLocal(string path)
    {
        if (File.Exists(path))
        {
            byte[] imageBuff = await ReadFile(path);
            Texture2D texture = Texture2DExt.CreateTexture2DFromWebP(imageBuff, lMipmaps: true, lLinear: false, lError: out WebP.Error lError);

            if (lError == WebP.Error.Success)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                return sprite;
            }
        }

        return null;
    }

    public static async Task<Sprite> GetCardImageData(string fileName, string filePath)
    {
        string urlPath = $"https://raw.githubusercontent.com/TakaOtaku/Digimon-Card-App/main/src/assets/images/cards/{fileName}.webp";

        UnityWebRequest webReq_CardImage = UnityWebRequest.Get(urlPath);
        UnityWebRequestAsyncOperation operation = webReq_CardImage.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }

        Debug.Log($"WebRequest isDone: {fileName}");
        if (webReq_CardImage.result == UnityWebRequest.Result.ConnectionError)
            return null;
        else if (webReq_CardImage.result == UnityWebRequest.Result.ProtocolError)
            return null;
        else
        {
            File.WriteAllBytes(filePath, webReq_CardImage.downloadHandler.data);

            Texture2D texture = Texture2DExt.CreateTexture2DFromWebP(webReq_CardImage.downloadHandler.data, lMipmaps: true, lLinear: false, lError: out WebP.Error lError);

            if (lError == WebP.Error.Success)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                return sprite;
            }

            return null;
        }
    }

    #endregion

    public static bool IsCardExists(CEntity_Base cEntity_Base)
    {
        string path = Path.Combine(GetStreamingAssetPath(false), $"Card/{cEntity_Base.CardSpriteName}.webp").Replace("\\", "/");

        if (cEntity_Base.CardSpriteName.Contains("token"))
            path = Path.Combine(GetStreamingAssetPath(false), $"Card/{cEntity_Base.CardSpriteName}.png").Replace("\\", "/");

        return File.Exists(path);
    }

    #region テキストファイルの取得
    public static string GetText(string fileName)
    {
        string path = Path.Combine(GetStreamingAssetPath(false), $"{fileName}.txt").Replace("\\", "/");

        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        return "";
    }
    #endregion

    static string GetStreamingAssetPath(bool isLauncher)
    {
        if (isLauncher)
        {
            string path = Application.streamingAssetsPath;

            path = GetOneUpperDirectoryPath(path);

            path = Path.Combine(path, $"Textures").Replace("\\", "/");

            return path;
        }

        else
        {
            string path = Application.streamingAssetsPath;

            path = GetOneUpperDirectoryPath(path);

            path = GetOneUpperDirectoryPath(path);

            path = Path.Combine(path, $"Textures").Replace("\\", "/");

            return path;
        }
    }

    static string GetOneUpperDirectoryPath(string path)
    {
        if (String.IsNullOrEmpty(path)) return "";
        path = path.Replace("\\", "/");
        if (!path.Contains("/")) return path;

        path = path.Substring(0, path.LastIndexOf("/") + 1);

        if (path.Length >= 1)
        {
            if (path[path.Length - 1] == '/')
            {
                path = path.Substring(0, path.LastIndexOf("/"));
            }
        }

        return path.Substring(0, path.LastIndexOf("/") + 1);
    }
}
