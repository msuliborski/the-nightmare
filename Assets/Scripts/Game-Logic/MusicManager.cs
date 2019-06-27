using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip _slow;
    [SerializeField] private AudioClip _fast;
    private AudioSource _source;

    private void Start()
    {
        _source = GetComponent<AudioSource>();
    }

    public void ChangeClip(bool isSlow)
    {
        if (!isSlow)
        {
            _source.clip = _fast;
            _source.Play();
        }
        else
        {
            _source.clip = _slow;
            _source.Play();
        }
    }
}
