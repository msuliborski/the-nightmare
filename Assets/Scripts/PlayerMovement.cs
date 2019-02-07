﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour {

    public float speed;

    private Rigidbody rb;

    private Camera mainCamera;

    void Start () {

        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        mainCamera.transform.SetParent(gameObject.transform, false);        
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate () {
        float moveHorizontal = Input.GetAxis ("Horizontal");
        float moveVertical = Input.GetAxis ("Vertical");

        Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

        rb.AddForce (movement * speed);
    }
}