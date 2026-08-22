using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
// System.Collections provides IDictionary / IList / DictionaryEntry for MiniJson

/// <summary>
/// Minimal PlayFab Client REST API used by ranked services (no full SDK dependency).
/// </summary>
public static class PlayFabClientApi
{
    public static string SessionTicket { get; private set; }
    public static string PlayFabId { get; private set; }
    public static string EntityToken { get; private set; }

    public static bool IsLoggedIn => !string.IsNullOrEmpty(SessionTicket) && !string.IsNullOrEmpty(PlayFabId);

    public static void ClearSession()
    {
        SessionTicket = null;
        PlayFabId = null;
        EntityToken = null;
    }

    static void ApplyLoginResult(ApiResult result)
    {
        if (result == null || !result.success || result.data == null)
        {
            return;
        }

        SessionTicket = GetString(result.data, "SessionTicket");
        PlayFabId = GetString(result.data, "PlayFabId");
        if (result.data.TryGetValue("EntityToken", out var etObj) && etObj is Dictionary<string, object> et)
        {
            EntityToken = GetString(et, "EntityToken");
        }
    }

    static string ApiRoot(string titleId) => $"https://{titleId}.playfabapi.com";

    public class ApiResult
    {
        public bool success;
        public int httpCode;
        public string errorMessage;
        public string rawBody;
        public Dictionary<string, object> data;
    }

    public static IEnumerator LoginWithCustomId(
        string titleId,
        string customId,
        bool createAccount,
        Action<ApiResult> onComplete)
    {
        // Keep login body minimal — nested InfoRequestParameters can cause InvalidParams
        // depending on MiniJson shape; stats load via GetPlayerStatistics after session.
        titleId = titleId?.Trim();
        customId = customId?.Trim();

        if (string.IsNullOrEmpty(titleId) || string.IsNullOrEmpty(customId))
        {
            onComplete?.Invoke(new ApiResult
            {
                success = false,
                errorMessage = "TitleId and CustomId are required for LoginWithCustomID",
            });
            yield break;
        }

        var body = new Dictionary<string, object>
        {
            { "TitleId", titleId },
            { "CustomId", customId },
            { "CreateAccount", createAccount },
        };

        yield return Post(titleId, "/Client/LoginWithCustomID", body, null, result =>
        {
            ApplyLoginResult(result);
            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator GetPlayerStatistics(string titleId, Action<ApiResult, Dictionary<string, int>> onComplete)
    {
        var body = new Dictionary<string, object>
        {
            {
                "StatisticNames", new List<string>
                {
                    RankedKeys.StatMmr,
                    RankedKeys.StatWins,
                    RankedKeys.StatLosses,
                }
            },
        };

        yield return Post(titleId, "/Client/GetPlayerStatistics", body, SessionTicket, result =>
        {
            var stats = new Dictionary<string, int>();
            if (result.success && result.data != null &&
                result.data.TryGetValue("Statistics", out var sObj) && sObj is List<object> list)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<string, object> stat)
                    {
                        string name = GetString(stat, "StatisticName");
                        int value = GetInt(stat, "Value");
                        if (!string.IsNullOrEmpty(name))
                        {
                            stats[name] = value;
                        }
                    }
                }
            }

            onComplete?.Invoke(result, stats);
        });
    }

    public static IEnumerator UpdatePlayerStatistics(string titleId, Dictionary<string, int> stats, Action<ApiResult> onComplete)
    {
        var list = new List<object>();
        foreach (var kv in stats)
        {
            list.Add(new Dictionary<string, object>
            {
                { "StatisticName", kv.Key },
                { "Value", kv.Value },
            });
        }

        var body = new Dictionary<string, object> { { "Statistics", list } };
        yield return Post(titleId, "/Client/UpdatePlayerStatistics", body, SessionTicket, onComplete);
    }

    public static IEnumerator GetPhotonAuthenticationToken(string titleId, string photonAppId, Action<ApiResult, string> onComplete)
    {
        var body = new Dictionary<string, object> { { "PhotonApplicationId", photonAppId } };
        yield return Post(titleId, "/Client/GetPhotonAuthenticationToken", body, SessionTicket, result =>
        {
            string token = null;
            if (result.success && result.data != null)
            {
                token = GetString(result.data, "PhotonCustomAuthenticationToken");
            }

            onComplete?.Invoke(result, token);
        });
    }

