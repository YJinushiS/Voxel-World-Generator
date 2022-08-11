//===========================================================================//
//                       FreeFlyCamera (Version 1.2)                         //
//                        (c) 2019 Sergey Stafeyev                           //
//===========================================================================//

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class FreeFlyCamera : MonoBehaviour
{
    #region UI

    [Space]

    [SerializeField]
    [Tooltip("The script is currently active")]
    private bool _active = true;

    [Space]

    [SerializeField]
    [Tooltip("Camera rotation by mouse movement is active")]
    private bool _enableRotation = true;

    [SerializeField]
    [Tooltip("Sensitivity of mouse rotation")]
    private float _mouseSense = 1.8f;

    [Space]

    [SerializeField]
    [Tooltip("Camera zooming in/out by 'Mouse Scroll Wheel' is active")]
    private bool _enableTranslation = true;

    [SerializeField]
    [Tooltip("Velocity of camera zooming in/out")]
    private float _translationSpeed = 55f;

    [Space]

    [SerializeField]
    [Tooltip("Camera movement by 'W','A','S','D','Q','E' keys is active")]
    private bool _enableMovement = true;

    [SerializeField]
    [Tooltip("Camera movement speed")]
    private float _movementSpeed = 10f;

    [SerializeField]
    [Tooltip("Speed of the quick camera movement when holding the 'Left Shift' key")]
    private float _boostedSpeed = 50f;

    [SerializeField]
    [Tooltip("Boost speed")]
    private KeyCode _boostSpeed = KeyCode.LeftShift;

    [SerializeField]
    [Tooltip("Move up")]
    private KeyCode _moveUp = KeyCode.E;

    [SerializeField]
    [Tooltip("Move down")]
    private KeyCode _moveDown = KeyCode.Q;

    [Space]

    [SerializeField]
    [Tooltip("Acceleration at camera movement is active")]
    private bool _enableSpeedAcceleration = true;

    [SerializeField]
    [Tooltip("Rate which is applied during camera movement")]
    private float _speedAccelerationFactor = 1.5f;

    [Space]

    [SerializeField]
    [Tooltip("This keypress will move the camera to initialization position")]
    private KeyCode _initPositonButton = KeyCode.R;

    #endregion UI
    private Keyboard _keyboard;
    private Mouse _mouse;
    private Vector2 _mouseLook;
    [SerializeField]private Transform _player;
    private float _xRotation;
    private VoxelWorldGenerator _inputActions;
    private PlayerInput _playerInput;
    private CursorLockMode _wantedMode;

    private float _currentIncrease = 1;
    private float _currentIncreaseMem = 0;

    private Vector3 _initPosition;
    private Vector3 _initRotation;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_boostedSpeed < _movementSpeed)
            _boostedSpeed = _movementSpeed;
    }
#endif


    private void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _inputActions = new VoxelWorldGenerator();
        _inputActions.Enable();
        _mouse = Mouse.current;
        _keyboard = Keyboard.current;
        _initPosition = transform.position;
        _initRotation = transform.eulerAngles;
    }

    private void OnEnable()
    {
        if (_active)
            _wantedMode = CursorLockMode.Locked;
    }

    // Apply requested cursor state
    private void SetCursorState()
    {
        if (_keyboard.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = _wantedMode = CursorLockMode.None;
        }

        if (_mouse.leftButton.wasPressedThisFrame)
        {
            _wantedMode = CursorLockMode.Locked;
        }

        // Apply cursor state
        Cursor.lockState = _wantedMode;
        // Hide cursor when locking
        Cursor.visible = (CursorLockMode.Locked != _wantedMode);
    }

    private void CalculateCurrentIncrease(bool moving)
    {
        _currentIncrease = Time.deltaTime;

        if (!_enableSpeedAcceleration || _enableSpeedAcceleration && !moving)
        {
            _currentIncreaseMem = 0;
            return;
        }

        _currentIncreaseMem += Time.deltaTime * (_speedAccelerationFactor - 1);
        _currentIncrease = Time.deltaTime + Mathf.Pow(_currentIncreaseMem, 3) * Time.deltaTime;
    }

    private void Update()
    {
        if (!_active)
            return;

        SetCursorState();

        if (Cursor.visible)
            return;

        // Translation
        if (_enableTranslation)
        {
            transform.Translate(Vector3.forward * _mouse.scroll.ReadValue() * Time.deltaTime * _translationSpeed);
        }

        // Movement
        if (_enableMovement)
        {
            Vector3 deltaPosition = Vector3.zero;
            float currentSpeed = _movementSpeed;

            if (_keyboard.shiftKey.isPressed)
                currentSpeed = _boostedSpeed;

            if (_keyboard.wKey.isPressed)
                deltaPosition += transform.forward;

            if (_keyboard.sKey.isPressed)
                deltaPosition -= transform.forward;

            if (_keyboard.aKey.isPressed)
                deltaPosition -= transform.right;

            if (_keyboard.dKey.isPressed)
                deltaPosition += transform.right;

            if (_keyboard.eKey.isPressed)
                deltaPosition += transform.up;

            if (_keyboard.qKey.isPressed)
                deltaPosition -= transform.up;

            // Calc acceleration
            CalculateCurrentIncrease(deltaPosition != Vector3.zero);

            _player.transform.position += deltaPosition * currentSpeed * _currentIncrease;
        }

        // Rotation
        if (_enableRotation)
        {
            _mouseLook = _inputActions.Player.Look.ReadValue<Vector2>();

            float mouseX = _mouseLook.x * _mouseSense * Time.deltaTime;
            float mouseY = _mouseLook.y * _mouseSense * Time.deltaTime;

            _player.Rotate(Vector3.up, mouseX);

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90);

            Vector3 targetRotation = transform.eulerAngles;
            targetRotation.x = _xRotation;
            transform.eulerAngles = targetRotation;
            //transform.Rotate(Vector3.up * mouseX);
        }

        // Return to init position
    }
}
