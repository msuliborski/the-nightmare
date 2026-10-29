// Compile-check stub only; the real project uses com.unity.textmeshpro.
namespace TMPro {
  public class TMP_Text : UnityEngine.UI.MaskableGraphic { public string text; public float fontSize; public UnityEngine.Color color; public TextAlignmentOptions alignment; }
  public class TextMeshProUGUI : TMP_Text {}
  public class TextMeshPro : TMP_Text {}
  public enum TextAlignmentOptions { Center, Left }
  public class TMP_InputField : UnityEngine.UI.Selectable { public string text; }
  public class TMP_Dropdown : UnityEngine.UI.Selectable { public int value; }
}
