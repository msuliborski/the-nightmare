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
    private Camera _camera;

    public GameObject CurrentObject
    {
        get => _currentObject;
        set => _currentObject = value;
    }

    private void Start()
    {
        _reverseGrid = 1f / _grid;
        _camera = gameObject.transform.Find("BuildingCamera").GetComponent<Camera>();
    }
    
    private void Update()
    {
        if (GameManager.CurrentState == GameManager.GameState.Building)
        {
            HandleKey();
            if (_currentObject != null)
            {
                MoveToMouse();
                ReleaseOnClick();
                RotateObject();
            }
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
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

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
        if (Input.GetKeyDown(KeyCode.R))
            GameManager.Instance.CmdRegisterBeingReady();
    }
}
