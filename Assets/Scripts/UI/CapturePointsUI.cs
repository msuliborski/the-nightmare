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
        Transform temp = transform.GetChild(0);
        foreach (Transform counter in temp.transform)
        {
            captures.Add(counter.GetComponent<TextMeshProUGUI>());
        }
    }
    
    void Update()
    {
        if (isSet)
        {
            RpcUpdateText();
        }
    }

    public void setRoom()
    {
        Transform temp = GameManager.Instance._currentRoom.transform;
        foreach (Transform counter in temp.transform)
        {
            areas.Add(counter.GetComponent<CaptureArea>());
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
    }
}
