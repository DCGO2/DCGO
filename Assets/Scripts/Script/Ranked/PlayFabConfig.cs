using System;
using UnityEngine;

/// <summary>
/// PlayFab client config. Place a JSON file at Resources/Ranked/PlayFabConfig.json
/// or assign values on the RankedServices component.
/// </summary>
[Serializable]
public class PlayFabConfigData
{
    [Tooltip("PlayFab Title ID from the Game Manager dashboard (not a secret).")]
    public string titleId = "";

    [Tooltip("When true and TitleId is empty, ranked uses local offline authority for development.")]
    public bool allowOfflineFallback = true;

    [Tooltip("Use PlayFab Photon custom auth token when connecting (requires Photon dashboard PlayFab auth).")]
    public bool usePhotonCustomAuth = true;

    public bool HasTitleId => !string.IsNullOrWhiteSpace(titleId);
}

public static class PlayFabConfig
{
    const string ResourcesPath = "Ranked/PlayFabConfig";

    static PlayFabConfigData _cached;

    public static PlayFabConfigData Current
    {
        get
        {
            if (_cached != null)
            {
                return _cached;
            }

            var asset = Resources.Load<TextAsset>(ResourcesPath);
            if (asset != null && !string.IsNullOrWhiteSpace(asset.text))
            {
                try
                {
                    _cached = JsonUtility.FromJson<PlayFabConfigData>(asset.text);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[PlayFab] Failed to parse config: {e.Message}");
                }
            }

            if (_cached == null)
            {
                _cached = new PlayFabConfigData();
            }

            if (!string.IsNullOrEmpty(_cached.titleId))
            {
                _cached.titleId = _cached.titleId.Trim();
            }

            Debug.Log(
                $"[PlayFab] Config loaded titleId='{_cached.titleId}' " +
                $"allowOfflineFallback={_cached.allowOfflineFallback} " +
                $"usePhotonCustomAuth={_cached.usePhotonCustomAuth}");

            return _cached;
        }
    }

    public static void Override(PlayFabConfigData data)
    {
        if (data != null)
        {
            if (!string.IsNullOrEmpty(data.titleId))
            {
                data.titleId = data.titleId.Trim();
            }

            _cached = data;
        }
    }
}
