# Modernisation roadmap

Work lands on the `ai` integration branch through small PRs and reaches `master` through the umbrella
PR. Each step must keep the **Compile check** and **Build game** workflows green.

## Done

See [CHANGELOG.md](../CHANGELOG.md). In short:
- CI builds and checks.
- Direct-connect multiplayer with LAN discovery.
- Server-authoritative gameplay.
- Remote players' weapons, settings window, player names and downed markers.
- Uncapped frame rate.
- Unity 6.6 and Mirror (replacing UNET).
- Many bug fixes.

## Next

1. **Play-test pass on Unity 6**:
   - Two-player game over LAN and cross-platform.
   - Check every HUD text (TextMesh Pro shaders were updated) and lighting.
2. **URP**:
   - Create the URP asset and convert materials.
   - Rewrite the custom shaders:
     - dissolve (enemies and particles),
     - QuickOutline,
     - KY effects,
     - starfield skybox.
   - Replace Post Processing v1 with URP Volumes and re-bake lighting.
3. **Menus**:
   - Give the settings window (IMGUI today) the game's paper look as a uGUI panel.
   - Add a "Play solo" button that hosts a private game.
4. **Internet play without port forwarding**: a relay/NAT-punch transport. Use Steam (FizzySteamworks)
   if the game goes on Steam, otherwise Epic Online Services or Unity Relay. Join codes instead of IPs.
5. **Server-side hit validation / lag compensation**: the server re-checks the shot ray against
   rewound positions instead of trusting the client's hit report. Today only the damage is capped.
6. **Key rebinding** in the settings window.
7. **Late join / reconnect**: sync the full match state to players joining a running match.
8. **Code health**:
   - Delete the old test scenes (see [ARCHITECTURE.md](ARCHITECTURE.md#scenes-build-settings-order)).
   - Split `GameManager`.
   - Replace `GameObject.Find` / `GetChild(n)` lookups with serialized references.
   - Fix the negative-scale BoxColliders on the sofas.
   - Re-save the 2018 NavMesh/LightingData assets.
   - Add play-mode tests for the match flow.
9. **Repository size**: move large binary assets to Git LFS (the history is ~2 GB) to speed up CI checkouts.
