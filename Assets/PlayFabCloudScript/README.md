# PlayFab Ranked Ladder Setup

## Title setup (required for online login)

Title id in client: `Resources/Ranked/PlayFabConfig.json` → `titleId` (e.g. `11E74D`).

In **Game Manager → Settings → API Features**, enable:

- **Allow Login with Custom ID** (client `LoginWithCustomID`)
- **Allow client to create new users** (or any equivalent “player creation” / “create accounts with Custom ID” option)
- Custom ID **linking** if listed separately (client `LinkCustomID` — used for wipe recovery codes)

If log shows `PlayerCreationDisabled` / `"Player creations have been disabled for this API"`:

1. Open title **11E74D** in PlayFab Game Manager  
2. **Settings → API Features**  
3. **Enable** client user creation (new guest Custom IDs cannot sign in while this is off)  
4. Re-enter Play Mode — you should see `[Ranked] PlayFab login OK` and a new player under **Players**

Without these, the client gets HTTP 400 and falls back to **offline ranked** (local MMR only; Players stay empty).

After a working login: **Players** shows new accounts; statistics `RankedMMR` / `RankedWins` / `RankedLosses` update after match report.

## Critical: redeploy Cloud Script after client fixes

Re-upload `RankedCloudScript.js` in:
**Live Ops → Automation → Cloud Script → new revision → make active.**

## Why stats never updated (fixed in client)

`EndGame` used to start the ranked report on `ContinuousController`, then immediately call `StopAllCoroutines()` on it — so PlayFab was never called. Reports now run on `RankedServices` and are not stopped.

Cloud Script now **settles on the first report** and calls `UpdatePlayerStatistics` immediately (second report is idempotent).

## Classic player statistics

Cloud Script uses classic APIs:

- `server.UpdatePlayerStatistics`
- `server.GetPlayerStatistics`

Exact names: `RankedMMR`, `RankedWins`, `RankedLosses`

Check **Players → [player] → Statistics**, not only the entity leaderboard screens.

## Guest identity across reinstalls

PlayFab login uses `LoginWithCustomID`. The guest CustomId is **stable on the same device**:

- Format: `dcgo-v2-` + SHA256(package + device fingerprint) truncated (no random `Guid`)
- PlayerPrefs caches the id; after uninstall/reinstall it is **recomputed the same way** when prefs are empty
- Existing installs keep their PlayerPrefs id until data is cleared (no mid-update migration)

**Survives (typical):** reinstall of the same Android package on the same user/device (Unity `deviceUniqueIdentifier` / app-scoped device id).

**Does not survive:** new phone, factory reset, some OEM privacy/device-id wipes, iOS when all apps from the same vendor are removed (IDFV reset), different Android user profile.

For those cases use the **recovery code** below (not Google/Apple login).

Offline ranked ids use the same device-stable hash (`offline-v2-...`).

## Friend code vs recovery code

| | Friend code | Recovery code |
|---|---|---|
| What it is | Public **PlayFabId** | Private write-down code (`XXXX-XXXX-XXXX`) |
| Where shown | Friends panel | Home → **Account** |
| Purpose | Add friends | Restore account after wipe / new phone |
| Safe to share? | Yes | **No** — anyone with it can take over the account |

After first online login the client links a second PlayFab CustomId (`dcgo-rc-{code}`) to the guest account and stores the display code in private UserData. The player only ever types the short code.

This uses the same **Custom ID** APIs already required for ranked — it does **not** need username/password API Features.

### Wipe / new-phone recovery

1. Before wiping: Home → **Account** → write down / Copy the recovery code.
2. Reinstall and open home (a new empty guest account is fine).
3. Home → **Account** → paste code → **Recover** (confirms replace).
4. Client: `LoginWithCustomID(dcgo-rc-…)` → `LinkCustomID` (ForceLink) for this device’s CustomId → restore display name → refresh ranked + friends.

**Restored:** PlayFabId, ranked MMR/wins/losses, PlayFab friends, nickname.

**Not restored:** local decks, casual win count, volume/language and other PlayerPrefs.

Console success: `[Ranked] Account recovered. playFabId=...` / `[Ranked] Recovery code attached...`

If Account shows `Recovery code: —`, check the Unity console for `LinkCustomID (recovery) failed` and confirm Custom ID login/linking is allowed under API Features.

## Verify

1. Finish a ranked match.
2. Console: `[Ranked] Reporting...` then `[Ranked] Stats updated...` (or an error).
3. Game Manager → that player → `RankedMMR` changed.
4. Uninstall + reinstall (same device, no factory reset): same PlayFabId and MMR.
5. Account panel shows a recovery code; Copy works.
6. (Optional) Clear app data / use another device fingerprint, Recover with the code → same PlayFabId and MMR.

## Offline

If log shows `offline=true` or console error about offline fallback:

- Only local PlayerPrefs MMR applies — PlayFab Players/stats do **not** update.
- Recovery codes are **not** attached in offline mode.
- In-game UI shows **Ranked (Offline): …** (player info + ranked queue) and **Offline rank** on the result screen.
- Fix: enable Custom ID login, confirm `titleId`, re-enter play mode; success log is `[Ranked] PlayFab login OK`.

Failed HTTP calls now log PlayFab `error` / `errorMessage` / `errorDetails` (not only `HTTP 400 Bad Request`).
