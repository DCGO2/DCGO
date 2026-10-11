using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WebP;

public class StreamingAssetsUtility
{
    public static async Task<byte[]> ReadFile(string path)
    {
        using FileStream fileStream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read);

        if (fileStream.Length > int.MaxValue)
            throw new IOException($"File is too large to read: {path}");

        byte[] resultBytes = new byte[(int)fileStream.Length];
        int offset = 0;

        while (offset < resultBytes.Length)
        {
            int bytesRead = await fileStream.ReadAsync(
                resultBytes, offset, resultBytes.Length - offset);

            if (bytesRead == 0)
                throw new EndOfStreamException($"Unexpected end of file: {path}");

            offset += bytesRead;
        }

        return resultBytes;
    }
    public static Texture2D BinaryToTexture(byte[] bytes)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);
        return texture;
    }

    public static async Task<Sprite> GetSprite(string fileName, bool isCard = false, bool isLauncher = false)
    {
        string path;

        if (isCard)
        {
            if (fileName.Contains("-token"))
            {
                return await GetTokenImageData(Path.Combine(GetStreamingAssetPath("Textures", isLauncher), $"Card/{fileName}.png").Replace("\\", "/"));
            }
            else
            {
                path = Path.Combine(GetStreamingAssetPath("Textures", isLauncher), $"Card/{fileName}.webp").Replace("\\", "/");

                Sprite sprite = null;

                if (File.Exists(path))
                    sprite = await GetCardImageDataLocal(path);

                if (sprite != null)
                    return sprite;

                // Also retry downloading if the local file exists but is invalid.
                return await GetCardImageData(fileName, path);
            }
        }
        else
        {
            return await GetSpriteImage(fileName, isLauncher);
        }
    }

    public static async Task<Sprite> GetSpriteImage(string fileName, bool isLauncher = false)
    {
        string path = Path.Combine(GetStreamingAssetPath("Textures", isLauncher), $"{fileName}.jpg").Replace("\\", "/");

        if (!File.Exists(path))
            path = Path.Combine(GetStreamingAssetPath("Textures", isLauncher), $"{fileName}.png").Replace("\\", "/");

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
        if (!File.Exists(path))
            return null;

        byte[] imageBuff = await ReadFile(path);

        if (!IsValidWebP(imageBuff))
        {
            Debug.LogWarning($"Invalid local WebP file: {path}. Will attempt to download it again.");
            return null;
        }

        try
        {
            Texture2D texture = Texture2DExt.CreateTexture2DFromWebP(
                imageBuff,
                lMipmaps: true,
                lLinear: false,
                lError: out WebP.Error lError);

            if (lError != WebP.Error.Success || texture == null)
            {
                Debug.LogWarning($"Failed to decode local WebP: {path}. Error: {lError}");
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.zero);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Exception decoding local WebP: {path}\n{ex.Message}");
            return null;
        }
    }

    private static bool IsValidWebP(byte[] data)
    {
        if (data == null || data.Length < 12)
            return false;

        return data[0] == (byte)'R'
            && data[1] == (byte)'I'
            && data[2] == (byte)'F'
            && data[3] == (byte)'F'
            && data[8] == (byte)'W'
            && data[9] == (byte)'E'
            && data[10] == (byte)'B'
            && data[11] == (byte)'P';
    }

    public static async Task<Sprite> GetCardImageData(string fileName, string filePath)
    {
        Sprite sprite = await HandleCardImage(fileName, filePath);

        if (sprite != null)
            return sprite;

        return await HandleCardImage(fileName, filePath, isSample: true);
    }

    public static async Task<Sprite> HandleCardImage(
        string fileName,
        string filePath,
        bool isSample = false,
        bool isBackup = false)
    {
        string urlPath = isBackup
            ? $"https://web-garage.takaotaku.de/{fileName}"
            : $"https://raw.githubusercontent.com/Facundo-Villafane/dcg-cards/main/cards/{fileName}";

        urlPath += isSample ? "-Sample.webp" : ".webp";

        using UnityWebRequest request = UnityWebRequest.Get(urlPath);
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        byte[] data = request.downloadHandler?.data;

        Debug.Log(
            $"Image request: {urlPath}\n" +
            $"Result: {request.result}\n" +
            $"HTTP status: {request.responseCode}\n" +
            $"Bytes received: {(data == null ? 0 : data.Length)}");

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Image request failed: {urlPath}\n{request.error}");

            if (!isBackup)
                return await HandleCardImage(fileName, filePath, isSample, true);

            return null;
        }

        if (!IsValidWebP(data))
        {
            Debug.LogWarning($"Response is not a valid WebP file: {urlPath}");

            if (!isBackup)
                return await HandleCardImage(fileName, filePath, isSample, true);

            return null;
        }

        try
        {
            Texture2D texture = Texture2DExt.CreateTexture2DFromWebP(
                data,
                lMipmaps: true,
                lLinear: false,
                lError: out WebP.Error lError);

            if (lError != WebP.Error.Success || texture == null)
            {
                Debug.LogWarning($"WebP decoding failed: {urlPath}. Error: {lError}");

                if (!isBackup)
                    return await HandleCardImage(fileName, filePath, isSample, true);

                return null;
            }

            // Save only after confirming the response can be decoded.
            string directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(filePath, data);

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.zero);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Exception decoding WebP from {urlPath}\n{ex}");

            if (!isBackup)
                return await HandleCardImage(fileName, filePath, isSample, true);

            return null;
        }
    }


    public static bool IsCardExists(CEntity_Base cEntity_Base)
    {
        string path = Path.Combine(GetStreamingAssetPath("Textures", false), $"Card/{cEntity_Base.CardSpriteName}.webp").Replace("\\", "/");

        if (cEntity_Base.CardSpriteName.Contains("token"))
            path = Path.Combine(GetStreamingAssetPath("Textures", false), $"Card/{cEntity_Base.CardSpriteName}.png").Replace("\\", "/");

        return File.Exists(path);
    }

    public static string GetText(string fileName)
    {
        string path = Path.Combine(GetStreamingAssetPath("", false), $"{fileName}.txt").Replace("\\", "/");

        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        return "";
    }

    public static string GetStreamingAssetPath(string subPath, bool isLauncher)
    {
        if (isLauncher)
        {
            string path = Application.streamingAssetsPath;

            path = GetOneUpperDirectoryPath(path);

            path = Path.Combine(path, $"Assets/{subPath}").Replace("\\", "/");

            return path;
        }

        else
        {
            string path = Application.streamingAssetsPath;

            path = GetOneUpperDirectoryPath(path);

            path = GetOneUpperDirectoryPath(path);

            path = Path.Combine(path, $"Assets/{subPath}").Replace("\\", "/");

            return path;
        }
    }

    static string GetOneUpperDirectoryPath(string path)
    {
        if (String.IsNullOrEmpty(path)) return "";
        path = path.Replace("\\", "/");
        if (!path.Contains("/")) return path;

        path = path[..(path.LastIndexOf("/") + 1)];

        if (path.Length >= 1)
        {
            if (path[path.Length - 1] == '/')
            {
                path = path[..path.LastIndexOf("/")];
            }
        }

        return path[..(path.LastIndexOf("/") + 1)];
    }
}
