using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClipsManager : MonoBehaviour
{
    private List<Image> _clips = new List<Image>();
    public PlayerEquipment player;
    public bool playerEnabled;
    private int _fullClips = 4;

    void Start()
    {
        foreach (Transform child in transform)
        {
            _clips.Add(child.GetComponent<Image>());
        }
    }
    
    void Update()
    {
        if (playerEnabled)
        {
            int ratio = (player.Weapon.CurrentAmmo - player.Weapon.CurrentMagAmmo)/ player.Weapon.MaxMagAmmo;
            
            for (int i = 0; i < ratio; i++)
            {
                _clips[i].enabled = true;
            }
            for (int i = ratio; i < _fullClips; i++)
            {
                _clips[i].enabled = false;
            }
        }
    }
}