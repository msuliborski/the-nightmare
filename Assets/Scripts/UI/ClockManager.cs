using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClockManager : MonoBehaviour
{
    public static bool canCount;
    public float time;
    private TextMeshProUGUI _minutesTM;
    private TextMeshProUGUI _secondsTM;

    private int _minutes;
    private int _seconds;
    private String _zero = "";
    
    void Start()
    {
        _minutesTM = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _secondsTM = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
    }
    
    void Update()
    {
        _zero = "";
        _minutes = (int) time / 60;
        _seconds = (int) time % 60;

        if (_minutes == 0 && _seconds == 0)
        {
            //WARUNEK PREZGRYWAJACY
            GameManager.Lose();
        }

        if (_minutes < 10)
            _zero = "0";
        _minutesTM.text = _zero + _minutes;
        _zero = "";

        if (_seconds < 10)
            _zero = "0";
        _secondsTM.text = _zero + _seconds;
        
        
        if (canCount)
        {
            time -= Time.deltaTime;
        }
    }
}
