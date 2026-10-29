using UnityEngine;
using UnityEngine.Networking;

public class DissolveEnemyBody : MonoBehaviour {

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
        if (!Enabled || _dissolveAmount >= 1f) return;

        _dissolveAmount = Mathf.Min(1f, _dissolveAmount + Time.deltaTime / 3);
        mat.SetFloat("_DissolveAmount", DissolveAmount);
        // The enemy is a networked object: the server despawns it for everyone. Clients just stay
        // fully dissolved until the destroy message arrives instead of destroying their copy early.
        if (_dissolveAmount >= 1f && NetworkServer.active)
            NetworkServer.Destroy(_enemyControllerServer.gameObject);
    }
}
