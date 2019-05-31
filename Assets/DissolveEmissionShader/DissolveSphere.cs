using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveEnemeyBody : MonoBehaviour {

    Material mat;
    private EnemyControllerServer _enemyControllerServer;
    private float _dissolveAmount = 0f;
    public bool Enabled { get; set; }
    public float DissolveAmount { get => _dissolveAmount; }

    private void Start() {
        mat = GetComponent<Renderer>().material;
        Enabled = false;
        _enemyControllerServer = GetComponentInParent<EnemyControllerServer>();
    }

    private void Update() {
        if (Enabled)
        {
           _dissolveAmount += Time.deltaTime / 4;
            mat.SetFloat("_DissolveAmount", DissolveAmount);
            if (_dissolveAmount >= 1)
                Destroy(_enemyControllerServer.gameObject);
        }
    }
}