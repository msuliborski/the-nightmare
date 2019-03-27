using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlacementController : NetworkBehaviour
{
    [SerializeField] private float _grid;
    [SerializeField] private GameObject _placeableObject;
    public GameObject PlaceableObject {get { return _placeableObject; } set { _placeableObject = value; }}
    private float _mouseWheelRotation;
    private float _x = 0, _y = 0, _reverseGrid;
    private GameObject _currentObject;
    private Camera _buildingCamera;
    private Camera _actionCamera;
    private Camera _currentCamera;
    [SerializeField] private GameObject _gridPointPrefab;
    private List<GameObject> _gridPoints = new List<GameObject>();
    private Canvas _gridCanvas;

    public GameObject CurrentObject
    {
        get => _currentObject;
        set => _currentObject = value;
    }

    private void Start()
    {
        _reverseGrid = 1f / _grid;
        _currentCamera = _buildingCamera = gameObject.transform.Find("BuildingCamera").GetComponent<Camera>();
        _actionCamera = gameObject.transform.Find("PlayerCamera").GetComponent<Camera>();
        _gridCanvas = GameObject.Find("GridCanvas").GetComponent<Canvas>();
        DrawGrid();
    }
    
    private void Update()
    {
        //if (GameManager.CurrentState == GameManager.GameState.Building)
        //{
            HandleKey();
            if (_currentObject != null)
            {
                MoveToMouse();
                ReleaseOnClick();
                RotateObject();
            }
        //}
    }


    void DrawGrid()
    {
        for (float x = -40f; x < 40f; x += _grid)
            for (float y = -40f; y < 40f; y += _grid)
            {
                _gridPoints.Add(Instantiate(_gridPointPrefab, new Vector3(x + 0.5f, 0.1f, y + 0.5f), Quaternion.Euler(90f, 0f, 0f), _gridCanvas.transform));
            }
          
    }


    void RotateObject()
    {
        _mouseWheelRotation = 0;
        _mouseWheelRotation += Input.mouseScrollDelta.y;
        
        if (_mouseWheelRotation >= 0.5)
        {
            _currentObject.transform.Rotate(Vector3.up, 90);
        }
        else if (_mouseWheelRotation <= -0.5)
        {
            _currentObject.transform.Rotate(Vector3.up, -90);
        }
    }

    void ReleaseOnClick()
    {
        if (Input.GetMouseButton(0))
        {
            CmdPlaceEntity(_currentObject.transform.position, _currentObject.transform.rotation);
            _currentObject.transform.GetComponent<BoxCollider>().enabled = true;
            _currentObject = null;
        }
    }

    [Command]
    void CmdPlaceEntity(Vector3 pos, Quaternion rot)
    {
        RpcPlaceEntity(pos, rot);
    }

    [ClientRpc]
    void RpcPlaceEntity(Vector3 pos, Quaternion rot)
    {
        if (!isLocalPlayer)
        {
            GameObject temp = Instantiate(_placeableObject, pos, rot);
            temp.GetComponent<BoxCollider>().enabled = true;
        }
    }
    
    
    void MoveToMouse()
    {
        if (_currentObject != null)
        {
            Ray ray = _currentCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                _x = Mathf.Round(hitInfo.point.x * _reverseGrid) / _reverseGrid;
                _y = Mathf.Round(hitInfo.point.z * _reverseGrid) / _reverseGrid;
                _currentObject.transform.position = new Vector3(_x, hitInfo.transform.position.y, _y);
                //_currentObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
        }
    }

    void HandleKey()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_currentObject == null)
                _currentObject = Instantiate(_placeableObject);
            else Destroy(_currentObject);
            
        }
        if (Input.GetKeyDown(KeyCode.R) && GameManager.CurrentState == GameManager.GameState.Building)
            CmdRegisterBeingReady();
    }

    [Command]
    void CmdRegisterBeingReady()
    {
        GameManager.Instance.ReadyPlayersCnt++;
        if (GameManager.Instance.ReadyPlayersCnt == GameManager.Players.Count)
           RpcRegisterBeingReady();
    }

    [ClientRpc]
    void RpcRegisterBeingReady()
    {
        if (isLocalPlayer) _currentCamera = _actionCamera;
        GameManager.CurrentState = GameManager.GameState.Fighting;
    }
}
