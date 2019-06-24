using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Skip : MonoBehaviour
{
    [SerializeField] private GameObject _player;

    private void Start()
    {
        _player.GetComponent<VideoPlayer>().loopPointReached += Kill;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Kill(_player.GetComponent<VideoPlayer>());
        }
    }

    private void Kill(VideoPlayer vp)
    {
        Destroy(_player);
        Destroy(gameObject);
    }
}
