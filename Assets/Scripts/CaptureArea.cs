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
    public int EnemyNum { get { return _enemyNum; } }
    private int candlesToLight;
    private List<GameObject> _candles = new List<GameObject>();
    private Sprite[] _sprites = new Sprite[2];
    private SpriteRenderer _renderer;
    private float _scale;
    // Progress was tuned as "_step per frame" at the game's 50 FPS cap; now it is per second.
    private const float STEPS_PER_SECOND = 50f;
    private readonly HashSet<EnemyControllerServer> _enemiesInside = new HashSet<EnemyControllerServer>();

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
        transform.tag = "CaptureArea";
        _progress = 100;
        _step = 0.05f;
        _sprites[0] = (Sprite) Resources.Load("red", typeof(Sprite));
        _sprites[1] = (Sprite) Resources.Load("green", typeof(Sprite));
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = _sprites[1];
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            _candles.Add(transform.GetChild(0).GetChild(i).gameObject);
            if(isServer) RpcActivateCandle(i, true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyLegs"))
        {
            EnemyControllerServer enemy = other.GetComponentInParent<EnemyControllerServer>();
            if (enemy.isActiveAndEnabled && !enemy.IsTriggerLocked && !enemy._isDying)
            {
                enemy.CurrentState = EnemyControllerServer.EnemyState.Blocked;
                enemy.TurnOnWalking(false);
                enemy.SetAnim("blocked", false);
                enemy.SetAnim("running", false);
                enemy.IsTriggerLocked = true;
                _enemiesInside.Add(enemy);
                _enemyNum = _enemiesInside.Count;
                _capturing = true;
            }
        }
    }

   

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EnemyLegs"))
        {
            EnemyControllerServer enemy = other.GetComponentInParent<EnemyControllerServer>();
            if (enemy.isActiveAndEnabled && _enemiesInside.Remove(enemy))
            {
                _enemyNum = _enemiesInside.Count;
                if (_enemyNum <= 0)
                    _capturing = false;
                enemy.TurnOnWalking(true);
                enemy.CurrentState = EnemyControllerServer.EnemyState.Running;
                enemy.SetAnim("running", true);
                //enemy.IsTriggerLocked = false;
            }
        }
    }

    // Enemies that die inside the area simply drop out of the set (their colliders are disabled,
    // so no OnTriggerExit). The old counter was decremented on death even for enemies that were
    // never counted, and could go negative or stay too high.
    void Update()
    {
        if (isServer && !_isCaptured)
        {
            _enemiesInside.RemoveWhere(enemy => enemy == null || enemy._isDying);
            _enemyNum = _enemiesInside.Count;
            _capturing = _enemyNum > 0;
            if (_enemyNum >= 4)
            {
                _scale = 3;
            }
            else if (_enemyNum >= 2)
            {
                _scale = 2;
            }
            else
            {
                _scale = 1;
            }

            if (_capturing)
            {
                _progress -= _step * _scale * STEPS_PER_SECOND * Time.deltaTime;
                if (_progress <= 0)
                {
                    _progress = 0;
                    IsCaptured = true;
                    RpcChangeSprite(0); // the old check compared against the wrong sprite, so it never turned red
                    GameManager.Instance.ServerSetMatchState(GameManager.MatchState.Lose);
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