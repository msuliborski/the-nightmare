using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public static bool menuActive;
    private GameObject _pauseMenu;
    
    void Start()
    {
        _pauseMenu = GameObject.Find("PauseMenu");
        menuActive = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuActive = !menuActive;
        }

        _pauseMenu.SetActive(menuActive);
    }
}
