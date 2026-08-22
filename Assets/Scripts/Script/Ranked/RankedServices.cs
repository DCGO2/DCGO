using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton host for ranked/PlayFab services. Lives on ContinuousController scene when possible.
/// </summary>
public class RankedServices : MonoBehaviour
{
    public static RankedServices Instance { get; private set; }

    public PlayFabAuthService Auth { get; } = new PlayFabAuthService();
    public RankedProfileService Profile { get; } = new RankedProfileService();
    public RankedMatchService Match { get; } = new RankedMatchService();

    public bool IsReady => Auth != null && Auth.IsLoggedIn && Profile.Cached != null;

    [SerializeField] PlayFabConfigData configOverride;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (configOverride != null && !string.IsNullOrWhiteSpace(configOverride.titleId))
        {
            PlayFabConfig.Override(configOverride);
        }

        DontDestroyOnLoad(gameObject);
    }

    public static RankedServices EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        var go = new GameObject("RankedServices");
        return go.AddComponent<RankedServices>();
    }

    public IEnumerator BootstrapForRanked(System.Action<bool, string> onComplete = null)
    {
        EnsureExists();
        bool ok = false;
        string error = null;

        yield return Auth.EnsureLoggedIn((success, err) =>
        {
            ok = success;
            error = err;
        });

        if (!ok)
        {
            onComplete?.Invoke(false, error);
            yield break;
        }

        yield return Profile.Refresh(Auth, (profile, err) =>
        {
            if (profile == null)
            {
                ok = false;
                error = err ?? "Failed to load ranked profile";
            }
        });

        onComplete?.Invoke(ok, error);
    }
}
