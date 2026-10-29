# Modernisation roadmap

Planned follow-up pull requests, in order. Each one should stay reviewable on its own and keep
the **Compile check** and **Build game** workflows green.

## Done in the first PR

- One-click Windows/macOS/Linux builds and releases (GameCI), license-free compile + UNET weaver check on PRs.
- Direct IP hosting/joining and LAN discovery instead of the shut-down Unity matchmaker.
- Server-authoritative damage, match phases and enemy movement; many desync and gameplay bug fixes
  (see the commit messages).

## Next pull requests

1. **Play-test pass and polish of the new lobby** — verify the cloned *Host IP* field / *Connect*
   button layout in the editor, tune positions, add a "copy my IP" button, show ping in the scoreboard.
2. **Unity 2018.4 → 2021.3 LTS (then 6 LTS)** — upgrade the editor, replace Post Processing v1 and
   Standard Assets scripts that no longer compile, move TextMesh Pro to the package version, re-bake
   lighting/NavMesh, update the CI image. UNET is removed in 2019.1+ (HLAPI package until 2021),
   so this goes together with the next item.
3. **UNET → Mirror** — the API is nearly identical (`NetworkBehaviour`, `[Command]`, `[ClientRpc]`,
   `[SyncVar]`); swap the components on prefabs/scenes, use the KCP transport, keep `LanDiscovery`.
   Remove the remaining string-based object lookups in favour of `NetworkIdentity` references.
4. **Internet play without port forwarding** — a relay/NAT-punch transport: Steam (FizzySteamworks)
   if the game goes on Steam, otherwise Epic Online Services or Unity Relay; join codes instead of IPs.
5. **Weapons visible to other players** — sync the active weapon (pistol/rifle) and its effects;
   today remote players always hold the pistol model.
6. **Server-side hit validation / lag compensation** — the server re-checks the shot ray against
   rewound positions instead of trusting the client's hit report (currently only damage is capped).
7. **Uncapped frame rate** — enable Rigidbody interpolation for the local player and remove the
   50 FPS cap (logic is already frame-rate independent).
8. **Settings menu** — mouse sensitivity, FOV, volume, resolution/fullscreen, key bindings.
9. **Late join / reconnect** — sync the full match state to players joining a running match.
10. **Code health** — remove unused test scenes/scripts and debug logs, split `GameManager`,
    replace `GetChild(n)` chains with serialized references, add play-mode tests for the match flow.
11. **Repository size** — move large binary assets to Git LFS (the history is ~2 GB) to speed up CI checkouts.
