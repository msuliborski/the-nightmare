using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayMenuVideo : MonoBehaviour
{
    [SerializeField] private RawImage _image;
    [SerializeField] private VideoPlayer _player;
    
    void Start()
    {
        StartCoroutine(PlayVideo());
    }

    IEnumerator PlayVideo()
    {
        // Until a frame is available the RawImage has no texture, which Unity draws as plain white.
        _image.color = Color.black;
        bool failed = false;
        _player.errorReceived += (source, message) => failed = true;
        _player.Prepare();
        // Wait until the clip is ready (the old loop gave up after one second and could show a blank texture).
        float timeout = Time.unscaledTime + 10f;
        while (!_player.isPrepared && !failed && Time.unscaledTime < timeout)
            yield return null;

        if (!_player.isPrepared)
        {
            Debug.LogWarning("[Menu] Video could not be prepared; showing a dark background instead.");
            yield break;
        }
        _image.texture = _player.texture;
        _image.color = Color.white;
        _player.Play();
    }
}
