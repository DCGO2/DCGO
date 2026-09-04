using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WebP;

public class StreamingAssetsUtility
{
    static bool _seededBundledTextures;

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

    /// <summary>
    /// Reads bytes from a normal filesystem path, or from StreamingAssets on Android (APK / jar).
    /// </summary>
    public static async Task<byte[]> ReadBytesFlexible(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        if (File.Exists(path))
            return await ReadFile(path);

#if UNITY_ANDROID && !UNITY_EDITOR
        // APK StreamingAssets are not normal files — must use UnityWebRequest.
        if (path.Contains(Application.streamingAssetsPath.Replace("\\", "/")) ||
            path.StartsWith("jar:", StringComparison.OrdinalIgnoreCase))
        {
            return await ReadStreamingAssetsBytes(path);
        }
#endif
        return null;
    }

    static async Task<byte[]> ReadStreamingAssetsBytes(string urlOrPath)
    {
        string url = urlOrPath.Replace("\\", "/");
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
                return null;

            return req.downloadHandler.data;
        }
    }

    #region image load
    public static Texture2D BinaryToTexture(byte[] bytes)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);
        return texture;
    }

    public static async Task<Sprite> GetSprite(string fileName, bool isCard = false, bool isLauncher = false)
    {
        await EnsureBundledTexturesSeeded();

        if (isCard)
        {
            if (fileName.Contains("-token"))
            {
                return await GetTokenImageData(Path.Combine(GetStreamingAssetPath("Textures", isLauncher), $"Card/{fileName}.png").Replace("\\", "/"));
            }
            else
            {
                string path = Path.Combine(GetStreamingAssetPath("Textures", isLauncher), $"Card/{fileName}.webp").Replace("\\", "/");

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
    }

    public static async Task<Sprite> GetSpriteImage(string fileName, bool isLauncher = false)
    {
        await EnsureBundledTexturesSeeded();

        // Prefer writable cache (persistent on Android / Assets layout on PC).
        string path = Path.Combine(GetStreamingAssetPath("Textures", isLauncher), $"{fileName}.jpg").Replace("\\", "/");
        if (!File.Exists(path))
            path = Path.Combine(GetStreamingAssetPath("Textures", isLauncher), $"{fileName}.png").Replace("\\", "/");

        byte[] imageBuff = null;
        if (File.Exists(path))
            imageBuff = await ReadFile(path);

#if UNITY_ANDROID && !UNITY_EDITOR
        // Fall back to APK StreamingAssets if not yet on disk.
        if (imageBuff == null)
        {
            string saJpg = Path.Combine(Application.streamingAssetsPath, "Textures", $"{fileName}.jpg").Replace("\\", "/");
            string saPng = Path.Combine(Application.streamingAssetsPath, "Textures", $"{fileName}.png").Replace("\\", "/");
            imageBuff = await ReadStreamingAssetsBytes(saJpg);
            if (imageBuff == null)
                imageBuff = await ReadStreamingAssetsBytes(saPng);
        }
#endif

        if (imageBuff == null)
            return null;

        Texture2D tex = BinaryToTexture(imageBuff);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }

    public static async Task<Sprite> GetTokenImageData(string path)
    {
        byte[] imageBuff = await ReadBytesFlexible(path);
        if (imageBuff == null)
            return null;

        Texture2D tex = BinaryToTexture(imageBuff);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }

    public static async Task<Sprite> GetCardImageDataLocal(string path)
    {
        if (File.Exists(path))
        {
            Debug.Log($"File Exists Locally: {path}");
            byte[] imageBuff = await ReadFile(path);
            Debug.Log($"Grabbing image bytes: {imageBuff}");
            Texture2D texture = Texture2DExt.CreateTexture2DFromWebP(imageBuff, lMipmaps: true, lLinear: false, lError: out WebP.Error lError);
            Debug.Log($"Converting WebP to Texture2D: {texture}");
            if (lError == WebP.Error.Success)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                return sprite;
            }
            else
            {
                Debug.Log(lError.ToString());
            }
        }

        return null;
    }

    public static async Task<Sprite> GetCardImageData(string fileName, string filePath)
    {
        Sprite sprite;

        // Attempt to get the card image from repo
        sprite = await HandleCardImage(fileName, filePath);

        if (sprite != null) return sprite;
        else
        {

            // Attempt to get the card image from repo, this time with the sample suffix
            sprite = await HandleCardImage(fileName, filePath, isSample: true);

            if (sprite != null) return sprite;
            return null;
        }

    }

    public static async Task<Sprite> HandleCardImage(string fileName, string filePath, bool isSample = false)
    {
        string urlPath = $"https://raw.githubusercontent.com/TakaOtaku/Digimon-Card-App/main/src/assets/images/cards/{fileName}";
        if (isSample) urlPath += $"-Sample.webp";
        else urlPath += $".webp";

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
            Debug.Log($"WebRequest Successful: Checking local file - {File.Exists(filePath)}");
            if (!File.Exists(filePath))
            {
                EnsureParentDirectory(filePath);
                File.WriteAllBytes(filePath, webReq_CardImage.downloadHandler.data);
            }

            Texture2D texture = Texture2DExt.CreateTexture2DFromWebP(webReq_CardImage.downloadHandler.data, lMipmaps: true, lLinear: false, lError: out WebP.Error lError);

            if (lError == WebP.Error.Success)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                return sprite;
            }
            else Debug.Log($"Failed to convert: {lError.ToString()}");
            return null;
        }
    }

    #endregion

    public static bool IsCardExists(CEntity_Base cEntity_Base)
    {
        string path = Path.Combine(GetStreamingAssetPath("Textures", false), $"Card/{cEntity_Base.CardSpriteName}.webp").Replace("\\", "/");

        if (cEntity_Base.CardSpriteName.Contains("token"))
            path = Path.Combine(GetStreamingAssetPath("Textures", false), $"Card/{cEntity_Base.CardSpriteName}.png").Replace("\\", "/");

        return File.Exists(path);
    }

    #region text
    public static string GetText(string fileName)
    {
        string path = Path.Combine(GetStreamingAssetPath("", false), $"{fileName}.txt").Replace("\\", "/");

        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        return "";
    }
    #endregion

    /// <summary>
    /// Returns a writable root for decks/textures.
    /// Editor and standalone use the historic Assets-relative layout; Android/iOS use persistentDataPath.
    /// </summary>
    public static string GetStreamingAssetPath(string subPath, bool isLauncher)
    {
        if (UsePersistentDataRoot())
        {
            string path = Application.persistentDataPath;
            if (!string.IsNullOrEmpty(subPath))
                path = Path.Combine(path, subPath);

            path = path.Replace("\\", "/");
            EnsureDirectoryExists(path);
            return path;
        }

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

    static bool UsePersistentDataRoot()
    {
#if UNITY_EDITOR
        return false;
#elif UNITY_ANDROID || UNITY_IOS
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// Copy bundled StreamingAssets/Textures (UI, mats, card backs) into persistentDataPath once
    /// so File.Exists-based loaders work on Android.
    /// </summary>
    public static async Task EnsureBundledTexturesSeeded()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        await Task.Yield();
        return;
#else
        if (_seededBundledTextures)
            return;

        _seededBundledTextures = true;

        string marker = Path.Combine(Application.persistentDataPath, "Textures", ".seeded_ui_v1");
        if (File.Exists(marker))
            return;

        string[] relativeFiles = new[]
        {
            "Textures/Background_home.png",
            "Textures/Background_battle.png",
            "Textures/card_back_main.png",
            "Textures/card_back_sub.png",
            "Textures/PlayMat_You.png",
            "Textures/PlayMat_Opponent.png",
            "Textures/SecurityIcon_You.png",
            "Textures/SecurityIcon_Opponent.png",
            "Textures/CurrentPhaseBar_You.png",
            "Textures/CurrentPhaseBar_Opponent.png",
        };

        foreach (string relative in relativeFiles)
        {
            string dest = Path.Combine(Application.persistentDataPath, relative).Replace("\\", "/");
            if (File.Exists(dest))
                continue;

            string src = Path.Combine(Application.streamingAssetsPath, relative).Replace("\\", "/");
            byte[] data = await ReadStreamingAssetsBytes(src);
            if (data == null || data.Length == 0)
                continue;

            EnsureParentDirectory(dest);
            File.WriteAllBytes(dest, data);
            Debug.Log($"[StreamingAssetsUtility] Seeded {relative}");
        }

        // Seed UI/ and Backgrounds/ folders if present in the APK.
        await SeedDirectoryListingFallback();

        EnsureParentDirectory(marker);
        File.WriteAllText(marker, DateTime.UtcNow.ToString("o"));
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static async Task SeedDirectoryListingFallback()
    {
        // StreamingAssets on Android has no directory listing API.
        // Known Backgrounds / UI names can be added here if needed later.
        await Task.Yield();
    }
#endif

    public static void EnsureDirectoryExists(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    static void EnsureParentDirectory(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        string directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            EnsureDirectoryExists(directory);
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
