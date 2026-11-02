using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// The game layers cameras the built-in way: the world camera clears the screen and cameras that clear
// "depth only" (the first-person weapon, UI cameras) draw on top. URP ignores that setting: every
// camera is a separate Base camera, so the last one cleared the screen to the background colour (the
// "all blue" view). This turns the depth-only cameras into Overlay cameras on the stack of the base
// camera rendering to the same target. Reflection keeps it free of compile-time URP references (the CI
// compile check), and it does nothing under the built-in pipeline.
public class UrpCameraStack : MonoBehaviour
{
    private static readonly Type DataType =
        Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (DataType == null || GraphicsSettings.currentRenderPipeline == null) return;
        var go = new GameObject("UrpCameraStack");
        DontDestroyOnLoad(go);
        go.AddComponent<UrpCameraStack>();
    }

    private IEnumerator Start()
    {
        // Cameras appear with the player prefab after the scene loads, so keep checking.
        var wait = new WaitForSecondsRealtime(0.5f);
        while (true)
        {
            Restack();
            yield return wait;
        }
    }

    private static void Restack()
    {
        Camera[] cameras = Camera.allCameras;
        foreach (Camera overlay in cameras.Where(c => c.clearFlags == CameraClearFlags.Depth || c.clearFlags == CameraClearFlags.Nothing))
        {
            Camera baseCamera = cameras
                .Where(c => c != overlay && c.targetTexture == overlay.targetTexture
                            && (c.clearFlags == CameraClearFlags.Skybox || c.clearFlags == CameraClearFlags.SolidColor)
                            && c.depth < overlay.depth)
                .OrderByDescending(c => c.depth)
                .FirstOrDefault();
            if (baseCamera == null) continue;

            Component overlayData = Data(overlay);
            PropertyInfo renderType = DataType.GetProperty("renderType");
            if (Convert.ToInt32(renderType.GetValue(overlayData)) != 1)
                renderType.SetValue(overlayData, Enum.ToObject(renderType.PropertyType, 1)); // CameraRenderType.Overlay

            var stack = DataType.GetProperty("cameraStack").GetValue(Data(baseCamera)) as System.Collections.Generic.List<Camera>;
            if (stack != null && !stack.Contains(overlay))
            {
                stack.Add(overlay);
                stack.Sort((a, b) => a.depth.CompareTo(b.depth));
            }
        }
    }

    private static Component Data(Camera camera)
    {
        return camera.GetComponent(DataType) ?? camera.gameObject.AddComponent(DataType);
    }
}
