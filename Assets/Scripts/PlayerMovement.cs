using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour {

    public float speed;

    private Rigidbody rb;

    private Camera mainCamera;
    private Camera temp;

    void Start ()
    {

        temp = GameObject.Find("TempCamera").GetComponent<Camera>();
        temp.enabled = false;
        if (!isLocalPlayer){
            mainCamera = gameObject.transform.GetChild(0).gameObject.GetComponent<Camera>();
            mainCamera.gameObject.SetActive(false);
        }
        
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate () {
        float moveHorizontal = Input.GetAxis ("Horizontal");
        float moveVertical = Input.GetAxis ("Vertical");

        Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

        rb.AddForce (movement * speed);
    }
}