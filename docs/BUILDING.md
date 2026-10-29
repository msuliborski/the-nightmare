# Building The Nightmare

## One-click builds (GitHub Actions)

The **Build game** workflow (`.github/workflows/build.yml`) builds Windows, macOS and Linux
packages with Unity 2018.4.3f1 through [GameCI](https://game.ci).

1. Open **Actions → Build game → Run workflow**.
2. Pick the platforms (default: all) and, optionally, tick *publish a GitHub Release*.
3. When it finishes, download the zip / tar.gz packages from the run's **Artifacts**
   (or from the pre-release, if you ticked it).

Pushing a tag such as `v1.1.0` builds all platforms and publishes a regular release.

Every pull request also runs **Compile check**: it compiles all scripts for Windows/macOS/Linux
plus the editor scripts and runs Unity's UNET weaver. It needs no Unity license and takes about a minute.

### One-time setup: Unity license secrets

Unity requires an activated license even for CI builds. Add these repository secrets
(**Settings → Secrets and variables → Actions → New repository secret**):

| Secret | Value |
| --- | --- |
| `UNITY_EMAIL` | Email of your Unity account |
| `UNITY_PASSWORD` | Password of your Unity account |
| `UNITY_LICENSE` | **Personal license:** the full contents of your `Unity_lic.ulf` file |
| `UNITY_SERIAL` | **Plus/Pro license only:** your serial key (instead of `UNITY_LICENSE`) |

To get `Unity_lic.ulf` for a Personal license, sign in to Unity Hub on your computer and activate
a Personal license (Hub → Settings → Licenses). The file is then at:

- Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
- macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
- Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`

See GameCI's [activation guide](https://game.ci/docs/github/activation) if Unity changes the flow.

The first build of each platform imports the whole project (~2.6 GB of assets) and takes a while;
the imported `Library` folder is cached, so later builds are much faster.

### Running the packages

- **Windows:** unzip and run `TheNightmare.exe`. Allow it through the firewall when asked
  (needed for hosting and LAN discovery).
- **macOS:** extract, then run `xattr -cr TheNightmare.app` once (the build is not signed/notarized)
  and open the app.
- **Linux:** extract and run `./TheNightmare`.

## Local builds

Open the project in Unity **2018.4.3f1** and use the menu **The Nightmare → Build**
(output goes to `Builds/<platform>/`). Building macOS/Linux from Windows requires the
corresponding Unity *Build Support* modules.

To check scripts without Unity installed, run the same compile check CI uses:

```bash
docker run --rm -v "$PWD:/project" -w /project unityci/editor:ubuntu-2018.4.3f1-base-3 bash .github/ci/compile-check.sh
```

## Playing together

The Unity matchmaking/relay service the game originally used has been shut down. The game now
uses direct connections:

- **Host:** *Play → enter a room name → Create Room*. Your machine is the server (UDP port 7777).
  Your IP address is shown in the top-right corner in the pre-match lobby and pause menu (or press F1).
- **Join on the same network:** hosted games appear in the server list automatically
  (LAN discovery on UDP port 47777). Click one to join.
- **Join over the internet:** type the host's address in the *Host IP* field and press *Connect*.
  The host must either forward **UDP 7777** on their router or both players use a virtual LAN
  such as [Tailscale](https://tailscale.com) or [ZeroTier](https://zerotier.com) (then use the
  host's Tailscale/ZeroTier IP).
- Everyone must run the same build (the server list marks other versions). Joining is closed
  once the match has started.

Windows, macOS and Linux players can play together.

Command line (handy for testing two instances on one machine):

```bash
./TheNightmare -host -name "Test room" -port 7777
```

```bash
./TheNightmare -connect 127.0.0.1:7777
```
