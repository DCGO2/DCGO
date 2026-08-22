using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// <summary>
/// PlayFab login (device CustomId) and optional Photon custom authentication token.
/// Guest CustomId is deterministic from package + device so reinstall on the same device
/// recovers the same PlayFab player (no random Guid). Existing PlayerPrefs ids are kept.
/// After first online login, a private recovery CustomId is linked so a phone wipe
/// (new device fingerprint) can restore the same account via a write-down code.
/// </summary>
public class PlayFabAuthService
{
    const string DeviceIdPrefKey = "RankedPlayFabCustomId";
    const string OfflineIdPrefKey = "RankedOfflinePlayerId";
    const string RecoveryCodePrefKey = "RankedRecoveryCode";
    const string RecoveryUserDataKey = "DCGO_RecoveryCode";
    const string RecoveryCustomIdPrefix = "dcgo-rc-";

    /// <summary>Crockford Base32 without I,L,O,U — easier to read aloud / write down.</summary>
    const string CrockfordAlphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    public string PlayFabId { get; private set; }
    public string PhotonAuthToken { get; private set; }
    public bool IsOfflineMode { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(PlayFabId);

    /// <summary>Formatted recovery code (XXXX-XXXX-XXXX), or null if not yet available.</summary>
    public string RecoveryCodeDisplay { get; private set; }

    /// <summary>
    /// PlayFab LoginWithCustomID guest id. Cached in PlayerPrefs when present;
    /// after wipe/reinstall, recomputed from device (stable v2, no per-install Guid).
    /// </summary>
    public string GetOrCreateCustomId()
    {
        if (PlayerPrefs.HasKey(DeviceIdPrefKey))
        {
            string existing = PlayerPrefs.GetString(DeviceIdPrefKey);
            if (!string.IsNullOrEmpty(existing))
            {
                Debug.Log("[Ranked] Using cached PlayFab CustomId from PlayerPrefs");
                return existing;
            }
        }

        string id = BuildStableCustomId(onlineGuest: true);
        PlayerPrefs.SetString(DeviceIdPrefKey, id);
        PlayerPrefs.Save();
        Debug.Log("[Ranked] Computed stable PlayFab CustomId from package+device (reinstall-safe)");
        return id;
    }

    public IEnumerator EnsureLoggedIn(Action<bool, string> onComplete = null)
    {
        var config = PlayFabConfig.Current;

        if (!config.HasTitleId)
        {
            if (!config.allowOfflineFallback)
            {
                onComplete?.Invoke(false, "PlayFab TitleId is not configured.");
                yield break;
            }

            IsOfflineMode = true;
            PlayFabId = GetOrCreateOfflineId();
            PlayFabClientApi.ClearSession();
            RecoveryCodeDisplay = null;
            Debug.Log("[Ranked] Offline PlayFab fallback active (set Resources/Ranked/PlayFabConfig.json titleId for production).");
            onComplete?.Invoke(true, null);
            yield break;
        }

        IsOfflineMode = false;

        if (PlayFabClientApi.IsLoggedIn && PlayFabClientApi.PlayFabId == PlayFabId && !string.IsNullOrEmpty(PlayFabId))
        {
            if (string.IsNullOrEmpty(RecoveryCodeDisplay))
            {
                yield return EnsureRecoveryCodeAttached();
            }

            onComplete?.Invoke(true, null);
            yield break;
        }

        bool done = false;
        bool ok = false;
        string error = null;

        string customId = GetOrCreateCustomId();

        yield return PlayFabClientApi.LoginWithCustomId(
            config.titleId,
            customId,
            true,
            result =>
            {
                ok = result.success;
                error = result.errorMessage;
                if (ok)
                {
                    PlayFabId = PlayFabClientApi.PlayFabId;
                }

                done = true;
            });

        while (!done)
        {
            yield return null;
        }

        if (!ok)
        {
            string customIdPreview = customId != null && customId.Length > 12
                ? customId.Substring(0, 12) + "…"
                : customId;

            if (config.allowOfflineFallback)
            {
                string tip = "Check Game Manager → Settings → API Features: " +
                    "Allow Login with Custom ID + Allow client to create new users.";
                if (!string.IsNullOrEmpty(error) &&
                    (error.IndexOf("PlayerCreationDisabled", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                     error.IndexOf("Player creations have been disabled", System.StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    tip =
                        "PlayFab error PlayerCreationDisabled: enable \"Allow client to create new users\" " +
                        "(Settings → API Features) for this title, then restart Play Mode.";
                }

                Debug.LogError(
                    $"[Ranked] PlayFab login failed; using offline fallback. " +
                    $"titleId={config.titleId} customId={customIdPreview} error={error}. " +
                    "Ranked will NOT update PlayFab players/statistics until login succeeds. " +
                    tip);
                IsOfflineMode = true;
                PlayFabId = GetOrCreateOfflineId();
                RecoveryCodeDisplay = null;
                onComplete?.Invoke(true, null);
                yield break;
            }

            onComplete?.Invoke(false, error ?? "PlayFab login failed");
            yield break;
        }

        Debug.Log($"[Ranked] PlayFab login OK. playFabId={PlayFabId} titleId={config.titleId}");

        // Best-effort display name sync
        if (!string.IsNullOrEmpty(ContinuousController.instance?.PlayerName))
        {
            bool nameDone = false;
            yield return PlayFabClientApi.UpdateDisplayName(
                config.titleId,
                ContinuousController.instance.PlayerName,
                _ => { nameDone = true; });
            while (!nameDone) yield return null;
        }

        yield return EnsureRecoveryCodeAttached();

        if (config.usePhotonCustomAuth)
        {
            string photonAppId = PhotonNetwork.PhotonServerSettings?.AppSettings?.AppIdRealtime;
            if (!string.IsNullOrEmpty(photonAppId))
            {
                bool tokenDone = false;
                yield return PlayFabClientApi.GetPhotonAuthenticationToken(
                    config.titleId,
                    photonAppId,
                    (result, token) =>
                    {
                        if (result.success)
                        {
                            PhotonAuthToken = token;
                        }
                        else
                        {
                            Debug.LogWarning($"[Ranked] GetPhotonAuthenticationToken failed: {result.errorMessage}");
                        }

                        tokenDone = true;
                    });
                while (!tokenDone) yield return null;
            }
        }

        onComplete?.Invoke(true, null);
    }

    /// <summary>
    /// Ensure this PlayFab player has a write-down recovery code.
    /// Uses a second linked CustomId (dcgo-rc-…) plus UserData — works with the same
    /// Custom ID API Features already required for ranked guest login (no username/password).
    /// </summary>
    public IEnumerator EnsureRecoveryCodeAttached(Action<bool, string> onComplete = null)
    {
        if (IsOfflineMode || !PlayFabClientApi.IsLoggedIn)
        {
            RecoveryCodeDisplay = null;
            onComplete?.Invoke(false, "Not logged in online");
            yield break;
        }

        var config = PlayFabConfig.Current;
        if (!config.HasTitleId)
        {
            onComplete?.Invoke(false, "No title id");
            yield break;
        }

        // Prefer cached code from this install
        if (PlayerPrefs.HasKey(RecoveryCodePrefKey))
        {
            string cached = PlayerPrefs.GetString(RecoveryCodePrefKey);
            string norm = NormalizeRecoveryCode(cached);
            if (IsValidNormalizedCode(norm))
            {
                RecoveryCodeDisplay = FormatRecoveryCode(norm);
                onComplete?.Invoke(true, null);
                yield break;
            }
        }

        // Same-device reinstall: UserData still has the code after CustomId login
        bool dataDone = false;
        bool dataOk = false;
        string storedCode = null;
        string dataError = null;

        yield return PlayFabClientApi.GetUserData(
            config.titleId,
            new List<string> { RecoveryUserDataKey },
            (result, map) =>
            {
                dataOk = result.success;
                dataError = result.errorMessage;
                if (map != null)
                {
                    map.TryGetValue(RecoveryUserDataKey, out storedCode);
                }

                dataDone = true;
            });

        while (!dataDone) yield return null;

        if (dataOk && IsValidNormalizedCode(NormalizeRecoveryCode(storedCode)))
        {
            CacheRecoveryCode(NormalizeRecoveryCode(storedCode));
            onComplete?.Invoke(true, null);
            yield break;
        }

        if (!dataOk)
        {
            Debug.LogWarning($"[Ranked] GetUserData failed while loading recovery code: {dataError}");
        }

        // No code yet — generate, LinkCustomID(dcgo-rc-…), store in UserData
        const int maxAttempts = 5;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            string norm = GenerateNormalizedRecoveryCode();
            string recoveryCustomId = RecoveryCustomIdPrefix + norm;

            bool linkDone = false;
            bool linkOk = false;
            string linkError = null;

            yield return PlayFabClientApi.LinkCustomId(
                config.titleId,
                recoveryCustomId,
                false,
                result =>
                {
                    linkOk = result.success;
                    linkError = result.errorMessage;
                    linkDone = true;
                });

            while (!linkDone) yield return null;

            if (!linkOk)
            {
                bool collision = !string.IsNullOrEmpty(linkError) &&
                    (linkError.IndexOf("LinkedIdentifierAlreadyClaimed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     linkError.IndexOf("already assigned", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     linkError.IndexOf("AlreadyLinked", StringComparison.OrdinalIgnoreCase) >= 0 &&
                     linkError.IndexOf("AccountAlreadyLinked", StringComparison.OrdinalIgnoreCase) < 0);

                // Already linked to this same account — reuse this code
                bool alreadyOurs = !string.IsNullOrEmpty(linkError) &&
                    linkError.IndexOf("AccountAlreadyLinked", StringComparison.OrdinalIgnoreCase) >= 0;

                if (alreadyOurs)
                {
                    yield return PersistRecoveryCode(config.titleId, norm, onComplete);
                    yield break;
                }

                if (collision)
                {
                    continue; // try another code
                }

                Debug.LogWarning($"[Ranked] LinkCustomID (recovery) failed: {linkError}");
                onComplete?.Invoke(false, FormatRecoveryAttachError(linkError));
                yield break;
            }

            yield return PersistRecoveryCode(config.titleId, norm, onComplete);
            yield break;
        }

        onComplete?.Invoke(false, "Could not allocate a unique recovery code");
    }

    IEnumerator PersistRecoveryCode(string titleId, string normalized, Action<bool, string> onComplete)
    {
        string formatted = FormatRecoveryCode(normalized);
        bool saveDone = false;
        bool saveOk = false;
        string saveError = null;

        yield return PlayFabClientApi.UpdateUserData(
            titleId,
            new Dictionary<string, string> { { RecoveryUserDataKey, formatted } },
            result =>
            {
                saveOk = result.success;
                saveError = result.errorMessage;
                saveDone = true;
            });

        while (!saveDone) yield return null;

        if (!saveOk)
        {
            Debug.LogWarning($"[Ranked] UpdateUserData (recovery code) failed: {saveError}");
            // CustomId link already succeeded — still usable for recover; cache locally
            CacheRecoveryCode(normalized);
            onComplete?.Invoke(true, null);
            yield break;
        }

        CacheRecoveryCode(normalized);
        Debug.Log("[Ranked] Recovery code attached to PlayFab account");
        onComplete?.Invoke(true, null);
    }

    static string FormatRecoveryAttachError(string playFabError)
    {
        if (!string.IsNullOrEmpty(playFabError) &&
            (playFabError.IndexOf("not enabled", StringComparison.OrdinalIgnoreCase) >= 0 ||
             playFabError.IndexOf("NotAllowed", StringComparison.OrdinalIgnoreCase) >= 0 ||
             playFabError.IndexOf("APINotEnabled", StringComparison.OrdinalIgnoreCase) >= 0))
        {
            return LocalizeUtility.GetLocalizedString(
                EngMessage: "PlayFab: enable Custom ID linking (API Features), then reopen Account.",
                JpnMessage: "PlayFabのAPI FeaturesでCustom IDのリンクを有効にしてください。");
        }

        return playFabError ?? LocalizeUtility.GetLocalizedString(
            EngMessage: "Failed to create recovery code.",
            JpnMessage: "リカバリーコードの作成に失敗しました。");
    }

    /// <summary>
    /// Restore a previous PlayFab account using a write-down recovery code, then ForceLink
    /// this device's CustomId so future silent logins use the recovered player.
    /// </summary>
    public IEnumerator RecoverWithCode(string inputCode, Action<bool, string> onComplete = null)
    {
        var config = PlayFabConfig.Current;
        if (!config.HasTitleId)
        {
            onComplete?.Invoke(false, "PlayFab TitleId is not configured.");
            yield break;
        }

        string norm = NormalizeRecoveryCode(inputCode);
        if (!IsValidNormalizedCode(norm))
        {
            onComplete?.Invoke(false, LocalizeUtility.GetLocalizedString(
                EngMessage: "Invalid recovery code.",
                JpnMessage: "リカバリーコードが無効です。"));
            yield break;
        }

        string recoveryCustomId = RecoveryCustomIdPrefix + norm;
        string deviceCustomId = GetOrCreateCustomId();

        bool loginDone = false;
        bool loginOk = false;
        string loginError = null;

        yield return PlayFabClientApi.LoginWithCustomId(
            config.titleId,
            recoveryCustomId,
            false,
            result =>
            {
                loginOk = result.success;
                loginError = result.errorMessage;
                loginDone = true;
            });

        while (!loginDone) yield return null;

        if (!loginOk)
        {
            onComplete?.Invoke(false, LocalizeUtility.GetLocalizedString(
                EngMessage: "Recovery failed. Check the code and try again.",
                JpnMessage: "リカバリーに失敗しました。コードを確認してください。")
                + (string.IsNullOrEmpty(loginError) ? "" : $" ({loginError})"));
            yield break;
        }

        IsOfflineMode = false;
        PlayFabId = PlayFabClientApi.PlayFabId;
        PhotonAuthToken = null;

        // Bind this phone's guest CustomId to the recovered account
        bool linkDone = false;
        bool linkOk = false;
        string linkError = null;

        yield return PlayFabClientApi.LinkCustomId(
            config.titleId,
            deviceCustomId,
            true,
            result =>
            {
                linkOk = result.success;
                linkError = result.errorMessage;
                linkDone = true;
            });

        while (!linkDone) yield return null;

        if (!linkOk)
        {
            Debug.LogWarning($"[Ranked] LinkCustomID after recovery failed: {linkError}");
        }
        else
        {
            PlayerPrefs.SetString(DeviceIdPrefKey, deviceCustomId);
            PlayerPrefs.Save();
        }

        CacheRecoveryCode(norm);

        // Restore display name from PlayFab
        bool profileDone = false;
        string displayName = null;
        yield return PlayFabClientApi.GetPlayerProfile(
            config.titleId,
            PlayFabId,
            (result, name) =>
            {
                if (result.success)
                {
                    displayName = name;
                }

                profileDone = true;
            });
        while (!profileDone) yield return null;

        if (!string.IsNullOrEmpty(displayName) && ContinuousController.instance != null)
        {
            ContinuousController.instance.SavePlayerName(displayName);
        }

        ApplyPhotonAuthValues();
        if (PhotonNetwork.IsConnected && ContinuousController.instance != null)
        {
            yield return ContinuousController.instance.StartCoroutine(PhotonUtility.DisconnectCoroutine());
        }

        Debug.Log($"[Ranked] Account recovered. playFabId={PlayFabId}");
        onComplete?.Invoke(true, null);
    }

    public void ApplyPhotonAuthValues()
    {
        var config = PlayFabConfig.Current;
        if (IsOfflineMode || !config.usePhotonCustomAuth || string.IsNullOrEmpty(PhotonAuthToken) || string.IsNullOrEmpty(PlayFabId))
        {
            PhotonNetwork.AuthValues = new AuthenticationValues(PlayFabId ?? ContinuousController.instance.PlayerName);
            return;
        }

        var auth = new AuthenticationValues
        {
            AuthType = CustomAuthenticationType.Custom,
            UserId = PlayFabId,
        };
        auth.AddAuthParameter("username", PlayFabId);
        auth.AddAuthParameter("token", PhotonAuthToken);
        PhotonNetwork.AuthValues = auth;
    }

    void CacheRecoveryCode(string normalized)
    {
        RecoveryCodeDisplay = FormatRecoveryCode(normalized);
        PlayerPrefs.SetString(RecoveryCodePrefKey, RecoveryCodeDisplay);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Offline mode id: same device-stable hash as online guest, with offline- prefix,
    /// so local Elo also recovers after reinstall on the same device.
    /// </summary>
    string GetOrCreateOfflineId()
    {
        if (PlayerPrefs.HasKey(OfflineIdPrefKey))
        {
            string id = PlayerPrefs.GetString(OfflineIdPrefKey);
            if (!string.IsNullOrEmpty(id))
            {
                Debug.Log("[Ranked] Using cached offline player id from PlayerPrefs");
                return id;
            }
        }

        string created = BuildStableCustomId(onlineGuest: false);
        PlayerPrefs.SetString(OfflineIdPrefKey, created);
        PlayerPrefs.Save();
        Debug.Log("[Ranked] Computed stable offline player id from package+device (reinstall-safe)");
        return created;
    }

    /// <summary>
    /// Deterministic guest id: package + device fingerprint, no per-install Guid.
    /// Format: dcgo-v2-/offline-v2- + 32 hex chars (SHA256 truncated).
    /// </summary>
    static string BuildStableCustomId(bool onlineGuest)
    {
        string package = Application.identifier;
        if (string.IsNullOrEmpty(package))
        {
            package = Application.productName ?? "dcgo";
        }

        string device = GetStableDeviceFingerprint();
        string material = $"{package}|{device}";
        string hash32 = Sha256HexPrefix(material, 32);
        string prefix = onlineGuest ? "dcgo-v2-" : "offline-v2-";
        return prefix + hash32;
    }

    static string GetStableDeviceFingerprint()
    {
        string duid = SystemInfo.deviceUniqueIdentifier;
        if (!string.IsNullOrEmpty(duid) &&
            duid != SystemInfo.unsupportedIdentifier)
        {
            return duid;
        }

#if UNITY_EDITOR
        // Editor has no reliable DUID — machine + device name is good enough for local testing.
        return $"editor|{SystemInfo.deviceName}|{Environment.MachineName}|{Environment.UserName}";
#else
        // Rare store-build path when DUID unsupported: degrade but still deterministic for the session hardware.
        return $"fallback|{SystemInfo.deviceModel}|{SystemInfo.processorType}|{SystemInfo.systemMemorySize}";
#endif
    }

    static string Sha256HexPrefix(string input, int hexCharCount)
    {
        using (var sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(hexCharCount);
            for (int i = 0; i < bytes.Length && sb.Length < hexCharCount; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }

            // Ensure exact length requested (already 64 max from full hash)
            if (sb.Length > hexCharCount)
            {
                return sb.ToString(0, hexCharCount);
            }

            return sb.ToString();
        }
    }

    // --- Recovery code helpers ---

    public static string NormalizeRecoveryCode(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        var sb = new StringBuilder(12);
        foreach (char c in input)
        {
            if (c == '-' || c == ' ' || c == '_')
            {
                continue;
            }

            char u = char.ToUpperInvariant(c);
            // Map ambiguous glyphs to Crockford equivalents
            if (u == 'I' || u == 'L') u = '1';
            else if (u == 'O') u = '0';
            else if (u == 'U') u = 'V';

            if (CrockfordAlphabet.IndexOf(u) >= 0)
            {
                sb.Append(u);
            }
        }

        return sb.ToString();
    }

    public static string FormatRecoveryCode(string normalized)
    {
        if (string.IsNullOrEmpty(normalized) || normalized.Length != 12)
        {
            return normalized;
        }

        return $"{normalized.Substring(0, 4)}-{normalized.Substring(4, 4)}-{normalized.Substring(8, 4)}";
    }

    public static bool IsValidNormalizedCode(string normalized)
    {
        if (string.IsNullOrEmpty(normalized) || normalized.Length != 12)
        {
            return false;
        }

        for (int i = 0; i < normalized.Length; i++)
        {
            if (CrockfordAlphabet.IndexOf(normalized[i]) < 0)
            {
                return false;
            }
        }

        return true;
    }

    static string GenerateNormalizedRecoveryCode()
    {
        var bytes = new byte[12];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        var sb = new StringBuilder(12);
        for (int i = 0; i < 12; i++)
        {
            sb.Append(CrockfordAlphabet[bytes[i] % CrockfordAlphabet.Length]);
        }

        return sb.ToString();
    }
}
