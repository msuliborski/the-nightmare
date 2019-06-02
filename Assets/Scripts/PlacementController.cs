using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlacementController : NetworkBehaviour
{
    public static float GridTileSize = 1f;
    [SerializeField] private GameObject _placeableObject;
    public GameObject PlaceableObject {get { return _placeableObject; } set { _placeableObject = value; }}
    private float _mouseWheelRotation;
    private float _x = 0, _y = 0, _reverseGrid, _camMinZoom, _camMaxZoom;
    private GameObject _currentObject;
    private Transform _buildingCameraHolder;
    private Camera _buildingCamera;
    private float _buildingCameraAngle = 0f;
    private Camera _actionCamera;
    private Camera _currentCamera;
    [SerializeField] private GameObject _gridPointPrefab;
    [SerializeField] private float _scrollBorderThickness = 0.005f;  // percentage of screen height
    [SerializeField] private float _moveSpeedMinZoom = 30f;
    [SerializeField] private float _moveSpeedMaxZoom = 30f;
    [SerializeField] private float _rotationSpeedKeyboard = 150f;
    private static float _zoom = 1f;

   private PlayerShoot _playerShoot;

    public GameObject CurrentObject
    {
        get => _currentObject;
        set => _currentObject = value;
    }

    private void Start()
    {
        _playerShoot = GetComponent<PlayerShoot>();
        _reverseGrid = 1f / GridTileSize;
        _buildingCameraHolder = gameObject.transform.Find("BuildingCameraHolder");
        _currentCamera = _buildingCamera = _buildingCameraHolder.GetComponentInChildren<Camera>();
        _camMaxZoom = _buildingCamera.transform.position.y;
        _camMinZoom = 1f;
        _actionCamera = gameObject.transform.Find("PlayerCamera").GetComponent<Camera>();
        
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
            Vector3 pos = new Vector3(_currentObject.transform.position.x, _currentObject.transform.position.y, _currentObject.transform.position.z);
            if (GameManager.Instance.BuildingPoints.ContainsKey(pos)
                && GameManager.Instance.BuildingPoints[pos].Buildable
                )
            {

                if (GameManager.CurrentState == GameManager.GameState.Fighting)
                    GameManager.TurnOnGridRenders(false);
                    
                CmdPlaceEntity(_currentObject.transform.position, _currentObject.transform.rotation);
                _currentObject.transform.GetComponent<BoxCollider>().enabled = true;
                GameManager.Instance.BuildingPoints[pos].Buildable = false;
                _currentObject = null;
                _playerShoot.WasBuilt = true;
            }
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
            Vector3 pos1 = new Vector3(temp.transform.position.x, temp.transform.position.y, temp.transform.position.z);
            GameManager.Instance.BuildingPoints[pos1].Buildable = false;
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

                if (Input.GetKeyDown(KeyCode.P))
                {
                    if (_currentObject == null)
                    {
                        GameManager.TurnOnGridRenders(true);
                        _currentObject = Instantiate(_placeableObject);
                        _playerShoot.IsBuildingOnFly = true;
                    }
                    else
                    {
                        GameManager.TurnOnGridRenders(false);
                        Destroy(_currentObject);
                        _playerShoot.IsBuildingOnFly = false;
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
                if (rotationDelta != 0f) AdjustRotationKeyboard(rotationDelta);

                if (Input.GetKeyDown(KeyCode.P))
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
           RpcRegisterLocalPlayerBeingReady(); // rpc bedzie wywolywana na obiketach symbolizujacych ostatniego playera ktory zadeklarowal sie 'ready'
    }

    [ClientRpc]
    void RpcRegisterLocalPlayerBeingReady() 
    {
        PlacementController localPlayer = GameManager.LocalPlayer.GetComponent<PlacementController>(); // dlatego musimy wyluskac local playera
        localPlayer._currentCamera = localPlayer._actionCamera;
        if (localPlayer._currentObject != null) Destroy(localPlayer._currentObject);
        GameManager.CurrentState = GameManager.GameState.Fighting;
    }

    void AdjustPositionMouse(float xDelta, float zDelta)
    {
        Vector3 direction = _buildingCameraHolder.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float distance = Mathf.Lerp(_moveSpeedMinZoom, _moveSpeedMaxZoom, _zoom) * Time.deltaTime;
        Vector3 position = _buildingCameraHolder.localPosition;
        position += direction * distance;
        //transform.localPosition = ClampPosition(position);
        _buildingCameraHolder.localPosition = position;
    }


    void AdjustZoom(float delta)
    {
        delta *= -1f;
        _zoom = Mathf.Clamp01(_zoom + delta);

       float distance = Mathf.Lerp(_camMinZoom, _camMaxZoom, _zoom);
       _buildingCamera.transform.localPosition = new Vector3(_buildingCamera.transform.localPosition.x, distance, _buildingCamera.transform.localPosition.z);

    }

    
    void AdjustRotationKeyboard(float angle)
    {
        angle *= _rotationSpeedKeyboard * Time.deltaTime;
        _buildingCameraHolder.rotation = Quaternion.Euler(0f, _buildingCameraHolder.localEulerAngles.y + angle, 0f);
    }
}
