using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureArea : MonoBehaviour
{
    private bool _isCaptured = false;
    public float _progress = 0;
    private float _max = 0;
    [SerializeField] private float _step = 0.2f;
    private bool _capturing = false;
    private int _capturingNum = 0;
    private List<GameObject> _candles = new List<GameObject>();
    private Sprite _red;
    private Sprite _green;
    private SpriteRenderer _renderer;

    public bool IsCaptured
    {
        get => _isCaptured;
        set => _isCaptured = value;
    }

    void Start()
    {
        _red = (Sprite)Resources.Load("red", typeof(Sprite));
        _green = (Sprite)Resources.Load("green", typeof(Sprite));
        _renderer = GetComponent<SpriteRenderer>();
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            _candles.Add(transform.GetChild(0).GetChild(i).gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _capturingNum++;
            _capturing = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _capturingNum--;
            if (_capturingNum == 0)
                _capturing = false;
        }
    }

    void Update()
    {
        if (_capturing)
            _step = Math.Abs(_step);

        else
            _step = -Math.Abs(_step);

        _progress += _step;
        if (_progress < 0)
            _progress = 0;

        if (_progress >= 100)
        {
            _progress = 100;
            _isCaptured = true;
            _renderer.sprite = _green;
        }
        else
        {
            _isCaptured = false;
            _renderer.sprite = _red;
        }


        if (_progress > 25)
        {
            _candles[0].transform.GetChild(0).gameObject.SetActive(true);
            if (_progress > 50)
            {
                _candles[1].transform.GetChild(0).gameObject.SetActive(true);
                if (_progress > 75)
                {
                    _candles[2].transform.GetChild(0).gameObject.SetActive(true);
                    if (_progress >= 100)
                    {
                        _candles[3].transform.GetChild(0).gameObject.SetActive(true);
                    }
                    else
                    {
                        _candles[3].transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
                else
                {
                    _candles[2].transform.GetChild(0).gameObject.SetActive(false);
                }
            }
            else
            {
                _candles[1].transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        else
        {
            _candles[0].transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}