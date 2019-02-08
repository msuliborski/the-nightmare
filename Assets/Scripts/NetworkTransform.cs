using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
 
public class NetworkTransform : NetworkBehaviour {
 
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
    private float posThreshold = 0.5f;
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
        TransmitMotion();
        LerpMotion();
    }
 
    [Command]
    void Cmd_ProvidePositionToServer(Vector3 pos, float rot)
    {
        syncPos = pos;
        syncYRot = rot;
    }
 
    [ClientCallback]
    void TransmitMotion()
    {
        if(hasAuthority)
        {
            if (Vector3.Distance(myTransform.localPosition, lastPos) > posThreshold || Quaternion.Angle(myTransform.localRotation, lastRot) > rotThreshold)
            {
                Cmd_ProvidePositionToServer(myTransform.localPosition, myTransform.localEulerAngles.y);
 
                lastPos = myTransform.localPosition;
                lastRot = myTransform.localRotation;
            }
        }
    }
 
    void LerpMotion()
    {
        if (!hasAuthority)
        {
            myTransform.localPosition = Vector3.Lerp(myTransform.transform.localPosition, syncPos, Time.deltaTime * lerpRate);
 
            Vector3 newRot = new Vector3(0, syncYRot, 0);
            myTransform.localRotation = Quaternion.Lerp(myTransform.localRotation, Quaternion.Euler(newRot), Time.deltaTime * lerpRate);
        }
    }
}