    public static IEnumerator ExecuteCloudScript(
        string titleId,
        string functionName,
        Dictionary<string, object> functionParameter,
        Action<ApiResult, Dictionary<string, object>> onComplete)
    {
        var body = new Dictionary<string, object>
        {
            { "FunctionName", functionName },
            { "FunctionParameter", functionParameter ?? new Dictionary<string, object>() },
            { "GeneratePlayStreamEvent", false },
        };

        yield return Post(titleId, "/Client/ExecuteCloudScript", body, SessionTicket, result =>
        {
            Dictionary<string, object> fnResult = null;
            if (result.success && result.data != null)
            {
                if (result.data.TryGetValue("Error", out var err) && err != null)
                {
                    result.success = false;
                    if (err is Dictionary<string, object> errDict)
                    {
                        result.errorMessage = GetString(errDict, "Message")
                            ?? GetString(errDict, "Error")
                            ?? err.ToString();
                    }
                    else
                    {
                        result.errorMessage = err.ToString();
                    }

                    Debug.LogError($"[PlayFab] CloudScript Error ({functionName}): {result.errorMessage}\n{result.rawBody}");
                }
                else if (result.data.TryGetValue("FunctionResult", out var fr))
                {
                    if (fr is Dictionary<string, object> dict)
                    {
                        fnResult = dict;
                    }
                    else if (fr != null)
                    {
                        // Some responses wrap result oddly
                        Debug.LogWarning($"[PlayFab] FunctionResult type={fr.GetType().Name} value={fr}");
                    }
                }

                if (result.data.TryGetValue("Logs", out var logsObj) && logsObj is System.Collections.IList logList)
                {
                    foreach (var entry in logList)
                    {
                        Debug.Log($"[PlayFab CloudScript log] {entry}");
                    }
                }
            }
            else if (!result.success)
            {
                Debug.LogError($"[PlayFab] ExecuteCloudScript HTTP failed ({functionName}): {result.errorMessage}\n{result.rawBody}");
            }

            onComplete?.Invoke(result, fnResult);
        });
    }

    public static IEnumerator UpdateDisplayName(string titleId, string displayName, Action<ApiResult> onComplete)
    {
        var body = new Dictionary<string, object> { { "DisplayName", displayName } };
        yield return Post(titleId, "/Client/UpdateUserTitleDisplayName", body, SessionTicket, onComplete);
    }

    // === DCGO-CUSTOM:recovery begin ===
    public static IEnumerator LoginWithPlayFab(
        string titleId,
        string username,
        string password,
        Action<ApiResult> onComplete)
    {
        titleId = titleId?.Trim();
        username = username?.Trim();

        if (string.IsNullOrEmpty(titleId) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            onComplete?.Invoke(new ApiResult
            {
                success = false,
                errorMessage = "TitleId, Username, and Password are required for LoginWithPlayFab",
            });
            yield break;
        }

        var body = new Dictionary<string, object>
        {
            { "TitleId", titleId },
            { "Username", username },
            { "Password", password },
        };

        yield return Post(titleId, "/Client/LoginWithPlayFab", body, null, result =>
        {
            ApplyLoginResult(result);
            onComplete?.Invoke(result);
        });
    }

    public static IEnumerator AddUsernamePassword(
        string titleId,
        string username,
        string password,
        Action<ApiResult> onComplete)
    {
        username = username?.Trim();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            onComplete?.Invoke(new ApiResult
            {
                success = false,
                errorMessage = "Username and Password are required for AddUsernamePassword",
            });
            yield break;
        }

        var body = new Dictionary<string, object>
        {
            { "Username", username },
            { "Password", password },
        };

