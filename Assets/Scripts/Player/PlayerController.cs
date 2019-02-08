using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _lookSensitivity = 3f;
    
    private PlayerMotor _motor;
    
    

    void Start()
    {
        
        _motor = GetComponent<PlayerMotor>();
        Cursor.lockState = CursorLockMode.Locked;
    }


    private void Update()
    {
        float xMov = Input.GetAxisRaw("Horizontal");
        float zMov = Input.GetAxisRaw("Vertical");

        Vector3 moveHorizontal = transform.right * xMov;
        Vector3 moveVertical = transform.forward * zMov;

        Vector3 velocity = (moveHorizontal + moveVertical).normalized * _speed;

        _motor.Move(velocity);

        float yRot = Input.GetAxisRaw("Mouse X");

        Vector3 rotation = new Vector3(0f, yRot, 0f) * _lookSensitivity;

        _motor.Rotate(rotation);

        float xRot = Input.GetAxis("Mouse Y");

        float cameraRotationX = xRot * _lookSensitivity;

        _motor.RotateCamera(cameraRotationX);

        //float yRot = Input.GetAxisRaw("Mouse Y");
    }

}
