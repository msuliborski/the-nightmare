using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Settings window (IMGUI, so it needs no scene or prefab changes). A "Settings" button shows up in the
// main menu, the server lobby and the in-game pause menu.
public class SettingsMenu : MonoBehaviour
{
    private const string MAIN_MENU_SCENE = "MainMenu";
    private const string LOBBY_SCENE = "LobbyTests";
    private const float WINDOW_WIDTH = 440f;

    private bool _open;
    private Rect _window;
    private Resolution[] _resolutions;
    private int _resolutionIndex;
    private bool _fullscreen;
    private GUIStyle _buttonStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("SettingsMenu");
        DontDestroyOnLoad(go);
        go.AddComponent<SettingsMenu>();
    }

    private bool CanOpen
    {
        get
        {
            string scene = SceneManager.GetActiveScene().name;
            return scene == MAIN_MENU_SCENE || scene == LOBBY_SCENE || PauseGame.menuActive;
        }
    }

    private void OpenWindow()
    {
        _open = true;
        _resolutions = Screen.resolutions
            .GroupBy(r => new { r.width, r.height })
            .Select(g => g.OrderByDescending(r => r.refreshRate).First())
            .ToArray();
        _resolutionIndex = System.Array.FindIndex(_resolutions,
            r => r.width == Screen.width && r.height == Screen.height);
        if (_resolutionIndex < 0) _resolutionIndex = _resolutions.Length - 1;
        _fullscreen = Screen.fullScreen;
        _window = new Rect((Screen.width - WINDOW_WIDTH) / 2f, Screen.height * 0.15f, WINDOW_WIDTH, 10f);
    }

    private void Update()
    {
        if (_open && (!CanOpen || Input.GetKeyDown(KeyCode.Escape)))
        {
            _open = false;
            GameSettings.Save();
        }
    }

    private void OnGUI()
    {
        if (!CanOpen) return;
        GUI.depth = -10;
        if (_buttonStyle == null) _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16 };

        if (!_open)
        {
            if (GUI.Button(new Rect(10f, Screen.height - 50f, 140f, 40f), "Settings", _buttonStyle)) OpenWindow();
            return;
        }
        _window = GUILayout.Window(GetInstanceID(), _window, DrawWindow, "Settings");
    }

    private void DrawWindow(int id)
    {
        GUILayout.Label("Mouse sensitivity: " + GameSettings.MouseSensitivity.ToString("0.00") + "x");
        GameSettings.MouseSensitivity = GUILayout.HorizontalSlider(GameSettings.MouseSensitivity, 0.1f, 3f);
        GameSettings.InvertY = GUILayout.Toggle(GameSettings.InvertY, " Invert mouse Y");

        GUILayout.Space(8);
        GUILayout.Label("Field of view: " + Mathf.RoundToInt(GameSettings.FieldOfView));
        GameSettings.FieldOfView = Mathf.Round(
            GUILayout.HorizontalSlider(GameSettings.FieldOfView, GameSettings.MIN_FOV, GameSettings.MAX_FOV));

        GUILayout.Space(8);
        GUILayout.Label("Volume: " + Mathf.RoundToInt(GameSettings.MasterVolume * 100f) + "%");
        float volume = GUILayout.HorizontalSlider(GameSettings.MasterVolume, 0f, 1f);
        if (!Mathf.Approximately(volume, GameSettings.MasterVolume)) GameSettings.MasterVolume = volume;

        GUILayout.Space(8);
        GUILayout.Label("Max FPS (ignored while V-Sync is on in the chosen quality level)");
        int[] fpsOptions = GameSettings.MAX_FPS_OPTIONS;
        int fpsIndex = System.Array.IndexOf(fpsOptions, GameSettings.MaxFps);
        string[] fpsLabels = System.Array.ConvertAll(fpsOptions, f => f == 0 ? "Unlimited" : f.ToString());
        int newFpsIndex = GUILayout.SelectionGrid(fpsIndex, fpsLabels, fpsOptions.Length);
        if (newFpsIndex != fpsIndex && newFpsIndex >= 0) GameSettings.MaxFps = fpsOptions[newFpsIndex];

        GUILayout.Space(8);
        string[] qualityNames = QualitySettings.names;
        if (qualityNames.Length > 0)
        {
            GUILayout.Label("Graphics quality");
            int quality = GUILayout.SelectionGrid(GameSettings.QualityLevel, qualityNames, Mathf.Min(3, qualityNames.Length));
            if (quality != GameSettings.QualityLevel) GameSettings.QualityLevel = quality;
        }

        if (_resolutions != null && _resolutions.Length > 0)
        {
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<", GUILayout.Width(30))) _resolutionIndex = Mathf.Max(0, _resolutionIndex - 1);
            Resolution r = _resolutions[_resolutionIndex];
            GUILayout.Label(r.width + " x " + r.height, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(">", GUILayout.Width(30))) _resolutionIndex = Mathf.Min(_resolutions.Length - 1, _resolutionIndex + 1);
            GUILayout.EndHorizontal();
            _fullscreen = GUILayout.Toggle(_fullscreen, " Fullscreen");
            if (GUILayout.Button("Apply resolution"))
                Screen.SetResolution(r.width, r.height, _fullscreen);
        }

        GUILayout.Space(12);
        if (GUILayout.Button("Close", _buttonStyle, GUILayout.Height(32)))
        {
            _open = false;
            GameSettings.Save();
        }
        GUI.DragWindow();
    }
}