        yield return Post(titleId, "/Client/AddUsernamePassword", body, SessionTicket, onComplete);
    }

    public static IEnumerator GetAccountInfo(string titleId, Action<ApiResult, string> onComplete)
    {
        var body = new Dictionary<string, object>();
        yield return Post(titleId, "/Client/GetAccountInfo", body, SessionTicket, result =>
        {
            string username = null;
            if (result.success && result.data != null &&
                result.data.TryGetValue("AccountInfo", out var aiObj) &&
                aiObj is Dictionary<string, object> accountInfo)
            {
                username = GetString(accountInfo, "Username");
            }

            onComplete?.Invoke(result, username);
        });
    }

    public static IEnumerator LinkCustomId(
        string titleId,
        string customId,
        bool forceLink,
        Action<ApiResult> onComplete)
    {
        customId = customId?.Trim();
        if (string.IsNullOrEmpty(customId))
        {
            onComplete?.Invoke(new ApiResult
            {
                success = false,
                errorMessage = "CustomId is required for LinkCustomID",
            });
            yield break;
        }

        var body = new Dictionary<string, object>
        {
            { "CustomId", customId },
            { "ForceLink", forceLink },
        };

        yield return Post(titleId, "/Client/LinkCustomID", body, SessionTicket, onComplete);
    }

    public static IEnumerator GetUserData(
        string titleId,
        IList<string> keys,
        Action<ApiResult, Dictionary<string, string>> onComplete)
    {
        var body = new Dictionary<string, object>();
        if (keys != null && keys.Count > 0)
        {
            body["Keys"] = new List<string>(keys);
        }

        yield return Post(titleId, "/Client/GetUserData", body, SessionTicket, result =>
        {
            var map = new Dictionary<string, string>();
            if (result.success && result.data != null &&
                result.data.TryGetValue("Data", out var dataObj) &&
                dataObj is Dictionary<string, object> dataDict)
            {
                foreach (var kv in dataDict)
                {
                    if (kv.Value is Dictionary<string, object> entry)
                    {
                        string value = GetString(entry, "Value");
                        if (value != null)
                        {
                            map[kv.Key] = value;
                        }
                    }
                }
            }

            onComplete?.Invoke(result, map);
        });
    }

    public static IEnumerator UpdateUserData(
        string titleId,
        Dictionary<string, string> data,
        Action<ApiResult> onComplete)
    {
        var dataObj = new Dictionary<string, object>();
        if (data != null)
        {
            foreach (var kv in data)
            {
                dataObj[kv.Key] = kv.Value;
            }
        }

        var body = new Dictionary<string, object>
        {
            { "Data", dataObj },
        };

        yield return Post(titleId, "/Client/UpdateUserData", body, SessionTicket, onComplete);
    }
    // === DCGO-CUSTOM:recovery end ===

    // === DCGO-CUSTOM:friends begin ===
    public static IEnumerator GetFriendsList(string titleId, Action<ApiResult, List<FriendEntry>> onComplete)
    {
        var body = new Dictionary<string, object>
        {
            { "IncludeSteamFriends", false },
            { "IncludeFacebookFriends", false },
        };

        yield return Post(titleId, "/Client/GetFriendsList", body, SessionTicket, result =>
        {
            var list = new List<FriendEntry>();
            if (result.success && result.data != null &&
                result.data.TryGetValue("Friends", out var fObj) && fObj is List<object> friends)
            {
                foreach (var item in friends)
                {
                    if (!(item is Dictionary<string, object> dict))
                    {
                        continue;
                    }

                    string id = GetString(dict, "FriendPlayFabId");
                    string name = null;
                    if (dict.TryGetValue("TitleDisplayName", out var tdn) && tdn != null)
                    {
                        name = tdn.ToString();
                    }
                    else if (dict.TryGetValue("Username", out var un) && un != null)
                    {
                        name = un.ToString();
                    }

                    if (dict.TryGetValue("Profile", out var profObj) && profObj is Dictionary<string, object> profile)
                    {
                        string dn = GetString(profile, "DisplayName");
                        if (!string.IsNullOrEmpty(dn))
                        {
                            name = dn;
                        }
                    }

                    if (!string.IsNullOrEmpty(id))
                    {
                        list.Add(new FriendEntry(id, string.IsNullOrEmpty(name) ? id : name));
                    }
                }
            }

            onComplete?.Invoke(result, list);
        });
    }

    public static IEnumerator AddFriend(string titleId, string friendPlayFabId, Action<ApiResult> onComplete)
    {
        var body = new Dictionary<string, object>
        {
            { "FriendPlayFabId", friendPlayFabId },
        };
        yield return Post(titleId, "/Client/AddFriend", body, SessionTicket, onComplete);
    }

    public static IEnumerator RemoveFriend(string titleId, string friendPlayFabId, Action<ApiResult> onComplete)
    {
        var body = new Dictionary<string, object>
        {
            { "FriendPlayFabId", friendPlayFabId },
        };
        yield return Post(titleId, "/Client/RemoveFriend", body, SessionTicket, onComplete);
    }

    public static IEnumerator GetPlayerProfile(string titleId, string playFabId, Action<ApiResult, string> onComplete)
    {
        var body = new Dictionary<string, object>
        {
            { "PlayFabId", playFabId },
            {
                "ProfileConstraints", new Dictionary<string, object>
                {
                    { "ShowDisplayName", true },
                }
            },
        };

        yield return Post(titleId, "/Client/GetPlayerProfile", body, SessionTicket, result =>
        {
            string displayName = null;
            if (result.success && result.data != null &&
                result.data.TryGetValue("PlayerProfile", out var pp) &&
                pp is Dictionary<string, object> profile)
            {
                displayName = GetString(profile, "DisplayName");
            }

            onComplete?.Invoke(result, displayName);
        });
    }
    // === DCGO-CUSTOM:friends end ===

    static IEnumerator Post(
        string titleId,
        string path,
        Dictionary<string, object> body,
        string sessionTicket,
        Action<ApiResult> onComplete)
    {
        string json = MiniJson.Serialize(body);
        byte[] raw = Encoding.UTF8.GetBytes(json);
        string url = ApiRoot(titleId?.Trim()) + path;

        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(raw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(sessionTicket))
            {
                req.SetRequestHeader("X-Authorization", sessionTicket);
            }

            yield return req.SendWebRequest();

            var result = new ApiResult
            {
                httpCode = (int)req.responseCode,
                rawBody = req.downloadHandler?.text,
            };

            // Always prefer PlayFab JSON body (error/errorMessage) over Unity's generic HTTP string.
            // ProtocolError (e.g. HTTP 400) still includes a useful downloadHandler body.
            try
            {
                if (string.IsNullOrEmpty(result.rawBody))
                {
                    result.success = false;
                    result.errorMessage = BuildHttpFallbackError(req);
                    LogPlayFabPostFailure(titleId, path, result, json);
                    onComplete?.Invoke(result);
                    yield break;
                }

                var root = MiniJson.Deserialize(result.rawBody) as Dictionary<string, object>;
                if (root == null)
                {
                    result.success = false;
                    result.errorMessage = BuildHttpFallbackError(req) ?? "Invalid JSON response";
                    LogPlayFabPostFailure(titleId, path, result, json);
                    onComplete?.Invoke(result);
                    yield break;
                }

                int code = GetInt(root, "code");
                string status = GetString(root, "status");
                if (code == 200 && string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    result.success = true;
                    if (root.TryGetValue("data", out var dataObj) && dataObj is Dictionary<string, object> data)
                    {
                        result.data = data;
                    }
                    else
                    {
                        result.data = new Dictionary<string, object>();
                    }
                }
                else
                {
                    result.success = false;
                    result.errorMessage = FormatPlayFabError(root, req);
                    if (root.TryGetValue("data", out var dataObj) && dataObj is Dictionary<string, object> data)
                    {
                        result.data = data;
                    }

                    LogPlayFabPostFailure(titleId, path, result, json);
                }
            }
            catch (Exception e)
            {
                result.success = false;
                result.errorMessage = e.Message;
                LogPlayFabPostFailure(titleId, path, result, json);
            }

            onComplete?.Invoke(result);
        }
    }

    static string FormatPlayFabError(Dictionary<string, object> root, UnityWebRequest req)
    {
        string error = GetString(root, "error");
        string errorMessage = GetString(root, "errorMessage");
        string details = FormatErrorDetails(root);

        var parts = new List<string>();
        if (!string.IsNullOrEmpty(error)) parts.Add(error);
        if (!string.IsNullOrEmpty(errorMessage)) parts.Add(errorMessage);
        if (!string.IsNullOrEmpty(details)) parts.Add(details);

        if (parts.Count > 0)
        {
            return string.Join(" — ", parts);
        }

        return BuildHttpFallbackError(req) ?? "PlayFab request failed";
    }

    static string FormatErrorDetails(Dictionary<string, object> root)
    {
        if (root == null || !root.TryGetValue("errorDetails", out var det) || det == null)
        {
            return null;
        }

        if (det is Dictionary<string, object> dict)
        {
            var sb = new StringBuilder();
            foreach (var kv in dict)
            {
                if (sb.Length > 0) sb.Append("; ");
                sb.Append(kv.Key).Append('=').Append(kv.Value);
            }

            return sb.Length > 0 ? sb.ToString() : null;
        }

        return det.ToString();
    }

    static string BuildHttpFallbackError(UnityWebRequest req)
    {
        if (req == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(req.error))
        {
            return $"{req.error} (HTTP {(int)req.responseCode})";
        }

        if (req.responseCode != 0)
        {
            return $"HTTP {(int)req.responseCode}";
        }

        return req.result.ToString();
    }

    static void LogPlayFabPostFailure(string titleId, string path, ApiResult result, string requestJson)
    {
        // Login has no secrets; other calls omit session ticket from logs. Truncate long bodies.
        string bodySnippet = Truncate(result?.rawBody, 600);
        string reqSnippet = Truncate(requestJson, 400);
        Debug.LogError(
            $"[PlayFab] {path} failed titleId={titleId} http={result?.httpCode} " +
            $"err={result?.errorMessage}\nresponse={bodySnippet}\nrequest={reqSnippet}");
    }

    static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s) || s.Length <= max)
        {
            return s;
        }

        return s.Substring(0, max) + "...";
    }

    public static string GetString(Dictionary<string, object> dict, string key)
    {
        if (dict == null || !dict.TryGetValue(key, out var v) || v == null)
        {
            return null;
        }

        return v.ToString();
    }

    public static int GetInt(Dictionary<string, object> dict, string key, int defaultValue = 0)
    {
        if (dict == null || !dict.TryGetValue(key, out var v) || v == null)
        {
            return defaultValue;
        }

        switch (v)
        {
            case int i: return i;
            case long l: return (int)l;
            case double d: return (int)d;
            case float f: return (int)f;
            default:
                if (int.TryParse(v.ToString(), out int parsed))
                {
                    return parsed;
                }

                return defaultValue;
        }
    }

    public static bool GetBool(Dictionary<string, object> dict, string key, bool defaultValue = false)
    {
        if (dict == null || !dict.TryGetValue(key, out var v) || v == null)
        {
            return defaultValue;
        }

        if (v is bool b) return b;
        if (bool.TryParse(v.ToString(), out bool parsed)) return parsed;
        return defaultValue;
    }
}

