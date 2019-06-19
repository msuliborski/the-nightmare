using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private bool isSet = false;
    private GameObject target;
    void Update()
    {
        if (isSet)
        {
            Vector3 targetPosition = target.transform.position;
            targetPosition.y = transform.position.y;
            transform.LookAt(targetPosition);
        }
    }

    public void setTarget()
    {
        target = GameManager.Instance.CurrentRoom.gameObject;
    }
}
