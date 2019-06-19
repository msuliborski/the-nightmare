using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestAlwaysFull : MonoBehaviour
{
    public bool active = true;
    public bool alreadyPicked = false;
    public int grenades;
    public int snares;
    public int teddyBears;
    public int barrels;
    private Room _room;

    private void Start()
    {
    }

    private void Update() {
        alreadyPicked = false;
    }
}
