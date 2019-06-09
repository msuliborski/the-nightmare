using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;

public class CapturedRooms : MonoBehaviour
{
    private List<Room> rooms = new List<Room>();
    private List<Image> heads = new List<Image>();
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            rooms.Add(transform.GetChild(i).GetComponent<Room>());
        }

        Transform roomCounter = GameObject.Find("RoomCounters").transform;
        for (int i = 0; i < roomCounter.childCount; i++)
        {
            heads.Add(roomCounter.GetChild(i).GetComponent<Image>());
        }
    }
    
    void Update()
    {
        bool check = true;
        int roomsToDisable = 0;
        
        foreach (Room room in rooms)
        {
            if (!room.roomCaptured)
            {
                check = false;
            }
            else
            {
                roomsToDisable++;
            }
        }

        for (int i = 0; i < roomsToDisable; i++)
        {
            heads[i].enabled = false;
        }
        for (int i = roomsToDisable; i < heads.Count; i++)
        {
            heads[i].enabled = true;
        }

        if (check)
        {
            //WARUNEK WYGRYWANIA
            GameManager.Win();
            Debug.Break();
        }
    }
}
