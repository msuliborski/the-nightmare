# Architecture and onboarding

A map of the project for developers coming back to it. Building and CI are covered in
[BUILDING.md](BUILDING.md); planned work is in [ROADMAP.md](ROADMAP.md).

## Getting started

1. Install **Unity 6000.6.7f1** (Unity 6.6) with *Windows/Mac/Linux Build Support (Mono)* via Unity Hub.
2. Open the repository folder in Unity Hub. The first import takes a while.
3. Open `Assets/Scenes/MainMenu.unity` and press Play. To test multiplayer locally:
   - Build a player (**The Nightmare → Build**) and run it next to the editor.
   - Host in one, and join `127.0.0.1` in the other.

## Scenes (Build Settings order)

| Scene | Purpose |
|---|---|
| `MainMenu` | Title screen, trailer video, Play / Settings / Quit. |
| `LobbyTests` | Host / join screen. Owns the `NetworkManager` (prefab in `Resources/Networking`). |
| `ExtremeHomeMakeover` | The game: the house, three rooms, `GameManager`, enemies and capture areas. |

The other scenes in `Assets/Scenes` (`fpsTest*`, `michal*`, `jedrek`, `scopeTest`, `weaponSpawn`,
`NetworkScene`, `SampleScene`) are old experiments and are not in the build. They are candidates
for deletion.

## Networking (Mirror)

- **`CustomNetworkManager`** (`Scripts/Networking`) extends Mirror's `NetworkManager`. It handles:
  - host/join by IP, with the KCP transport on UDP 7777,
  - rejecting joins when the game is full or the match has started (`RejectMessage`),
  - status messages for the lobby,
  - the `-host` / `-connect` command line,
  - the F1 connection overlay.
- **`LanDiscovery`** announces and finds games on the local network over UDP broadcast on port 47777.
- **Server authority.** The host runs the server. Clients only:
  - send input-like commands (`[Command]`, e.g. shooting),
  - follow state through `[SyncVar]`s and `[ClientRpc]`s.

  Damage, health, enemy AI and match phases are decided on the server.
- **Networked prefabs** are listed in the `NetworkManager`'s *spawnPrefabs*:
  - players (`Boy`, `Girl`),
  - enemies (`Ghost`, `Skeleton*`),
  - traps (`TeddyBear`, `Snares`, `Barrel`),
  - `Grenade`,
  - `CaptureArea`.

  Mirror allows **one `NetworkIdentity` per object, at its root**.
- **Scene objects with a `NetworkIdentity`** (`GameManager`, the capture areas) need a **sceneId**
  that is saved in the scene file:
  - When you add one in the editor and save, Mirror assigns it.
  - Never let the build generate it: CI fails a PR that has a networked scene object without one.
  - `Tools/assign_scene_ids.py` can fill missing ids.

Files with network code (Commands/Rpcs/SyncVars):

- `PlayerManager` (health, downed/revive)
- `PlayerShoot` (shots, hits, barrels)
- `PlayerEquipment` (weapon switching, ammo)
- `GameManager` (match state)
- `EnemyControllerServer`
- `CaptureArea`
- `PlacementController` (placing traps)
- `TeddyBearServer`, `Barrel`, `Snares`, `Grenade`

## Game flow (`GameManager`)

`MatchState`: `Lobby → Room1Prepare → Room1Fight → Room2Prepare → … → Room3Fight → Win | Lose`.

- **Prepare phases** (clock counts down to midnight): players collect weapons from chests and
  place traps.
- **Fight phases:**
  - Enemies spawn from the room's spawn points and walk to the room's capture areas (candles).
  - A room is lost when its candles are taken.
  - Surviving the wave moves on to the next room.

All of this runs on the server. Clients get the state through `GameManager`'s SyncVars and Rpcs.

## Code map (`Assets/Scripts`)

| Folder | Contents |
|---|---|
| `Player/FPS` | `PlayerSetup` (local vs remote setup, HUD wiring), `PlayerController` + `PlayerMotor` (input, movement), `PlayerShoot`, `PlayerEquipment`, `PlayerManager` (health, revive). |
| `Weapons` | `Weapon` (stats, ammo), chests and pickups, `Grenade`. |
| `Enemy` | `EnemyControllerServer` (NavMesh AI, server only), `EnemyControllerClient` (visuals), dissolve on death. |
| `Traps` | Teddy bear (defends an area), snares (stop enemies), explosive barrels, `ExplosionDamage`. |
| `Building` | Placement grid for traps (`PlacementController` in the root folder uses it). |
| `UI` | HUD (`BulletsHUD`, `HealthBar`, `ClipsManager`, `ClockManager`), menus, `SettingsMenu` + `GameSettings` (IMGUI settings window, PlayerPrefs), `Nameplates`, `FullscreenUIFitter` (keeps full-screen art covering non-16:9 screens). |
| `Networking` | Network manager, LAN discovery, lobby UI (`HostGame`, `JoinGame`, `ServerListItem`). |
| `Editor` | `NightmareBuild` (CI and menu builds), `NetworkIdBaker` (Mirror asset ids before builds). |

## Third-party code in `Assets`

| Folder | What it is |
|---|---|
| `Mirror` | Networking, MIT. Update it by replacing the folder from a Mirror release. |
| `TextMesh Pro` | Essential resources from uGUI 2.6. |
| `PostProcessing` | Post Processing Stack v1, for the built-in render pipeline. |
| `QuickOutline` | Outline effect. |
| `KY_effects`, `Particle Dissolve Shader…`, `Standard Assets` (trimmed) | Effects and particles. |

## Tools (`Tools/`, outside Unity)

| Script | Purpose |
|---|---|
| `migrate_unet_to_mirror.py` | One-off conversion of UNET components in scenes and prefabs. Kept for reference and idempotent. |
| `assign_scene_ids.py` | Writes missing Mirror sceneIds into scenes. Run it with `--check` to only verify. |

## Known gaps

See [ROADMAP.md](ROADMAP.md). The main ones:
- The render pipeline is still built-in (not URP).
- The settings window uses plain IMGUI styling.
- Internet play needs port forwarding or a VPN.