/// <summary>
/// Minimal JSON serializer/deserializer (dictionary-based) for PlayFab REST payloads.
/// Adapted from common MiniJSON patterns; sufficient for PlayFab client objects.
/// </summary>
public static class MiniJson
{
    public static string Serialize(object obj)
    {
        var sb = new StringBuilder();
        SerializeValue(obj, sb);
        return sb.ToString();
    }

    public static object Deserialize(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return new Parser(json).ParseValue();
    }

    static void SerializeValue(object value, StringBuilder sb)
    {
        if (value == null)
        {
            sb.Append("null");
            return;
        }

        switch (value)
        {
            case string s:
                SerializeString(s, sb);
                break;
            case bool b:
                sb.Append(b ? "true" : "false");
                break;
            case int i:
                sb.Append(i);
                break;
            case long l:
                sb.Append(l);
                break;
            case float f:
                sb.Append(f.ToString(System.Globalization.CultureInfo.InvariantCulture));
                break;
            case double d:
                sb.Append(d.ToString(System.Globalization.CultureInfo.InvariantCulture));
                break;
            case IDictionary dict:
                sb.Append('{');
                bool first = true;
                foreach (DictionaryEntry e in dict)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    SerializeString(e.Key.ToString(), sb);
                    sb.Append(':');
                    SerializeValue(e.Value, sb);
                }

                sb.Append('}');
                break;
            case IList list:
                sb.Append('[');
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    SerializeValue(list[i], sb);
                }

