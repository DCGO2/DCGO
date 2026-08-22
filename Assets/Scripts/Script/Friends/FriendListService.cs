using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FriendEntry
{
    public string playFabId;
    public string displayName;

    public FriendEntry() { }

    public FriendEntry(string id, string name)
    {
        playFabId = id;
        displayName = name;
    }
}

[Serializable]
class FriendEntryListWrapper
{
    public List<FriendEntry> friends = new List<FriendEntry>();
}

/// <summary>
/// Local + PlayFab friend list storage. PlayFab is best-effort; PlayerPrefs is always updated.
/// </summary>
public class FriendListService
{
    readonly List<FriendEntry> _friends = new List<FriendEntry>();

    public IReadOnlyList<FriendEntry> Friends => _friends;

    public string LastOpponentPlayFabId { get; private set; }
    public string LastOpponentDisplayName { get; private set; }

    public event Action Changed;

    public void LoadLocal()
    {
        _friends.Clear();
        if (PlayerPrefs.HasKey(FriendKeys.LocalFriendsPrefsKey))
        {
            string json = PlayerPrefs.GetString(FriendKeys.LocalFriendsPrefsKey);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var wrap = JsonUtility.FromJson<FriendEntryListWrapper>(json);
                    if (wrap?.friends != null)
                    {
                        foreach (var f in wrap.friends)
                        {
                            if (f != null && !string.IsNullOrEmpty(f.playFabId))
                            {
                                _friends.Add(f);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Friends] Failed to parse local list: {e.Message}");
                }
            }
        }

        LastOpponentPlayFabId = PlayerPrefs.GetString(FriendKeys.LastOpponentIdPrefsKey, null);
        LastOpponentDisplayName = PlayerPrefs.GetString(FriendKeys.LastOpponentNamePrefsKey, null);
        if (string.IsNullOrEmpty(LastOpponentPlayFabId))
        {
            LastOpponentPlayFabId = null;
            LastOpponentDisplayName = null;
        }

        Changed?.Invoke();
    }

