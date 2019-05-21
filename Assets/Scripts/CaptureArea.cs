using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureArea : MonoBehaviour
{
    private bool _isCaptured = false;
    private List<GameObject> candles = new List<GameObject>();
    
    public bool IsCaptured
    {
        get => _isCaptured;
        set => _isCaptured = value;
    }

    void Start()
    {
        
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            candles.Add(transform.GetChild(0).GetChild(i).gameObject);
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("entering collision 0");
        if (other.CompareTag("Player"))
        {
            Debug.Log("entering collision 1");
            Debug.Log(candles.Count);
            foreach (var candle in candles)
            {
                Debug.Log("entering collision 2");
                candle.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    void Update()
    {
        
    }
}