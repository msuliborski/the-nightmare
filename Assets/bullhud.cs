using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class bullhud : MonoBehaviour
{
    private TextMeshProUGUI _text;
    public PlayerEquipment player;
    public bool playerEnabled = false;

    void Start()
    {
        _text = gameObject.GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        if(playerEnabled)
            _text.text = player.Weapon.CurrentMagAmmo + "/" + (player.Weapon.CurrentAmmo - player.Weapon.CurrentMagAmmo);
    }
}
