using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public static bool menuActive;
    private GameObject _pauseMenu;
//    private GameObject[] _mobileControl = new GameObject[2];
    
    private void Start()
    {
//        _mobileControl[0] = GameObject.Find("Move");
//        _mobileControl[1] = GameObject.Find("Look");
//#if !UNITY_ANDROID
//        _mobileControl[0].SetActive(false);
//        _mobileControl[1].SetActive(false);
//#endif
        _pauseMenu = GameObject.Find("PauseMenu");
        menuActive = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuActive = !menuActive;
        }

        _pauseMenu.SetActive(menuActive);

    }

    public void Disconnect()
    {
        menuActive = false;
        if (CustomNetworkManager.Instance != null) CustomNetworkManager.Instance.Leave();
    }
}
