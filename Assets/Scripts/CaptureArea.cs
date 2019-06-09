using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CaptureArea : NetworkBehaviour
{
    [SyncVar] public bool _isCaptured = false;
    [SyncVar] private bool _isLocked = false;
    [SyncVar] private bool _isConflict = false;
    [SyncVar] public float _progress = 0;
    [SyncVar] private float _max = 0;
    [SyncVar] [SerializeField] private float _step = 0.2f;
    [SyncVar] private bool _capturing = false;
    [SyncVar] private int _capturingNum = 0;
    [SyncVar] private int _enemyNum = 0;
    private int candlesToLight;
    private List<GameObject> _candles = new List<GameObject>();
    private Sprite[] _sprites = new Sprite[2];
    private SpriteRenderer _renderer;

    public bool IsCaptured
    {
        get => _isCaptured;
        set => _isCaptured = value;
    }

    void Start()
    {
        _sprites[0] = (Sprite) Resources.Load("red", typeof(Sprite));
        _sprites[1] = (Sprite) Resources.Load("green", typeof(Sprite));
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
            PlayerManager playerManager = other.GetComponentInParent<PlayerManager>();
            if (playerManager.isLocalPlayer)
            {
                CmdPlayerInsideCaptureZone();
            }
        }

        if (other.CompareTag("EnemyLegs"))
        {
            EnemyControllerServer enemy = other.GetComponentInParent<EnemyControllerServer>();
            if (enemy.isActiveAndEnabled)
            {
                _enemyNum++;
                _isLocked = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerManager playerManager = other.GetComponentInParent<PlayerManager>();
            if (playerManager.isLocalPlayer)
            {
                CmdPlayerOutsideCaputerZone();
            }
        }

        if (other.CompareTag("EnemyLegs"))
        {
            EnemyControllerServer enemy = other.GetComponentInParent<EnemyControllerServer>();
            if (enemy.isActiveAndEnabled)
            {
                _enemyNum--;
                if (_enemyNum == 0)
                    _isLocked = true;
            }
        }
    }

    [Command]
    public void CmdDecrementEnemies()
    {
        _enemyNum--;
    }

    [Command]
    void CmdPlayerOutsideCaputerZone()
    {
        _capturingNum--;
        if (_capturingNum == 0)
            _capturing = false;
    }

    [Command]
    void CmdPlayerInsideCaptureZone()
    {
        _capturingNum++;
        _capturing = true;
    }


    void Update()
    {
        if (isServer)
        {
            if (_capturingNum < 0)
                _capturingNum = 0;
            if (_enemyNum < 0)
                _enemyNum = 0;
            
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
                    if (_renderer.sprite != _sprites[1])
                        RpcChangeSprite(1);
                }
                else
                {
                    _isCaptured = false;
                    if (_renderer.sprite != _sprites[0])
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
//                if (_progress > 25)
//                {
//                    if (!_candles[0].transform.GetChild(0).gameObject.activeSelf)
//                        RpcActivateCandle(0, true);
//                    if (_progress > 50)
//                    {
//                        if (!_candles[1].transform.GetChild(0).gameObject.activeSelf)
//                            RpcActivateCandle(1, true);
//                        if (_progress > 75)
//                        {
//                            if (!_candles[2].transform.GetChild(0).gameObject.activeSelf)
//                                RpcActivateCandle(2, true);
//                            if (_progress >= 100)
//                            {
//                                if (!_candles[3].transform.GetChild(0).gameObject.activeSelf)
//                                    RpcActivateCandle(3, true);
//                            }
//                            else
//                            {
//                                if (_candles[3].transform.GetChild(0).gameObject.activeSelf)
//                                    RpcActivateCandle(3, false);
//                            }
//                        }
//                        else
//                        {
//                            if (_candles[2].transform.GetChild(0).gameObject.activeSelf)
//                                RpcActivateCandle(2, false);
//                        }
//                    }
//                    else
//                    {
//                        if (_candles[1].transform.GetChild(0).gameObject.activeSelf)
//                            RpcActivateCandle(1, false);
//                    }
//                }
//                else
//                {
//                    if (_candles[0].transform.GetChild(0).gameObject.activeSelf)
//                        RpcActivateCandle(0, false);
//                }
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