using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skip : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(_player);
            Destroy(gameObject);
        }
    }
}
