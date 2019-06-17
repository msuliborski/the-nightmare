using UnityEngine;

public class Snares : MonoBehaviour
{
    public float freezeTime;
    [SerializeField] private int _enemiesToDestroy = 3;
    public int EnemiesToDestroy { get { return _enemiesToDestroy; } set { _enemiesToDestroy = value; } }
    public int EnemiesCounter { get; set; }
    public GameManager.PosAndTag InitialPosAndTag;

    private void Awake()
    {
        EnemiesCounter = 0;
    }
}
