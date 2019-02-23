using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class JoinGame : MonoBehaviour
{
   [SerializeField] private GameObject _roomListInst;
   [SerializeField] private Text status;
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
      _networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
      status.text = "Loading...";
   }

   public void OnMatchList(bool success, string extendedinfo, List<MatchInfoSnapshot> responsedata)
   {
      status.text = "";

      if (responsedata == null)
      {
         status.text = "Couldn't get room list";
         return;
      }

      ClearRoomList();
      foreach (var response in responsedata)
      {
         GameObject roomListItemGO = Instantiate(_roomListInst);
         roomListItemGO.transform.SetParent(_roomListParent);
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
}