                sb.Append(']');
                break;
            case IDictionary<string, object> genDict:
                sb.Append('{');
                bool first2 = true;
                foreach (var e in genDict)
                {
                    if (!first2) sb.Append(',');
                    first2 = false;
                    SerializeString(e.Key, sb);
                    sb.Append(':');
                    SerializeValue(e.Value, sb);
                }

                sb.Append('}');
                break;
            case IList<object> genList:
                sb.Append('[');
                for (int i = 0; i < genList.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    SerializeValue(genList[i], sb);
                }

                sb.Append(']');
                break;
            default:
                SerializeString(value.ToString(), sb);
                break;
        }
    }

    static void SerializeString(string s, StringBuilder sb)
    {
        sb.Append('"');
        foreach (char c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < ' ')
                    {
                        sb.Append("\\u");
                        sb.Append(((int)c).ToString("x4"));
                    }
                    else
                    {
                        sb.Append(c);
                    }

                    break;
            }
        }

        sb.Append('"');
    }

    sealed class Parser
    {
        readonly string _json;
        int _index;

        public Parser(string json)
        {
            _json = json;
            _index = 0;
        }

        public object ParseValue()
        {
            SkipWhitespace();
            if (_index >= _json.Length) return null;

            char c = _json[_index];
            if (c == '{') return ParseObject();
            if (c == '[') return ParseArray();
            if (c == '"') return ParseString();
            if (c == 't' || c == 'f') return ParseBool();
            if (c == 'n') return ParseNull();
            return ParseNumber();
        }

        Dictionary<string, object> ParseObject()
        {
            var dict = new Dictionary<string, object>();
            _index++; // {
            while (true)
            {
                SkipWhitespace();
                if (_index >= _json.Length) break;
                if (_json[_index] == '}')
                {
                    _index++;
                    break;
                }

                string key = ParseString();
                SkipWhitespace();
                _index++; // :
                object value = ParseValue();
                dict[key] = value;
                SkipWhitespace();
                if (_index < _json.Length && _json[_index] == ',')
                {
                    _index++;
                }
            }

            return dict;
        }

        List<object> ParseArray()
        {
            var list = new List<object>();
            _index++; // [
            while (true)
            {
                SkipWhitespace();
                if (_index >= _json.Length) break;
                if (_json[_index] == ']')
                {
                    _index++;
                    break;
                }

                list.Add(ParseValue());
                SkipWhitespace();
                if (_index < _json.Length && _json[_index] == ',')
                {
                    _index++;
                }
            }

            return list;
        }

        string ParseString()
        {
            var sb = new StringBuilder();
            _index++; // "
            while (_index < _json.Length)
            {
                char c = _json[_index++];
                if (c == '"') break;
                if (c == '\\' && _index < _json.Length)
                {
                    char e = _json[_index++];
                    switch (e)
                    {
                        case '"':
                        case '\\':
                        case '/':
                            sb.Append(e);
                            break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_index + 3 < _json.Length)
                            {
                                string hex = _json.Substring(_index, 4);
                                sb.Append((char)Convert.ToInt32(hex, 16));
                                _index += 4;
                            }

                            break;
                        default:
                            sb.Append(e);
                            break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        object ParseNumber()
        {
            int start = _index;
            while (_index < _json.Length)
            {
                char c = _json[_index];
                if ((c >= '0' && c <= '9') || c == '-' || c == '+' || c == '.' || c == 'e' || c == 'E')
                {
                    _index++;
                }
                else
                {
                    break;
                }
            }

            string num = _json.Substring(start, _index - start);
            if (num.IndexOf('.') >= 0 || num.IndexOf('e') >= 0 || num.IndexOf('E') >= 0)
            {
                if (double.TryParse(num, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double d))
                {
                    return d;
                }
            }
            else if (long.TryParse(num, out long l))
            {
                if (l >= int.MinValue && l <= int.MaxValue)
                {
                    return (int)l;
                }

                return l;
            }

            return 0;
        }

        bool ParseBool()
        {
            if (_json.IndexOf("true", _index, StringComparison.Ordinal) == _index)
            {
                _index += 4;
                return true;
            }

            _index += 5;
            return false;
        }

        object ParseNull()
        {
            _index += 4;
            return null;
        }

        void SkipWhitespace()
        {
            while (_index < _json.Length)
            {
                char c = _json[_index];
                if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                {
                    _index++;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
