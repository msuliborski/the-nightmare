using System.Linq;
using System.Reflection;
using Mirror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Mirror identifies networked prefabs by an assetId and networked scene objects by a sceneId. Both are
// assigned by NetworkIdentity.OnValidate when an asset is opened and saved in the editor. Prefabs and
// scenes converted from UNET by script were never opened that way, so this assigns the missing ids and
// saves the assets. Runs before every build (NightmareBuild) and from the "The Nightmare" menu.
public static class NetworkIdBaker
{
    private static readonly MethodInfo SetupIds =
        typeof(NetworkIdentity).GetMethod("SetupIDs", BindingFlags.Instance | BindingFlags.NonPublic);

    [MenuItem("The Nightmare/Assign missing network ids")]
    public static void BakeAll()
    {
        int prefabs = BakePrefabs();
        int sceneObjects = BakeScenes(EditorBuildSettings.scenes.Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                                                               .Select(s => s.path).ToArray());
        Debug.Log("[NetworkIdBaker] Checked " + prefabs + " networked prefabs and " + sceneObjects + " networked scene objects.");
    }

    private static int BakePrefabs()
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            var identity = prefab.GetComponent<NetworkIdentity>();
            if (identity == null) continue;
            count++;
            if (identity.assetId != 0) continue; // the getter assigns it if missing
            Setup(identity);
            EditorUtility.SetDirty(prefab);
        }
        AssetDatabase.SaveAssets();
        return count;
    }

    private static int BakeScenes(string[] scenePaths)
    {
        if (scenePaths.Length == 0) return 0;
        string[] previous = Enumerable.Range(0, EditorSceneManager.sceneCount)
                                      .Select(i => EditorSceneManager.GetSceneAt(i).path)
                                      .Where(p => !string.IsNullOrEmpty(p)).ToArray();
        int count = 0;
        foreach (string path in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            bool changed = false;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (NetworkIdentity identity in root.GetComponentsInChildren<NetworkIdentity>(true))
                {
                    count++;
                    if (identity.sceneId != 0) continue;
                    Setup(identity);
                    EditorUtility.SetDirty(identity);
                    changed = true;
                    if (identity.sceneId == 0) Debug.LogError("[NetworkIdBaker] No sceneId for " + identity.name + " in " + path);
                }
            }
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
        if (previous.Length > 0) EditorSceneManager.OpenScene(previous[0], OpenSceneMode.Single);
        return count;
    }

    private static void Setup(NetworkIdentity identity)
    {
        if (SetupIds == null)
        {
            Debug.LogError("[NetworkIdBaker] NetworkIdentity.SetupIDs not found; did Mirror change?");
            return;
        }
        SetupIds.Invoke(identity, null);
    }
}
