using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonsControll : MonoBehaviour
{
    public static bool screensOver = false;
    public void SkipWelcomeScreen()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(true);
    }

    public void SkipControls()
    {
        transform.GetChild(1).gameObject.SetActive(false);
        screensOver = true;
    }
}
