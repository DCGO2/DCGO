using UnityEngine;

/// <summary>
/// Persistent host for friend list + direct duel services.
/// </summary>
public class FriendServices : MonoBehaviour
{
    public static FriendServices Instance { get; private set; }

    public FriendListService List { get; } = new FriendListService();
    public FriendDuelService Duel { get; private set; }
    public FriendDuelDirector Director { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        List.LoadLocal();

        Duel = GetComponent<FriendDuelService>();
        if (Duel == null)
        {
            Duel = gameObject.AddComponent<FriendDuelService>();
        }

        Director = GetComponent<FriendDuelDirector>();
        if (Director == null)
        {
            Director = gameObject.AddComponent<FriendDuelDirector>();
        }
    }

    public static FriendServices EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        var go = new GameObject("FriendServices");
        return go.AddComponent<FriendServices>();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
