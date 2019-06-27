using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Skip : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private AudioSource _source; 

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
        _source.enabled = true;
        Destroy(gameObject);
    }
}
