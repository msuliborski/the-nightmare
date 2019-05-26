using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureArea : MonoBehaviour
{
    private bool _isCaptured = false;
    private bool _isLocked = false;
    private bool _isConflict = false;
    public float _progress = 0;
    private float _max = 0;
    [SerializeField] private float _step = 0.2f;
    private bool _capturing = false;
    private int _capturingNum = 0;
    public int _enemyNum = 0;
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

        if (other.CompareTag("EnemyLegs"))
        {
            _enemyNum++;
            _isLocked = false;
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
        if (other.CompareTag("EnemyLegs"))
        {
            _enemyNum--;
            if (_enemyNum == 0)
                _isLocked = true;
        }
    }

    void Update()
    {
        if (_capturingNum > 0 && _enemyNum > 0)
            _isConflict = true;
        else
            _isConflict = false;
        
        if (_capturing)
            _step = Math.Abs(_step);

        else
            _step = -Math.Abs(_step);
        
        
        if (!_isLocked && !_isConflict)
        {
            

            _progress += _step;
            if (_progress < 0)
                _progress = 0;

            if (_progress >= 100)
            {
                _progress = 100;
                _isCaptured = true;
                _isLocked = true;
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
}