    void SaveLocal()
    {
        var wrap = new FriendEntryListWrapper { friends = new List<FriendEntry>(_friends) };
        PlayerPrefs.SetString(FriendKeys.LocalFriendsPrefsKey, JsonUtility.ToJson(wrap));
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public bool Contains(string playFabId)
    {
        if (string.IsNullOrEmpty(playFabId))
        {
            return false;
        }

        for (int i = 0; i < _friends.Count; i++)
        {
            if (string.Equals(_friends[i].playFabId, playFabId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void RememberLastOpponent(string playFabId, string displayName)
    {
        if (string.IsNullOrEmpty(playFabId))
        {
            return;
        }

        string localId = LocalPlayFabId();
        if (!string.IsNullOrEmpty(localId) &&
            string.Equals(localId, playFabId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        LastOpponentPlayFabId = playFabId;
        LastOpponentDisplayName = string.IsNullOrEmpty(displayName) ? playFabId : displayName;
        PlayerPrefs.SetString(FriendKeys.LastOpponentIdPrefsKey, LastOpponentPlayFabId);
        PlayerPrefs.SetString(FriendKeys.LastOpponentNamePrefsKey, LastOpponentDisplayName);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static string LocalPlayFabId()
    {
        var ranked = RankedServices.Instance ?? RankedServices.EnsureExists();
        if (ranked?.Auth != null && !string.IsNullOrEmpty(ranked.Auth.PlayFabId))
        {
            return ranked.Auth.PlayFabId;
        }

        return Photon.Pun.PhotonNetwork.LocalPlayer?.UserId;
    }

    public IEnumerator EnsureLoggedIn()
    {
        var ranked = RankedServices.EnsureExists();
        if (ranked.Auth.IsLoggedIn)
        {
            yield break;
        }

        yield return ranked.Auth.EnsureLoggedIn();
    }

    public IEnumerator RefreshFromPlayFab(Action<bool, string> onComplete = null)
    {
        LoadLocal();
        yield return EnsureLoggedIn();

        var config = PlayFabConfig.Current;
        var ranked = RankedServices.EnsureExists();
        if (ranked.Auth.IsOfflineMode || !PlayFabClientApi.IsLoggedIn || !config.HasTitleId)
        {
            onComplete?.Invoke(true, null);
            yield break;
        }

        bool done = false;
        List<FriendEntry> remote = null;
        string error = null;

        yield return PlayFabClientApi.GetFriendsList(config.titleId, (result, list) =>
        {
            done = true;
            if (result.success && list != null)
            {
                remote = list;
            }
            else
            {
                error = result.errorMessage;
            }
        });

        while (!done)
        {
            yield return null;
        }

        if (remote != null)
        {
            MergeRemote(remote);
            SaveLocal();
        }

        onComplete?.Invoke(remote != null, error);
    }

    void MergeRemote(List<FriendEntry> remote)
    {
        var byId = new Dictionary<string, FriendEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in _friends)
        {
            if (!string.IsNullOrEmpty(f.playFabId))
            {
                byId[f.playFabId] = f;
            }
        }

        foreach (var r in remote)
        {
            if (r == null || string.IsNullOrEmpty(r.playFabId))
            {
                continue;
            }

            if (byId.TryGetValue(r.playFabId, out var existing))
            {
                if (!string.IsNullOrEmpty(r.displayName))
                {
                    existing.displayName = r.displayName;
                }
            }
            else if (byId.Count < FriendKeys.MaxFriends)
            {
                byId[r.playFabId] = new FriendEntry(r.playFabId, r.displayName);
            }
        }

        _friends.Clear();
        foreach (var kv in byId)
        {
            _friends.Add(kv.Value);
        }
    }

    public IEnumerator AddFriendById(string playFabId, string displayNameHint, Action<bool, string> onComplete)
    {
        playFabId = playFabId?.Trim();
        if (string.IsNullOrEmpty(playFabId))
        {
            onComplete?.Invoke(false, "Friend code is empty.");
            yield break;
        }

        string localId = LocalPlayFabId();
        if (!string.IsNullOrEmpty(localId) &&
            string.Equals(localId, playFabId, StringComparison.OrdinalIgnoreCase))
        {
            onComplete?.Invoke(false, "That is your own friend code.");
            yield break;
        }

        if (Contains(playFabId))
        {
            onComplete?.Invoke(true, null);
            yield break;
        }

        if (_friends.Count >= FriendKeys.MaxFriends)
        {
            onComplete?.Invoke(false, $"Friend list is full ({FriendKeys.MaxFriends}).");
            yield break;
        }

        string displayName = displayNameHint;
        yield return EnsureLoggedIn();

        var config = PlayFabConfig.Current;
        var ranked = RankedServices.EnsureExists();
        if (!ranked.Auth.IsOfflineMode && PlayFabClientApi.IsLoggedIn && config.HasTitleId)
        {
            bool profileDone = false;
            yield return PlayFabClientApi.GetPlayerProfile(config.titleId, playFabId, (result, name) =>
            {
                if (result.success && !string.IsNullOrEmpty(name))
                {
                    displayName = name;
                }

                profileDone = true;
            });
            while (!profileDone)
            {
                yield return null;
            }

            bool addDone = false;
            bool addOk = false;
            string addErr = null;
            yield return PlayFabClientApi.AddFriend(config.titleId, playFabId, result =>
            {
                addOk = result.success;
                addErr = result.errorMessage;
                addDone = true;
            });
            while (!addDone)
            {
                yield return null;
            }

            // Still store locally even if PlayFab AddFriend fails (already friends, etc.)
            if (!addOk)
            {
                Debug.LogWarning($"[Friends] PlayFab AddFriend: {addErr}");
            }
        }

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = playFabId;
        }

        _friends.Add(new FriendEntry(playFabId, displayName));
        SaveLocal();
        onComplete?.Invoke(true, null);
    }

    public IEnumerator AddLastOpponent(Action<bool, string> onComplete)
    {
        if (string.IsNullOrEmpty(LastOpponentPlayFabId))
        {
            onComplete?.Invoke(false, "No recent opponent.");
            yield break;
        }

        yield return AddFriendById(LastOpponentPlayFabId, LastOpponentDisplayName, onComplete);
    }

    public IEnumerator RemoveFriend(string playFabId, Action<bool, string> onComplete = null)
    {
        playFabId = playFabId?.Trim();
        if (string.IsNullOrEmpty(playFabId))
        {
            onComplete?.Invoke(false, "Invalid id.");
            yield break;
        }

        for (int i = _friends.Count - 1; i >= 0; i--)
        {
            if (string.Equals(_friends[i].playFabId, playFabId, StringComparison.OrdinalIgnoreCase))
            {
                _friends.RemoveAt(i);
            }
        }

        SaveLocal();

        yield return EnsureLoggedIn();
        var config = PlayFabConfig.Current;
        var ranked = RankedServices.EnsureExists();
        if (!ranked.Auth.IsOfflineMode && PlayFabClientApi.IsLoggedIn && config.HasTitleId)
        {
            bool done = false;
            yield return PlayFabClientApi.RemoveFriend(config.titleId, playFabId, _ => { done = true; });
            while (!done)
            {
                yield return null;
            }
        }

        onComplete?.Invoke(true, null);
    }

    public string[] FriendUserIds()
    {
        var ids = new List<string>(_friends.Count);
        for (int i = 0; i < _friends.Count; i++)
        {
            if (!string.IsNullOrEmpty(_friends[i].playFabId))
            {
                ids.Add(_friends[i].playFabId);
            }
        }

        return ids.ToArray();
    }
}
