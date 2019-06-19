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
        Debug.Log(_playerIconRotation);
//        transform.rotation = Quaternion.Euler(_thisRotation.x, _thisRotation.y, _thisRotation.z);
//        transform.rotation = Quaternion.Euler(_playerIconRotation, _thisRotation.y, _thisRotation.z);
//        transform.rotation = Quaternion.Euler(_thisRotation.x, _playerIconRotation, _thisRotation.z);
        transform.rotation = Quaternion.Euler(90, _thisRotation.y, -_playerIconRotation);
    }
}
