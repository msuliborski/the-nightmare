using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayMenuVideo : MonoBehaviour
{
    private RawImage _image;
    private VideoPlayer _player;
    
    void Start()
    {
        _image = GameObject.Find("Background").GetComponent<RawImage>();
        _player = GetComponent<VideoPlayer>();
        StartCoroutine(PlayVideo());
    }

    IEnumerator PlayVideo()
    {
        _player.Prepare();
        WaitForSeconds wait = new WaitForSeconds(1);
        while (!_player.isPrepared)
        {
            yield return wait;
            break;
        }

        _image.texture = _player.texture;
        _player.Play();
    }
}
