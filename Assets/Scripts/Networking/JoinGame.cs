using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Server list (games found on the LAN) plus a "join by IP" field for internet / VPN play.
public class JoinGame : MonoBehaviour
{
   private const string LAST_ADDRESS_KEY = "LastJoinAddress";

   [SerializeField] private GameObject _roomListInst;
   [FormerlySerializedAs("status")] [SerializeField] private TextMeshProUGUI _status;
   [SerializeField] private Transform _roomListParent;
   [SerializeField] private GameObject _panel;
   [Tooltip("Input field cloned to create the address field")]
   [SerializeField] private InputField _addressFieldTemplate;
   [Tooltip("Button cloned (next to which the Connect button is placed)")]
   [SerializeField] private Button _refreshButton;
   [SerializeField] private Sprite _connectSprite;

   private readonly List<GameObject> _roomList = new List<GameObject>();
   private InputField _addressField;
   private CustomNetworkManager _networkManager;

   void Start()
   {
      _networkManager = CustomNetworkManager.Instance;
      CreateAddressUI();

      CustomNetworkManager.StatusChanged += OnStatusChanged;
      if (_networkManager != null)
      {
         _networkManager.Discovery.GamesChanged += RefreshRoomList;
         _networkManager.Discovery.StartListening();
      }
      RefreshRoomList();
   }

   void OnDestroy()
   {
      CustomNetworkManager.StatusChanged -= OnStatusChanged;
      if (_networkManager != null)
      {
         _networkManager.Discovery.GamesChanged -= RefreshRoomList;
         _networkManager.Discovery.StopListening();
      }
   }

   private void CreateAddressUI()
   {
      if (_addressFieldTemplate == null || _refreshButton == null) return;
      Transform column = _refreshButton.transform.parent;
      int index = _refreshButton.transform.GetSiblingIndex();

      _addressField = Instantiate(_addressFieldTemplate, column, false);
      _addressField.name = "JoinAddress";
      _addressField.gameObject.SetActive(true);
      // Drop the template's persistent bindings (it belongs to the "room name" field).
      _addressField.onValueChanged = new InputField.OnChangeEvent();
      _addressField.onEndEdit = new InputField.SubmitEvent();
      _addressField.onEndEdit.AddListener(_ =>
      {
         if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) JoinByAddress();
      });
      _addressField.contentType = InputField.ContentType.Standard;
      _addressField.characterLimit = 64;
      _addressField.text = PlayerPrefs.GetString(LAST_ADDRESS_KEY, "");
      Text placeholder = _addressField.placeholder as Text;
      if (placeholder != null) placeholder.text = "Host IP, e.g. 192.168.0.10";
      _addressField.transform.SetSiblingIndex(index);

      Button connect = Instantiate(_refreshButton, column, false);
      connect.name = "ConnectButton";
      connect.onClick = new Button.ButtonClickedEvent();
      connect.onClick.AddListener(JoinByAddress);
      if (_connectSprite != null) connect.image.sprite = _connectSprite;
      connect.transform.SetSiblingIndex(index + 1);
   }

   public void RefreshRoomList()
   {
      ClearRoomList();
      if (_networkManager == null)
      {
         _status.text = "Networking is not available.";
         return;
      }

      foreach (LanDiscovery.LanGame game in _networkManager.Discovery.Games.OrderBy(g => g.Name))
      {
         GameObject roomListItemGO = Instantiate(_roomListInst, _roomListParent, false);
         ServerListItem item = roomListItemGO.GetComponent<ServerListItem>();
         if (item != null) item.Setup(game, JoinRoom);
         _roomList.Add(roomListItemGO);
      }

      if (!string.IsNullOrEmpty(CustomNetworkManager.LastStatus)) _status.text = CustomNetworkManager.LastStatus;
      else if (_roomList.Count == 0)
         _status.text = _networkManager.Discovery.IsListening
            ? "Searching your network for games...\nOr type the host's IP below."
            : "LAN search unavailable. Type the host's IP below.";
      else _status.text = "";
   }

   private void OnStatusChanged(string status)
   {
      _status.text = status;
      // Connecting failed or was refused: let the player try again.
      if (_networkManager != null && !_networkManager.isNetworkActive) _panel.SetActive(false);
   }

   void ClearRoomList()
   {
      for (int i = 0; i < _roomList.Count; i++) Destroy(_roomList[i]);
      _roomList.Clear();
   }

   public void JoinRoom(LanDiscovery.LanGame game)
   {
      Connect(game.Address, game.Port);
   }

   public void JoinByAddress()
   {
      string host;
      int port;
      if (_addressField == null || !CustomNetworkManager.TryParseAddress(_addressField.text, out host, out port))
      {
         _status.text = "Enter the host's IP address, e.g. 192.168.0.10 or 192.168.0.10:7777";
         return;
      }
      PlayerPrefs.SetString(LAST_ADDRESS_KEY, _addressField.text.Trim());
      PlayerPrefs.Save();
      Connect(host, port);
   }

   private void Connect(string host, int port)
   {
      if (_networkManager == null || _networkManager.isNetworkActive) return;
      _panel.SetActive(true);
      _panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Joining " + host + "...";
      _networkManager.Join(host, port);
   }
}
