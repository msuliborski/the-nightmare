using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class PlacementController : NetworkBehaviour
{
    public static float GridTileSize = 1f;
    [SerializeField] private List<GameObject> _placeableObject;
    [SerializeField] private List<GameObject> _placeableObjectModels;
    //public GameObject PlaceableObject {get { return _placeableObject; } set { _placeableObject = value; }}
    private float _mouseWheelRotation;
    private float _x = 0, _y = 0, _reverseGrid, _camMinZoom, _camMaxZoom;
    private GameObject _currentObject;
    private string _currentTag;
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
    private int _placeableIndex = 0;
    private bool _isPlacing = false;
    public int[] maxPlaceable = {3, 1, 3};
    public int[] placeableCount = new int[3];
    private TextMeshProUGUI[] _placeableTM = new TextMeshProUGUI[3];
    public const int INDEX_OF_SNARES = 0;
    public const int INDEX_OF_BEAR = 1;
    public const int INDEX_OF_BARREL = 2;
    private CustomNetworkManager _customNetworkManager;

    private PlayerShoot _playerShoot;
    private PlayerManager _playerManager;

    private AudioSource _source;
    [SerializeField] private AudioClip bop;
    [SerializeField] private AudioClip nono;

    public GameObject CurrentObject
    {
        get => _currentObject;
        set => _currentObject = value;
    }

    private void Start()
    {
        _source = GetComponent<AudioSource>();
        if (isLocalPlayer)
        {
            //_customNetworkManager = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>();
            _placeableTM[0] = GameObject.Find("SnaresCount").GetComponent<TextMeshProUGUI>();
            _placeableTM[1] = GameObject.Find("TeddyBearCount").GetComponent<TextMeshProUGUI>();
            _placeableTM[2] = GameObject.Find("BarrelsCount").GetComponent<TextMeshProUGUI>();
        }

        _playerShoot = GetComponent<PlayerShoot>();
        _playerManager = GetComponent<PlayerManager>();
        _reverseGrid = 1f / GridTileSize;
        _buildingCameraHolder = gameObject.transform.Find("BuildingCameraHolder");
        _buildingCamera = _buildingCameraHolder.GetComponentInChildren<Camera>();
        _camMaxZoom = _buildingCamera.transform.position.y;
        _camMinZoom = 1f;
        _currentCamera = _actionCamera = gameObject.transform.Find("PlayerCamera").GetComponent<Camera>();
        
    }

    private void Update()
    {
        if(isLocalPlayer)
            for (int i = 0; i < 3; i++)
            {
                _placeableTM[i].text = placeableCount[i].ToString();
            }
            
        HandleKeys();
        if (_currentObject != null)
        {
            MoveToMouse();
            ReleaseOnClick();
            RotateWheel();
        }
        
    }

    private void PlaySound(AudioClip clip)
    {
        _source.clip = clip;
        _source.PlayOneShot(_source.clip);
    }

    void RotateWheel()
    {
        _mouseWheelRotation = 0;
        _mouseWheelRotation += Input.mouseScrollDelta.y;
        
        if (_mouseWheelRotation > 0.5)
        {
            _placeableIndex++;
            if (_placeableIndex > _placeableObject.Count - 1) _placeableIndex = 0;
            UpdatePlaceable();
            //_currentObject.transform.Rotate(Vector3.up, 90);
        }
        else if (_mouseWheelRotation < -0.5)
        {
            _placeableIndex--;
            if (_placeableIndex < 0) _placeableIndex = _placeableObject.Count - 1;
            UpdatePlaceable();
            //_currentObject.transform.Rotate(Vector3.up, -90);
        }
    }
   
    void UpdatePlaceable()
    {
        Destroy(_currentObject);
        _currentObject = Instantiate(_placeableObjectModels[_placeableIndex]);
    }

    void ReleaseOnClick()
    {
        if (Input.GetMouseButton(0))
        {
            string posAndTag = _currentObject.transform.position.x.ToString() + "_" + _currentObject.transform.position.z.ToString() + "_" + _currentTag;
            if(placeableCount[_placeableIndex] > 0)
            {
                if (GameManager.Instance.BuildingPoints.ContainsKey(posAndTag)
                    && GameManager.Instance.BuildingPoints[posAndTag].Buildable
                    )
                {
                    Debug.Log("clicked");
                    PlaySound(bop);
                    if (GameManager.CurrentState == GameManager.GameState.Fighting)
                        GameManager.TurnOnGridRenders(false);

                    Destroy(_currentObject);
                    CmdPlaceEntity(_currentObject.transform.position, _currentObject.transform.rotation, _currentTag, _placeableIndex);
                    GameManager.Instance.BuildingPoints[posAndTag].Buildable = false;
                    _playerShoot.WasBuilt = true;
                    placeableCount[_placeableIndex]--;
                }
                else
                    PlaySound(nono);
            }
            else 
                PlaySound(nono);
        }
    }

    [Command]
    void CmdPlaceEntity(Vector3 pos, Quaternion rot, string tag, int placeableIndex)
    {
        string posAndTag = pos.x.ToString() + "_" + pos.z.ToString() + "_" + tag;
        GameObject placeableObject = Instantiate(_placeableObject[placeableIndex], pos, rot);
        switch (placeableIndex)
        {
            case INDEX_OF_SNARES:
                Snares snares = placeableObject.GetComponent<Snares>();
                snares.InitialPosAndTag = posAndTag;
                break;
            case INDEX_OF_BEAR:
                TeddyBearServer teddyBearServer = placeableObject.GetComponent<TeddyBearServer>();
                teddyBearServer.InitialPosAndTag = posAndTag;
                teddyBearServer.GetComponentInChildren<SpriteRenderer>().enabled = false;
                break;
            case INDEX_OF_BARREL:
                Barrel barrel = placeableObject.GetComponentInChildren<Barrel>();
                barrel.InitialPosAndTag = posAndTag;
                break;
        }
        NetworkServer.Spawn(placeableObject);
        RpcPlaceEntity(posAndTag);
    }

    [ClientRpc]
    void RpcPlaceEntity(string posAndTag)
    {
        if (!isLocalPlayer)
        {
            GameManager.Instance.BuildingPoints[posAndTag].Buildable = false;
        }
    }
    
    
    void MoveToMouse()
    {
        if (_currentObject != null)
        {
            Ray ray = _currentCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 100f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
            {
                _x = Mathf.Round(hitInfo.point.x * _reverseGrid) / _reverseGrid;
                _y = Mathf.Round(hitInfo.point.z * _reverseGrid) / _reverseGrid;
                _currentObject.transform.position = new Vector3(_x, hitInfo.transform.position.y, _y);
                _currentTag = hitInfo.collider.tag;
                //_currentObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
        }
    }

    void HandleKeys()
    {
        switch (GameManager.CurrentState)
        {
            case GameManager.GameState.Fighting:

                if (Input.GetKeyDown(KeyCode.T))
                {
                    if (_currentObject == null)
                    {
                        GameManager.TurnOnGridRenders(true);
                        _currentObject = Instantiate(_placeableObjectModels[_placeableIndex]);
                        _playerShoot.IsBuildingOnFly = true;
                    }
                    else
                    {
                        GameManager.TurnOnGridRenders(false);
                        Destroy(_currentObject);
                        _playerShoot.IsBuildingOnFly = false;
                    }

                }
                if (Input.GetKeyDown(KeyCode.Return) && GameManager.IsListeningForReady)
                {
                    GameManager.IsListeningForReady = false;
                    CmdRegisterBeingReady();

                }
                break;

            case GameManager.GameState.Building:
            
                float xDelta = 0f, zDelta = 0f, rotationDelta = 0f;

                if (!_isPlacing)
                {
                    float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
                    if (zoomDelta != 0f) AdjustZoom(zoomDelta);                    
                }
                


                if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Comma)) rotationDelta++;
                if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Period)) rotationDelta--;

                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - _scrollBorderThickness) zDelta++;
                if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Input.mousePosition.y <= _scrollBorderThickness) zDelta--;
                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - _scrollBorderThickness) xDelta++;
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.mousePosition.x <= _scrollBorderThickness) xDelta--;

                if (xDelta != 0f || zDelta != 0f) AdjustPositionMouse(xDelta, zDelta);
                if (rotationDelta != 0f) AdjustRotationKeyboard(rotationDelta);

                if (Input.GetKeyDown(KeyCode.T))
                {
                    if (_currentObject == null)
                    {
                        _currentObject = Instantiate(_placeableObject[_placeableIndex]);
                        _isPlacing = true;
                    }
                    else
                    {
                        Destroy(_currentObject);
                        _isPlacing = false;
                    }

                }

                
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
        Debug.Log("aaaa: " + GameManager.LocalPlayer.transform.name);
        /*PlacementController localPlayer = GameManager.LocalPlayer.GetComponent<PlacementController>(); // dlatego musimy wyluskac local playera
        localPlayer._currentCamera = localPlayer._actionCamera;
        ClockManager.canCount = true;
        if (localPlayer._currentObject != null) Destroy(localPlayer._currentObject);
        GameManager.CurrentState = GameManager.GameState.Fighting;*/
        GameManager.Instance.Teleport();
        
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
