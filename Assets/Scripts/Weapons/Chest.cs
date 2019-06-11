using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public bool active;
    public bool alreadyPicked;
    public int grenades;
    public int snares;
    private Room _room;

    private void Start()
    {
        _room = GetComponentInParent<Room>();
    }

    private void Update()
    {
        active = _room.roomCaptured;
    }
}
