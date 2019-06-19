using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class CapturePointsUI : NetworkBehaviour
{
    private List<TextMeshProUGUI> captures = new List<TextMeshProUGUI>();
    private List<CaptureArea> areas = new List<CaptureArea>();
    private bool isSet = false;
    
    void Start()
    {
        foreach (Transform counter in transform)
        {
            captures.Add(counter.GetChild(0).GetComponent<TextMeshProUGUI>());
        }
    }
    
    void Update()
    {
        
        if (isSet)
        {
            for (int i = 0; i < areas.Count; i++)
            {
                captures[i].text = ((int)areas[i]._progress).ToString();
            }
            //RpcUpdateText();
        }
    }

    public void setRoom()
    {
        areas = GameManager.Instance.CurrentRoom.CaptureAreas;

        isSet = true;
    }

    [ClientRpc]
    private void RpcUpdateText()
    {
        for (int i = 0; i < areas.Count; i++)
        {
            captures[i].text = areas[i]._progress.ToString();
        }
    }
}
