using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CaptureArea : NetworkBehaviour
{
    [SyncVar] public bool _isCaptured = false;
    [SyncVar] public float _progress = 100;
    [SyncVar] [SerializeField] private float _step = 0.2f;
    [SyncVar] private bool _capturing = false;
    [SyncVar] private int _enemyNum = 0;
    private int candlesToLight;
    private List<GameObject> _candles = new List<GameObject>();
    private Sprite[] _sprites = new Sprite[2];
    private SpriteRenderer _renderer;
    private GameObject Room;

    public bool IsCaptured
    {
        get => _isCaptured;
        set
        {
            _isCaptured = value;
        }
    }

    void Start()
    {
        _progress = 100;
        _step = 0.05f;
        _sprites[0] = (Sprite) Resources.Load("red", typeof(Sprite));
        _sprites[1] = (Sprite) Resources.Load("green", typeof(Sprite));
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = _sprites[1];
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            _candles.Add(transform.GetChild(0).GetChild(i).gameObject);
            RpcActivateCandle(i, true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyLegs"))
        {
            EnemyControllerServer enemy = other.GetComponentInParent<EnemyControllerServer>();
            if (enemy.isActiveAndEnabled)
            {
                _enemyNum++;
                _capturing = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EnemyLegs"))
        {
            EnemyControllerServer enemy = other.GetComponentInParent<EnemyControllerServer>();
            if (enemy.isActiveAndEnabled)
            {
                _enemyNum--;
                if (_enemyNum <= 0)
                    _capturing = false;
            }
        }
    }

    [Command]
    public void CmdDecrementEnemies()
    {
        _enemyNum--;

    }
    
    void Update()
    {
        if (isServer)
        {
            if (_enemyNum <= 0)
            {
                _enemyNum = 0;
                _capturing = false;
            }
            else
                _capturing = true;
               
            
            if (_capturing)
            {
                _progress -= _step;
                if (_progress < 0)
                {
                    _progress = 0;
                    IsCaptured = true;
                    GameManager.Lose();
                    if (_renderer.sprite != _sprites[1])
                        RpcChangeSprite(0);
                }
                
                candlesToLight =  (int)(  _progress / 100 * _candles.Count);

                for (int i = 0; i < candlesToLight; i++)
                {
                    if (!_candles[i].transform.GetChild(0).gameObject.activeSelf)
                        RpcActivateCandle(i, true);
                }
                for (int i = candlesToLight; i < _candles.Count; i++)
                {
                    if (_candles[i].transform.GetChild(0).gameObject.activeSelf)
                        RpcActivateCandle(i, false);
                }
            }
        }
    }


    [ClientRpc]
    void RpcChangeSprite(int index)
    {
        _renderer.sprite = _sprites[index];
    }

    [ClientRpc]
    void RpcActivateCandle(int index, bool isOn)
    {
        _candles[index].transform.GetChild(0).gameObject.SetActive(isOn);
    }
}