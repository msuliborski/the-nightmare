using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : NetworkBehaviour {
    [SerializeField] private Camera _cam;
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _rotation = Vector3.zero;
    private float _cameraRotationX = 0f;
    private float currentCameraRotationX = 0f;
    private Rigidbody _rb;
    private Animator _anim;
    [SerializeField] private float _cameraRotationLimit = 85f;
    private float _recoilOffset = 0f;

    void Start() {
        _anim = transform.GetChild(0).GetChild(0).GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 velocity) {
        _velocity = velocity;
    }

    void FixedUpdate() {
        PerformMovement();
        PerformRotation();
        DecreaseRecoil();
    }

    private void PerformMovement() {
        if (_velocity != Vector3.zero) {
            _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
            _anim.SetBool("running", true);
        }
        else _anim.SetBool("running", false);
    }

    public void Rotate(Vector3 rotation) {
        _rotation = rotation;
    }

    void PerformRotation() {
        _rb.MoveRotation(_rb.rotation * Quaternion.Euler(_rotation));
        if (_cam != null) {
            currentCameraRotationX -= _cameraRotationX;
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -_cameraRotationLimit, _cameraRotationLimit);

            _cam.transform.localEulerAngles = new Vector3(currentCameraRotationX - _recoilOffset, 0f, 0f);
        }
    }

    public void RotateCamera(float cameraRotation) {
        _cameraRotationX = cameraRotation;
    }

    public void IncreaseRecoil(float weaponRecoil) {
        _recoilOffset += weaponRecoil - _recoilOffset * 0.05f;
    }

    private void DecreaseRecoil() {
        _recoilOffset -= _recoilOffset * 0.03f + 0.01f;
        if (_recoilOffset < 0) _recoilOffset = 0;
    }
}
