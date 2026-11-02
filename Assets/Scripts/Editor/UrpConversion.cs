using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Switches the project from the built-in render pipeline to URP with Unity's own "Render Pipeline
// Converter" (Window > Rendering > Render Pipeline Converter), run in batch mode:
//  1. RenderSettings: creates URP pipeline/renderer assets from the quality levels and assigns them.
//  2. Material / ReadonlyMaterial / AnimationClip: upgrades Standard, Legacy and particle materials.
// Custom shaders (dissolve, outline, KY effects, skybox) are not converted automatically; see
// docs/URP.md. Called before every build (NightmareBuild); does nothing once a pipeline is assigned.
// The converter API is reached by reflection so the license-free CI compile check needs no URP refs.
public static class UrpConversion
{
    private const string CONVERTERS_TYPE = "UnityEditor.Rendering.Universal.Converters, Unity.RenderPipelines.Universal.Editor";

    [MenuItem("The Nightmare/Convert project to URP")]
    public static void ConvertIfNeeded()
    {
        if (GraphicsSettings.defaultRenderPipeline != null) return;

        Type converters = Type.GetType(CONVERTERS_TYPE);
        MethodInfo run = converters?.GetMethod("RunInBatchMode", BindingFlags.Public | BindingFlags.Static, null,
                                               new[] { typeof(string), typeof(List<string>), typeof(bool) }, null);
        if (run == null) throw new InvalidOperationException("[URP] Render Pipeline Converter API not found.");

        Run(run, "RenderSettings");
        Run(run, "Material", "ReadonlyMaterial", "AnimationClip");
        AssetDatabase.SaveAssets();

        if (GraphicsSettings.defaultRenderPipeline == null)
            throw new InvalidOperationException("[URP] Conversion did not assign a URP asset.");
        Debug.Log("[URP] Converted to " + GraphicsSettings.defaultRenderPipeline.name);
    }

    private static void Run(MethodInfo run, params string[] converterIds)
    {
        Debug.Log("[URP] Running converters: " + string.Join(", ", converterIds));
        run.Invoke(null, new object[] { "BuiltInToURP", converterIds.ToList(), true });
    }
}
