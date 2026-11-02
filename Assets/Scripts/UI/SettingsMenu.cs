using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Settings window (IMGUI, so it needs no scene or prefab changes). It opens from a SETTINGS button that
// is added to the main menu at runtime (a copy of the Credits button with Resources/UI/settingsButton),
// and from a small button in the in-game pause menu.
public class SettingsMenu : MonoBehaviour
{
    private const string MAIN_MENU_SCENE = "MainMenu";
    private const string LOBBY_SCENE = "LobbyTests";
    private const float WINDOW_WIDTH = 440f;
    private const int WINDOW_ID = 0x5E77;

    private bool _open;
    private Rect _window;
    private Resolution[] _resolutions;
    private int _resolutionIndex;
    private bool _fullscreen;
    private GUIStyle _buttonStyle;
    private GUIStyle _windowStyle;
    private const float MENU_BUTTON_SPACING = 190f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("SettingsMenu");
        DontDestroyOnLoad(go);
        go.AddComponent<SettingsMenu>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddMainMenuButton();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddMainMenuButton();
    }

    // Main menu: Play / Credits / Settings / Quit, in the style of the other buttons.
    private void AddMainMenuButton()
    {
        if (SceneManager.GetActiveScene().name != MAIN_MENU_SCENE) return;
        GameObject credits = GameObject.Find("CreditsButton");
        GameObject quit = GameObject.Find("QuitButton");
        var sprite = Resources.Load<Sprite>("UI/settingsButton");
        if (credits == null || quit == null || sprite == null || GameObject.Find("SettingsButton") != null) return;

        var quitRect = (RectTransform) quit.transform;
        GameObject settings = Instantiate(credits, credits.transform.parent, false);
        settings.name = "SettingsButton";
        ((RectTransform) settings.transform).anchoredPosition = quitRect.anchoredPosition;
        quitRect.anchoredPosition += Vector2.down * MENU_BUTTON_SPACING;

        var button = settings.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(OpenWindow);
        foreach (Image image in settings.GetComponentsInChildren<Image>(true))
            if (image.sprite != null && image.sprite.name.ToLowerInvariant().Contains("credit")) image.sprite = sprite;
        button.image.sprite = sprite;
        // Swap-sprite transitions would bring the Credits art back on hover.
        if (button.transition == Selectable.Transition.SpriteSwap) button.transition = Selectable.Transition.ColorTint;
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
        if (_buttonStyle == null) CreateStyles();

        if (!_open)
        {
            // The main menu has its own SETTINGS button; the pause menu gets a small one.
            if (PauseGame.menuActive && GUI.Button(new Rect(10f, Screen.height - 50f, 140f, 40f), "Settings", _buttonStyle))
                OpenWindow();
            return;
        }
        GUI.skin.label.fontSize = 15;
        _window = GUILayout.Window(WINDOW_ID, _window, DrawWindow, "SETTINGS", _windowStyle);
    }

    private string _nameField;

    // Dark, almost opaque panel instead of the default grey IMGUI window, so it reads over the menu art.
    private void CreateStyles()
    {
        _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16 };
        var background = new Texture2D(1, 1);
        background.SetPixel(0, 0, new Color(0.11f, 0.09f, 0.08f, 0.96f));
        background.Apply();
        _windowStyle = new GUIStyle(GUI.skin.window)
        {
            fontSize = 22, fontStyle = FontStyle.Bold, padding = new RectOffset(18, 18, 44, 18)
        };
        _windowStyle.normal.background = background;
        _windowStyle.onNormal.background = background;
        _windowStyle.normal.textColor = _windowStyle.onNormal.textColor = new Color(0.86f, 0.78f, 0.65f);
    }

    private void DrawWindow(int id)
    {
        if (_nameField == null) _nameField = GameSettings.PlayerName;
        GUILayout.Label("Player name (applies when you join the next game)");
        _nameField = GUILayout.TextField(_nameField, GameSettings.MAX_NAME_LENGTH);
        GameSettings.PlayerName = _nameField;

        GUILayout.Space(8);
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
