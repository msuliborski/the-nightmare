using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlacementController : NetworkBehaviour
{
    public static float GridTileSize = 1f;
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
    public Canvas GridCanvas { get; set; }

    [SerializeField] private float _scrollBorderThickness = 0.005f;  // percentage of screen height
    [SerializeField] private float _moveSpeedMinZoom = 30f;
    [SerializeField] private float _moveSpeedMaxZoom = 30f;
    private static float _zoom = 0f;

    public GameObject CurrentObject
    {
        get => _currentObject;
        set => _currentObject = value;
    }

    private void Start()
    {
        _reverseGrid = 1f / GridTileSize;
        _currentCamera = _buildingCamera = gameObject.transform.Find("BuildingCamera").GetComponent<Camera>();
        _actionCamera = gameObject.transform.Find("PlayerCamera").GetComponent<Camera>();
        GridCanvas = GameObject.Find("GridCanvas").GetComponent<Canvas>();
        for (int i = 0; i < GridCanvas.transform.childCount; i++)
        {
            _gridPoints.Add(GridCanvas.transform.GetChild(i).gameObject);
        }
    }

    private void Update()
    {

        HandleKeys();
        if (_currentObject != null)
        {
            MoveToMouse();
            ReleaseOnClick();
            RotateObject();
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
            if (GameManager.CurrentState == GameManager.GameState.Fighting)
                GridCanvas.gameObject.SetActive(false);
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

    void HandleKeys()
    {
        switch (GameManager.CurrentState)
        {
            case GameManager.GameState.Fighting:

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (_currentObject == null)
                    {
                        
                        GridCanvas.gameObject.SetActive(true);
                        Debug.Log(GridCanvas.gameObject.activeSelf);
                        _currentObject = Instantiate(_placeableObject);
                    }
                    else
                    {
                        GridCanvas.gameObject.SetActive(false);
                        Destroy(_currentObject);
                    }

                }
                break;

            case GameManager.GameState.Building:
            
                float xDelta = 0f, zDelta = 0f, rotationDelta = 0f;

                float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
                if (zoomDelta != 0f) AdjustZoom(zoomDelta);


                if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Comma)) rotationDelta++;
                if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Period)) rotationDelta--;

                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - _scrollBorderThickness) zDelta++;
                if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Input.mousePosition.y <= _scrollBorderThickness) zDelta--;
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - _scrollBorderThickness) xDelta++;
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.mousePosition.x <= _scrollBorderThickness) xDelta--;

                if (xDelta != 0f || zDelta != 0f) AdjustPositionMouse(xDelta, zDelta);
                //if (rotationDelta != 0f) AdjustRotationKeyboard(rotationDelta);

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (_currentObject == null)
                        _currentObject = Instantiate(_placeableObject);
                    else Destroy(_currentObject);

                }

                if (Input.GetKeyDown(KeyCode.R))
                    CmdRegisterBeingReady();
                break;
        }
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

    void AdjustPositionMouse(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float distance = Mathf.Lerp(_moveSpeedMinZoom, _moveSpeedMaxZoom, _zoom) * Time.deltaTime;
        Vector3 position = _buildingCamera.transform.localPosition;
        position += direction * distance;
        //transform.localPosition = ClampPosition(position);
        _buildingCamera.transform.localPosition = position;
    }


    void AdjustZoom(float delta)
    {
        _zoom = Mathf.Clamp01(_zoom + delta);

       // float distance = Mathf.Lerp(_stickMinZoom, _stickMaxZoom, _zoom);
       // came.localPosition = new Vector3(0f, 0f, distance);

    }
}
