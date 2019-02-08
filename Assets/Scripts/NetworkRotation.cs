using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
 
public class NetworkRotation : NetworkBehaviour {
 
    [SyncVar]
    private Vector3 syncPos;
 
    [SyncVar]
    private float syncYRot;
 
    private Vector3 lastPos;
    private Quaternion lastRot;
    private Transform myTransform;
    [SerializeField]
    private float lerpRate = 10;
    [SerializeField]
    private float rotThreshold = 5;
 
    // Use this for initialization
    void Start()
    {
        myTransform = transform;
    }
 
    // Update is called once per frame
    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            TransmitMotion();
//            LerpMotion();
        }
        else
        {   
            LerpMotion();
        }
    }
 
    [Command]
    void Cmd_ProvidePositionToServer(Vector3 pos, float rot)
    {
        syncYRot = rot;
    }
 
    [ClientCallback]
    void TransmitMotion()
    {
        if(hasAuthority)
        {
            if (Quaternion.Angle(myTransform.rotation, lastRot) > rotThreshold)
            {
                Cmd_ProvidePositionToServer(myTransform.position, myTransform.localEulerAngles.y);
                lastRot = myTransform.rotation;
            }
        }
    }
 
    void LerpMotion()
    {
        if (!hasAuthority)
        {
            Vector3 newRot = new Vector3(0, syncYRot, 0);
            myTransform.rotation = Quaternion.Lerp(myTransform.rotation, Quaternion.Euler(newRot), Time.deltaTime * lerpRate);
        }
    }
}