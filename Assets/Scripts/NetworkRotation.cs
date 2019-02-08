using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
 
public class NetworkRotation : NetworkBehaviour {
 
//    [SyncVar]
//    private Vector3 syncPos;
 
    [SyncVar]
    private float syncXRot;
 
    //private Vector3 lastPos;
//    private Quaternion lastRot;
    private Transform myTransform;
    [SerializeField]
    private float lerpRate = 10;
//    [SerializeField]
    //private float posThreshold = 0.5f;
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

 
    [ClientCallback]
    void TransmitMotion()
    {
        if(localPlayerAuthority)
        {
//            if (Quaternion.Angle(myTransform.localRotation, lastRot) > rotThreshold)
//            {
// 
                //syncPos = myTransform.localPosition;
                syncXRot = myTransform.localEulerAngles.x;
                
                //lastPos = myTransform.localPosition;
//                lastRot = myTransform.localRotation;
//            }
        }
    }
 
    void LerpMotion()
    {
        if (!localPlayerAuthority)
        {
            //myTransform.localPosition = Vector3.Lerp(myTransform.transform.localPosition, syncPos, Time.deltaTime * lerpRate);
 
            Vector3 newRot = new Vector3(syncXRot, 0, 0);
            myTransform.localRotation = Quaternion.Lerp(myTransform.localRotation, Quaternion.Euler(newRot), Time.deltaTime * lerpRate);
        }
    }
}