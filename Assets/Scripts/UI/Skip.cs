using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Skip : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private AudioSource _source; 

    private const float START_TIMEOUT = 12f;
    private float _startDeadline;

    private void Start()
    {
        VideoPlayer video = _player.GetComponent<VideoPlayer>();
        video.loopPointReached += Kill;
        // A trailer that cannot play would otherwise leave the player on a blank screen until Esc.
        video.errorReceived += (source, message) => Kill(source);
        _startDeadline = Time.unscaledTime + START_TIMEOUT;
    }

    void Update()
    {
        VideoPlayer video = _player.GetComponent<VideoPlayer>();
        if (Input.GetKeyDown(KeyCode.Escape) || (!video.isPlaying && Time.unscaledTime > _startDeadline))
        {
            Kill(video);
        }
    }

    private bool _killed;

    private void Kill(VideoPlayer vp)
    {
        if (_killed) return;
        _killed = true;
        Destroy(_player);
        _source.enabled = true;
        Destroy(gameObject);
    }
}
