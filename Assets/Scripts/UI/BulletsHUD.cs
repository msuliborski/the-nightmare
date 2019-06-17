using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BulletsHUD : MonoBehaviour {
    private TextMeshProUGUI _text;
    public PlayerEquipment Equipment;
    public bool playerEnabled = false;

    void Start() {
        _text = gameObject.GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        if (playerEnabled)
            if (Equipment.getActiveWeapon().CurrentAmmo < 1000)
                _text.text = Equipment.getActiveWeapon().CurrentMagAmmo + "/" +
                             (Equipment.getActiveWeapon().CurrentAmmo - Equipment.getActiveWeapon().CurrentMagAmmo);
            else
                _text.text = Equipment.getActiveWeapon().CurrentMagAmmo + "/inf.";// + "/\u221E"; //znak infinity nieobsługiwany przez czcionkę
    }
}