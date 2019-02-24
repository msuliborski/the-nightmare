using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Serialization;

public class JoinGame : MonoBehaviour
{
   [SerializeField] private GameObject _roomListInst;
   [FormerlySerializedAs("status")] [SerializeField] private Text _status;
   [SerializeField] private Transform _roomListParent;
   private List<GameObject> _roomList = new List<GameObject>();
   private NetworkManager _networkManager;

   void Start()
   {
      _networkManager = NetworkManager.singleton;
      if (_networkManager.matchMaker == null)
      {
         _networkManager.StartMatchMaker();
      }

      RefreshRoomList();
   }

   public void RefreshRoomList()
   {
      ClearRoomList();
      _networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
      _status.text = "Loading...";
   }

   public void OnMatchList(bool success, string extendedinfo, List<MatchInfoSnapshot> responsedata)
   {
      _status.text = "";

      if (responsedata == null)
      {
         _status.text = "Couldn't get room list";
         return;
      }

      foreach (MatchInfoSnapshot response in responsedata)
      {
         GameObject roomListItemGO = Instantiate(_roomListInst);
         roomListItemGO.transform.SetParent(_roomListParent);

         ServerListItem item = roomListItemGO.GetComponent<ServerListItem>();
         if (item != null)
         {
            item.Setup(response, JoinRoom);
         }
         
         
         _roomList.Add(roomListItemGO);
      }

      if (_roomList.Count == 0)
      {
         _status.text = "No servers found";
      }
   }

   void ClearRoomList()
   {
      for (int i = 0; i < _roomList.Count; i++)
      {
         Destroy(_roomList[i]);
      }
      
      _roomList.Clear();
   }

   public void JoinRoom(MatchInfoSnapshot match)
   {
      _networkManager.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, _networkManager.OnMatchJoined);
      ClearRoomList();
      _status.text = "Joining...";
      GameObject.Find("Lobby").SetActive(false);
   }
}
