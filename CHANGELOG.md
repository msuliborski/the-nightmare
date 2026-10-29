# Changelog

## Unreleased (`ai` branch)

### Multiplayer
- **Playable online again.** Unity's matchmaking service is gone, so games are now hosted directly:
  - Same network: hosted games show up in the server list.
  - Internet: join by IP or use a VPN such as Tailscale/ZeroTier.
  - Windows, macOS and Linux players can play together.
- Connection panel (lobby, pause menu, F1) with the host IP, a *Copy IP* button and ping.
- Readable connection errors. Joining is closed once a match has started.
- Player names (set in Settings) above teammates. Downed teammates are marked in red with their distance, visible through walls.
- Other players are shown holding the weapon they actually use.
- The server now decides damage, health, match phases and enemy movement, so every player sees the same game. This fixes:
  - monsters standing in different places for different players,
  - health not updating,
  - phases and losses happening at different times for different players.
- Crashes when players had different system languages (e.g. Polish and English) are fixed.

### Gameplay fixes
- Weapons:
  - Fire-mode cycling no longer enables automatic fire on weapons that don't support it.
  - A weapon can't fire, reload or aim during the swap animation.
  - Reload needs spare ammo, and the rifle refill no longer exceeds the maximum.
- Grenades and explosions:
  - Grenades fly where you aim.
  - Explosions hit each enemy once (they used to hit up to 3 times); barrels explode once.
- Pressing E (chests, pickups, revives) no longer triggers through other players' characters. Removing chairs is synced to everyone.
- Players:
  - Sprint speed no longer stays reduced after touching a slowing wall.
  - Mouse movement is no longer partly lost.
  - Players don't spawn on top of each other in the lobby.
- Rooms and matches:
  - Rooms with a single enemy spawn point no longer crash.
  - Capture areas count enemies correctly, no longer depend on the host's FPS, and turn red when lost.
  - A second match in the same session works.

### New
- Settings window: mouse sensitivity, invert Y, FOV, volume, graphics quality, resolution/fullscreen, player name.
- Command line: `-host [-name "Room"] [-port 7777]`, `-connect <ip[:port]>`.

### Under the hood
- One-click Windows/macOS/Linux builds and releases (GitHub Actions + GameCI).
- Every PR is checked by: C# compile for all platforms, the UNET weaver, a SyncVar deserialization check and a workflow lint.
- Music streams from disk instead of being kept decompressed in memory.
