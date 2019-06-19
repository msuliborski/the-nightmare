﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class CapturePointsUI : NetworkBehaviour
{
    private List<TextMeshProUGUI> captures = new List<TextMeshProUGUI>();
    private List<GameObject> captureBG = new List<GameObject>();
    private List<CaptureArea> areas = new List<CaptureArea>();
    private bool isSet = false;
    private bool hasPlayed = false;
    private AudioSource source;
    
    void Start()
    {
        source = GetComponent<AudioSource>();
        foreach (Transform counter in transform)
        {
            captureBG.Add(counter.gameObject);
            captures.Add(counter.GetChild(0).GetComponent<TextMeshProUGUI>());
        }
    }
    
    void Update()
    {
        
        if (isSet)
        {
            for (int i = 0; i < areas.Count; i++)
            {
                if (!hasPlayed)
                {
                    if (areas[i]._progress < 20)
                    {
                        source.PlayOneShot(source.clip);
                        hasPlayed = true;
                    }
                }
                captures[i].text = ((int)areas[i]._progress).ToString();
            }
            //RpcUpdateText();
        }
    }

    public void setRoom()
    {
        areas = GameManager.Instance.CurrentRoom.CaptureAreas;

        for (int i = areas.Count; i < captures.Count; i++)
        {
            captureBG[i].SetActive(false);
        }
        for (int i = 0; i < areas.Count; i++)
        {
            captureBG[i].SetActive(true);
        }
        isSet = true;
    }

    [ClientRpc]
    private void RpcUpdateText()
    {
        for (int i = 0; i < areas.Count; i++)
        {
            captures[i].text = areas[i]._progress.ToString();
        }

        hasPlayed = false;
    }
}