using System;
using Mirror;
using UnityEditor;
using UnityEngine;

// Mirror identifies networked prefabs by an assetId and networked scene objects by a sceneId.
//  - assetIds are derived from the prefab's GUID, so every platform computes the same value. Prefabs
//    converted from UNET were never saved by the editor with one, so this writes them into the prefab
//    files. Runs before each build (NightmareBuild) and from the menu.
//  - sceneIds are stored in the scene files (the editor assigns one when you add a networked object to
//    a scene and save it; Tools/assign_scene_ids.py did it for the converted scenes). They must never
//    be generated during a build, or Windows, macOS and Linux builds would disagree and could not play
//    together. CI checks them with "Tools/assign_scene_ids.py --check".
public static class NetworkIdBaker
{
    [MenuItem("The Nightmare/Assign prefab network ids")]
    public static void BakeAll()
    {
        int count = 0, changed = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var identity = prefab != null ? prefab.GetComponent<NetworkIdentity>() : null;
            if (identity == null) continue;
            count++;

            var serialized = new SerializedObject(identity);
            SerializedProperty stored = serialized.FindProperty("_assetId");
            uint expected = NetworkIdentity.AssetGuidToUint(new Guid(guid));
            if (stored == null || stored.uintValue == expected) continue;
            stored.uintValue = expected;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SavePrefabAsset(prefab);
            changed++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[NetworkIdBaker] " + count + " networked prefabs, " + changed + " assetIds written.");
    }
}
