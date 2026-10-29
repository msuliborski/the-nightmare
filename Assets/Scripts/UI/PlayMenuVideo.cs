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
        _player.Prepare();
        // Wait until the clip is ready (the old loop gave up after one second and could show a blank texture).
        float timeout = Time.unscaledTime + 10f;
        while (!_player.isPrepared && Time.unscaledTime < timeout)
            yield return null;

        _image.texture = _player.texture;
        _player.Play();
    }
}
