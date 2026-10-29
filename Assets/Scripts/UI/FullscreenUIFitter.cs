using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Full-screen UI art (welcome/tutorial screens, menu title) was authored as fixed 1920x1080 rects. On
// screens that are not 16:9 (e.g. 16:10 MacBooks, 4:3, ultrawide) the canvas is taller or wider than
// that, so those screens floated in the middle with gaps around them. This scales such rects up just
// enough to cover the whole canvas, like CSS "background-size: cover", without touching other UI.
public class FullscreenUIFitter : MonoBehaviour
{
    private const float SIZE_TOLERANCE = 0.05f;
    private const float RESCAN_INTERVAL = 1f;

    private readonly Dictionary<RectTransform, Vector3> _originalScales = new Dictionary<RectTransform, Vector3>();
    private Vector2Int _lastScreenSize;
    private float _nextScan;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("FullscreenUIFitter");
        DontDestroyOnLoad(go);
        go.AddComponent<FullscreenUIFitter>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _originalScales.Clear();
        _nextScan = 0f;
    }

    private void LateUpdate()
    {
        var screenSize = new Vector2Int(Screen.width, Screen.height);
        if (screenSize == _lastScreenSize && Time.unscaledTime < _nextScan) return;
        _lastScreenSize = screenSize;
        // Rescan periodically too: UI prefabs (e.g. the in-game HUD) can appear after the scene loads.
        _nextScan = Time.unscaledTime + RESCAN_INTERVAL;
        FitAll();
    }

    private void FitAll()
    {
        foreach (CanvasScaler scaler in FindObjectsOfType<CanvasScaler>())
        {
            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) continue;
            var canvasRect = scaler.GetComponent<RectTransform>();
            if (canvasRect == null) continue;
            Fit(canvasRect, canvasRect.rect.size, scaler.referenceResolution);
        }
    }

    // Walks the hierarchy (including plain Transform containers such as "Screens") and stops at the
    // first match so nested full-screen children are not scaled twice.
    private void Fit(Transform parent, Vector2 canvasSize, Vector2 reference)
    {
        foreach (Transform child in parent)
        {
            var rect = child as RectTransform;
            if (rect != null && IsFixedFullscreen(rect, canvasSize, reference))
            {
                Vector3 original;
                if (!_originalScales.TryGetValue(rect, out original))
                {
                    original = rect.localScale;
                    _originalScales[rect] = original;
                }
                float cover = Mathf.Max(canvasSize.x / reference.x, canvasSize.y / reference.y, 1f);
                rect.localScale = original * cover;
                continue;
            }
            Fit(child, canvasSize, reference);
        }
    }

    // The actual size, not sizeDelta: the tutorial screens use stretch anchors but sit under plain
    // Transforms, so they still end up exactly 1920x1080. A rect that really follows the canvas has the
    // canvas size instead and is left alone (on a 16:9 screen the cover factor is 1 anyway).
    private static bool IsFixedFullscreen(RectTransform rect, Vector2 canvasSize, Vector2 reference)
    {
        Vector2 size = rect.rect.size;
        bool isReferenceSized = Mathf.Abs(size.x - reference.x) <= reference.x * SIZE_TOLERANCE
                                && Mathf.Abs(size.y - reference.y) <= reference.y * SIZE_TOLERANCE;
        bool followsCanvas = Mathf.Abs(size.x - canvasSize.x) < 1f && Mathf.Abs(size.y - canvasSize.y) < 1f;
        return isReferenceSized && !followsCanvas;
    }
}
