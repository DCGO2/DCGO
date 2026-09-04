using UnityEngine;

/// <summary>
/// Persistent host for tournament match routing. Survives BattleScene unload.
/// </summary>
public class TournamentServices : MonoBehaviour
{
    public static TournamentServices Instance { get; private set; }

    public TournamentMatchDirector Match { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Match = GetComponent<TournamentMatchDirector>();
        if (Match == null)
        {
            Match = gameObject.AddComponent<TournamentMatchDirector>();
        }
    }

    public static TournamentServices EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        var go = new GameObject("TournamentServices");
        return go.AddComponent<TournamentServices>();
    }
}
