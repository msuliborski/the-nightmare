using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _lookSensitivity = 3f;
    [SerializeField] private Joystick move;
    [SerializeField] private Joystick look;
    
    private PlayerMotor _motor;

    
    
    

    void Start()
    {
        _motor = GetComponent<PlayerMotor>();
        move = GameObject.Find("Move").GetComponent<Joystick>();
        look = GameObject.Find("Look").GetComponent<Joystick>();
    }


    private void Update()
    {
        if (!PauseGame.menuActive)
        {

            #if UNITY_ANDROID
            float xMov = move.Horizontal;
            float zMov = move.Vertical;
            #endif
            
            #if UNITY_STANDALONE
            float xMov = Input.GetAxisRaw("Horizontal");
            float zMov = Input.GetAxisRaw("Vertical");
            #endif

            Vector3 moveHorizontal = transform.right * xMov;
            Vector3 moveVertical = transform.forward * zMov;

            Vector3 velocity = (moveHorizontal + moveVertical).normalized * _speed;

            _motor.Move(velocity);


            #if UNITY_ANDROID
            float yRot = look.Horizontal;
            float xRot = look.Vertical;
            #endif
            
            #if UNITY_STANDALONE
            float yRot = Input.GetAxisRaw("Mouse X");
            float xRot = Input.GetAxis("Mouse Y");
            #endif

            Vector3 rotation = new Vector3(0f, yRot, 0f) * _lookSensitivity;

            _motor.Rotate(rotation);


            


            float cameraRotationX = xRot * _lookSensitivity;

            _motor.RotateCamera(cameraRotationX);

            
            
            Cursor.lockState = CursorLockMode.Locked;
        }

        else
        {
            _motor.Move(Vector3.zero);
            _motor.Rotate(Vector3.zero);
            _motor.RotateCamera(0f);
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
