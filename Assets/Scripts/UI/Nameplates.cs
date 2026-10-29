using UnityEngine;

// Names above other players. Downed teammates are always marked (through walls and at any distance)
// so they can be found and revived.
public class Nameplates : MonoBehaviour
{
    private const float MAX_NAME_DISTANCE = 30f;
    private const float HEAD_HEIGHT = 2.1f;

    private GUIStyle _style;
    private GUIStyle _shadow;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("Nameplates");
        DontDestroyOnLoad(go);
        go.AddComponent<Nameplates>();
    }

    private void OnGUI()
    {
        PlayerManager local = GameManager.LocalPlayer;
        if (local == null || WinLoseScreens.winLoseActive || !ButtonsControll.screensOver) return;
        Camera cam = local.GetComponent<PlayerSetup>().ActionCamera;
        if (cam == null || !cam.isActiveAndEnabled) return;

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Bold };
            _shadow = new GUIStyle(_style);
            _shadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
        }

        foreach (PlayerManager player in GameManager.Players.Values)
        {
            if (player == null || player == local) continue;

            Vector3 world = player.transform.position + Vector3.up * HEAD_HEIGHT;
            Vector3 screen = cam.WorldToScreenPoint(world);
            if (screen.z <= 0f) continue;
            float distance = Vector3.Distance(cam.transform.position, world);
            if (!player.IsDead && distance > MAX_NAME_DISTANCE) continue;

            string text = player.DisplayName;
            if (player.IsDead) text += "\nDOWN - revive (E)  " + Mathf.RoundToInt(distance) + " m";
            _style.normal.textColor = player.IsDead ? new Color(1f, 0.25f, 0.2f) : Color.white;

            var rect = new Rect(screen.x - 150f, Screen.height - screen.y - 20f, 300f, 40f);
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, _shadow);
            GUI.Label(rect, text, _style);
        }
    }
}
