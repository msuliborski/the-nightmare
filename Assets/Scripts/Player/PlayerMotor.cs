using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : NetworkBehaviour
{

    [SerializeField] private Camera _cam;

    private Vector3 _velocity = Vector3.zero;
    private Vector3 _rotation = Vector3.zero;
    private Vector3 _cameraRotation = Vector3.zero;
    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 velocity)
    {
        _velocity = velocity;
    }

    void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }

    private void PerformMovement()
    {
        if (_velocity != Vector3.zero)
            _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
    }

    public void Rotate(Vector3 rotation)
    {
        _rotation = rotation;
    }

    void PerformRotation()
    {
        _rb.MoveRotation(_rb.rotation * Quaternion.Euler(_rotation));
        if (_cam != null) _cam.transform.Rotate(-_cameraRotation);
    }

    public void RotateCamera(Vector3 cameraRotation)
    {
        _cameraRotation = cameraRotation;
    }
}
