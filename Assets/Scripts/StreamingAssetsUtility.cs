
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Threading.Tasks;
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
        string path = Path.Combine(GetStreamingAssetPath(isLauncher), $"{fileName}.jpg").Replace("\\", "/");
        Debug.Log(path);
        if (!File.Exists(path))
        {
            path = Path.Combine(GetStreamingAssetPath(isLauncher), $"{fileName}.png").Replace("\\", "/");
        }

        if (isCard)
        {
            path = Path.Combine(GetStreamingAssetPath(isLauncher), $"Card/{fileName}.png").Replace("\\", "/");

            if (!File.Exists(path))
            {
                path = Path.Combine(GetStreamingAssetPath(isLauncher), $"Card/{fileName}.jpg").Replace("\\", "/");
            }
        }

        if (File.Exists(path))
        {
            byte[] imageBuff = await ReadFile(path);
            Texture2D tex = BinaryToTexture(imageBuff);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);

            return sprite;
        }

        return null;
    }
    #endregion

    public static bool IsCardExists(CEntity_Base cEntity_Base)
    {
        string path = Path.Combine(GetStreamingAssetPath(false), $"Card/{cEntity_Base.CardSpriteName}.png").Replace("\\", "/");

        if (File.Exists(path))
        {
            return true;
        }

        path = Path.Combine(GetStreamingAssetPath(false), $"Card/{cEntity_Base.CardSpriteName}.jpg").Replace("\\", "/");

        if (File.Exists(path))
        {
            return true;
        }

        return false;
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
