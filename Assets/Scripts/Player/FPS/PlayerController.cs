using System;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : NetworkBehaviour {
    [SerializeField] public float _speed = 2f;
    [SerializeField] private float _lookSensitivity = 2.5f;
    [SerializeField] private Joystick move;
    [SerializeField] private Joystick look;
    public float SensitivityScale { get; set; }
    [SerializeField] private float _nonZoomSensivity = 0.7f;

    public float NonZoomSensitivity {
        get { return _nonZoomSensivity; }
        set { _nonZoomSensivity = value; }
    }

    [SerializeField] private float _zoomSensivity = 0.1f;

    public float ZoomSensitivity {
        get { return _zoomSensivity; }
        set { _zoomSensivity = value; }
    }

    private float _speedSlow = 2f;
    private float _speedFast = 7f;

    private PlayerMotor _motor;
    private static readonly int IsSprinting = Animator.StringToHash("isSprinting");

    void Start() {
        _motor = GetComponent<PlayerMotor>();
        SensitivityScale = 0.7f;
#if UNITY_ANDROID
        move = GameObject.Find("Move").GetComponent<Joystick>();
        look = GameObject.Find("Look").GetComponent<Joystick>();
#endif
    }


    private void Update() {
        if (transform.GetComponent<PlayerEquipment>().getActiveWeapon().GetComponent<Animator>().GetBool(IsSprinting))
            _speed = _speedFast;
        else
            _speed = _speedSlow;


//        Debug.Log(_speed);
        if (!PauseGame.menuActive) {
            float xMov = 0;
            float zMov = 0;

#if UNITY_ANDROID
            if(Mathf.Abs(move.Horizontal) >= 0.2)
                xMov = move.Horizontal;
            if(Mathf.Abs(move.Vertical) >= 0.2)
                zMov = move.Vertical;
#endif

#if UNITY_STANDALONE
            xMov = Input.GetAxisRaw("Horizontal");
            zMov = Input.GetAxisRaw("Vertical");
#endif

            Vector3 moveHorizontal = transform.right * xMov;
            Vector3 moveVertical = transform.forward * zMov;

            Vector3 velocity = (moveHorizontal + moveVertical).normalized * _speed;

            _motor.Move(velocity);

            float yRot = 0;
            float xRot = 0;

#if UNITY_ANDROID
            if(Mathf.Abs(look.Horizontal) >= 0.2)
                yRot = look.Horizontal;
            if(Mathf.Abs(look.Horizontal) >= 0.2)
                xRot = look.Vertical;
#endif

#if UNITY_STANDALONE
            yRot = Input.GetAxisRaw("Mouse X") * SensitivityScale;
            xRot = Input.GetAxis("Mouse Y") * SensitivityScale;
#endif

            Vector3 rotation = new Vector3(0f, yRot, 0f) * _lookSensitivity;
            _motor.Rotate(rotation);

            float cameraRotationX = xRot * _lookSensitivity;
            _motor.RotateCamera(cameraRotationX);


            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        else {
            _motor.Move(Vector3.zero);
            _motor.Rotate(Vector3.zero);
            _motor.RotateCamera(0f);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.CompareTag("SlowDownWall")) {
            _speedSlow = 1f;
            _speedFast = 1f;
        }
    }    
    
    void OnCollisionExit(Collision col) {
        if (col.gameObject.CompareTag("SlowDownWall")) {
            _speedSlow = 2f;
            _speedFast = 7f;
        }
    }
}