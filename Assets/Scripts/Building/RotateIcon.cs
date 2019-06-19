using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateIcon : MonoBehaviour {
    private float _playerIconRotation = 0;
    private Quaternion _thisRotation;
    
    void Start() {
//        _playerIconRotation = GameManager.LocalPlayer.transform.rotation.eulerAngles.y;
        _thisRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update() {
        if (GameManager.LocalPlayer != null) _playerIconRotation = GameManager.LocalPlayer.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(90, _thisRotation.y, -_playerIconRotation);
    }
}
