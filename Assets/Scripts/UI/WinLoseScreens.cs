using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseScreens : MonoBehaviour
{
    private GameObject _win;
    private GameObject _lose;
    public static bool winLoseActive = false;

    private void Start()
    {
        _win = transform.GetChild(0).gameObject;
        _lose = transform.GetChild(1).gameObject;
    }

    public void ActivateScreen(bool isWin)
    {
        if(isWin)
            _win.SetActive(true);
        else
            _lose.SetActive(true);
        
        winLoseActive = true;
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
