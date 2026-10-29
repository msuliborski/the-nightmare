# Modernisation roadmap

Work lands on the `ai` integration branch through small PRs and reaches `master` through the umbrella
PR. Each step must keep the **Compile check** and **Build game** workflows green.

## Done

See [CHANGELOG.md](../CHANGELOG.md). In short:
- CI builds and checks.
- Direct-connect multiplayer with LAN discovery.
- Server-authoritative gameplay.
- Remote players' weapons, settings window, player names and downed markers.
- Many bug fixes.

## Next

1. **Play-test pass**: verify the lobby *Host IP* field and *Connect* button layout and the IMGUI
   panels in a real build, and polish their look.
2. **Unity 2018.4 → 2021.3 LTS (then 6 LTS)**:
   - Upgrade the editor.
   - Replace Post Processing v1 and the Standard Assets scripts that no longer compile.
   - Move TextMesh Pro to the package version.
   - Re-bake lighting and NavMesh, and update the CI image.

   UNET is removed in 2019.1+ (an HLAPI package exists until 2021), so this goes together with the next item.
3. **UNET → Mirror**: the API is nearly identical (`NetworkBehaviour`, `[Command]`, `[ClientRpc]`,
   `[SyncVar]`). Swap the components on prefabs and scenes, use the KCP transport and keep `LanDiscovery`.
   Replace the remaining string-based object lookups with `NetworkIdentity` references.
4. **Internet play without port forwarding**: a relay/NAT-punch transport. Use Steam (FizzySteamworks)
   if the game goes on Steam, otherwise Epic Online Services or Unity Relay. Join codes instead of IPs.
5. **Server-side hit validation / lag compensation**: the server re-checks the shot ray against
   rewound positions instead of trusting the client's hit report. Today only the damage is capped.
6. **Uncapped frame rate**: enable Rigidbody interpolation for the local player and remove the
   50 FPS cap. The game logic is already frame-rate independent.
7. **Key rebinding** in the settings window.
8. **Late join / reconnect**: sync the full match state to players joining a running match.
9. **Code health**:
   - Remove unused test scenes and scripts.
   - Split `GameManager`.
   - Replace `GetChild(n)` chains with serialized references.
   - Add play-mode tests for the match flow.
10. **Repository size**: move large binary assets to Git LFS (the history is ~2 GB) to speed up CI checkouts.
