using UnityEngine;

// Player preferences, persisted with PlayerPrefs and applied at startup.
public static class GameSettings
{
    private const string SENSITIVITY_KEY = "settings.mouseSensitivity";
    private const string INVERT_Y_KEY = "settings.invertY";
    private const string FOV_KEY = "settings.fieldOfView";
    private const string VOLUME_KEY = "settings.masterVolume";
    private const string QUALITY_KEY = "settings.quality";

    public const float DEFAULT_FOV = 90f;
    public const float MIN_FOV = 60f;
    public const float MAX_FOV = 110f;
    // Aiming down sights narrows the view by this much (the original 90 -> 50).
    public const float AIM_FOV_REDUCTION = 40f;

    // Multiplies the game's built-in look sensitivity.
    public static float MouseSensitivity
    {
        get { return PlayerPrefs.GetFloat(SENSITIVITY_KEY, 1f); }
        set { PlayerPrefs.SetFloat(SENSITIVITY_KEY, Mathf.Clamp(value, 0.1f, 3f)); }
    }

    public static bool InvertY
    {
        get { return PlayerPrefs.GetInt(INVERT_Y_KEY, 0) == 1; }
        set { PlayerPrefs.SetInt(INVERT_Y_KEY, value ? 1 : 0); }
    }

    public static float FieldOfView
    {
        get { return PlayerPrefs.GetFloat(FOV_KEY, DEFAULT_FOV); }
        set { PlayerPrefs.SetFloat(FOV_KEY, Mathf.Clamp(value, MIN_FOV, MAX_FOV)); }
    }

    public static float MasterVolume
    {
        get { return PlayerPrefs.GetFloat(VOLUME_KEY, 1f); }
        set
        {
            PlayerPrefs.SetFloat(VOLUME_KEY, Mathf.Clamp01(value));
            AudioListener.volume = Mathf.Clamp01(value);
        }
    }

    public static int QualityLevel
    {
        get { return PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel()); }
        set
        {
            int level = Mathf.Clamp(value, 0, QualitySettings.names.Length - 1);
            PlayerPrefs.SetInt(QUALITY_KEY, level);
            QualitySettings.SetQualityLevel(level, true);
        }
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyOnStartup()
    {
        AudioListener.volume = MasterVolume;
        if (PlayerPrefs.HasKey(QUALITY_KEY)) QualitySettings.SetQualityLevel(QualityLevel, true);
    }
}
