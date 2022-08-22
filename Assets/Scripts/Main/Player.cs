using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    #region Variables
    public bool IsGrounded;
    public bool IsSprinting = false;

    private Keyboard _keyboard;
    private Mouse _mouse;
    public float MouseSense;
    public float LookLimit;
    private Vector2 _mouseLookInput;
    private bool _isCursorLocked;
    private Vector2 _keyboardInput;

    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _cameraTransform;
    private World _world;

    [SerializeField] private float _walkSpeed = 1f;
    [SerializeField] private float _sprintSpeed = 2f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _gravity = -9.8f;

    [SerializeField] public float PlayerWidth = 0.6125f;
    private float _xRotation;
    private Vector3 _velocity = Vector3.zero;
    private float _verticalMomentum;
    private bool _jumpRequest;
    private VoxelWorldGenerator _inputActions;
    private PlayerInput _playerInput;
    #endregion
    private void Start()
    {
        _playerTransform = GetComponent<Transform>();
        Cursor.lockState = CursorLockMode.Locked;
        _isCursorLocked = true;
        _world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
        _mouse = Mouse.current;
        _keyboard = Keyboard.current;
        _cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        _inputActions = new VoxelWorldGenerator();
        _inputActions.Enable();
    }

    private void Update()
    {
        GetPlayerInputs();
    }
    private void FixedUpdate()
    {
        CalculateVelocity();
        Look();
        if (_jumpRequest)
        {
            Jump();
        }
    }
    private void CalculateVelocity()
    {
        if (_verticalMomentum > _gravity)
        {

            _verticalMomentum += Time.fixedDeltaTime * _gravity;

        }
        if (IsSprinting)
        {
            _velocity = ((transform.forward * _keyboardInput.y) + (transform.right * _keyboardInput.x)) * Time.fixedDeltaTime * _sprintSpeed;
        }
        else
        {
            _velocity = ((transform.forward * _keyboardInput.y) + (transform.right * _keyboardInput.x)) * Time.fixedDeltaTime * _walkSpeed;
        }
        if (!CheckInside())
            _velocity += Vector3.up * _verticalMomentum * Time.fixedDeltaTime;
        else
            _velocity += Vector3.up * (-_verticalMomentum) * Time.fixedDeltaTime;
        if ((_velocity.z < 0 && Back()) || _velocity.z > 0 && Front())
        {
            _velocity.z = 0;
        }
        if ((_velocity.x < 0 && Left()) || _velocity.x > 0 && Right())
        {
            _velocity.x = 0;
        }
        if (_velocity.y < 0)
        {
            _velocity.y = CheckDownSpeed(_velocity.y);
        }
        else if (_velocity.y > 0)
        {
            _velocity.y = CheckUpSpeed(_velocity.y);
        }

        transform.Translate(_velocity, Space.World);

    }
    private void Look()
    {
        float mouseX = _mouseLookInput.x * MouseSense * Time.deltaTime;
        float mouseY = _mouseLookInput.y * MouseSense * Time.deltaTime;

        transform.Rotate(Vector3.up, mouseX);

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -LookLimit, LookLimit);

        Vector3 targetRotation = transform.eulerAngles;
        targetRotation.x = _xRotation;
        _cameraTransform.eulerAngles = targetRotation;
    }
    private void Move(float currentSpeed)
    {
        GetPlayerInputs();
        Vector3 movement = ((transform.forward * _keyboardInput.y) + (transform.right * _keyboardInput.x)) * Time.deltaTime * currentSpeed;
        movement.y += 1 * _gravity * Time.deltaTime;
        movement.y = CheckDownSpeed(movement.y);
        _velocity = movement;
        transform.Translate(movement, Space.World);
    }

    private void Jump()
    {
        _verticalMomentum = _jumpForce;
        IsGrounded = false;
        _jumpRequest = false;
    }
    private void GetPlayerInputs()
    {
        _mouseLookInput = _inputActions.Player.Look.ReadValue<Vector2>();
        _keyboardInput = _inputActions.Player.Move.ReadValue<Vector2>();
        if (_keyboard.shiftKey.wasPressedThisFrame) IsSprinting = true;
        if (_keyboard.shiftKey.wasReleasedThisFrame) IsSprinting = false;

        if (IsGrounded && _keyboard.spaceKey.wasPressedThisFrame) _jumpRequest = true;
    }
    private float CheckDownSpeed(float downSpeed)
    {
        if (
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + downSpeed, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + downSpeed, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + downSpeed, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + downSpeed, _playerTransform.position.z + PlayerWidth)) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 1f)))

            )
        {
            IsGrounded = true;
            return 0;
        }
        else
        {
            IsGrounded = false;
            return downSpeed;
        }
    }
    private float CheckUpSpeed(float upSpeed)
    {
        if (
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 4f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 4f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 4f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 4f, _playerTransform.position.z + PlayerWidth)) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y + 4f, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y + 4f, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y + 4f, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y + 4f, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y + 4f, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y + 4f, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y + 4f, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y + 4f, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y + 4f, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y + 4f, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y + 4f, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y + 4f, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 4f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 4f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 4f, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 4f, _playerTransform.position.z + (PlayerWidth - 1f)))
            )
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }
    public bool Front()
    {
        if (
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 1f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 1.25f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 1.5f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 1.75f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 2f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 2.25f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 2.5f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 2.75f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 3f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 3.25f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 3.5f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 3.75f, _playerTransform.position.z + PlayerWidth))
            )
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool Back()
    {
        if (
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 1f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 1.25f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 1.5f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 1.75f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 2f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 2.25f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 2.5f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 2.75f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 3f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 3.25f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 3.5f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 3.75f, _playerTransform.position.z - PlayerWidth))
            )
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool Right()
    {
        if (
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 1f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 1.25f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 1.5f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 1.75f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 2f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 2.25f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 2.5f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 2.75f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 3f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 3.25f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 3.5f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 3.75f, _playerTransform.position.z))
            )
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool Left()
    {
        if (
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 1f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 1.25f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 1.5f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 1.75f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 2f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 2.25f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 2.5f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 2.75f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 3f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 3.25f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 3.5f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 3.75f, _playerTransform.position.z))
            )
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CheckInside()
    {
        if (
        #region Layers
            //Ground Layer
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            //First Layer       
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y + 0.25f, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y + 0.25f, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y + 0.25f, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y + 0.25f, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y + 0.25f, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y + 0.25f, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y + 0.25f, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y + 0.25f, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y + 0.25f, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y + 0.25f, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y + 0.25f, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y + 0.25f, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 0.25f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 0.25f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 0.25f, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 0.25f, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            //Second Layer 
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y + 0.5f, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y + 0.5f, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y + 0.5f, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y + 0.5f, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y + 0.5f, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y + 0.5f, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y + 0.5f, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y + 0.5f, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y + 0.5f, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y + 0.5f, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y + 0.5f, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y + 0.5f, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 0.5f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 0.5f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 0.5f, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 0.5f, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            //Third Layer      
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y + 0.75f, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y + 0.75f, _playerTransform.position.z - (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.25f), _playerTransform.position.y + 0.75f, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.25f), _playerTransform.position.y + 0.75f, _playerTransform.position.z + (PlayerWidth - 0.25f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y + 0.75f, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y + 0.75f, _playerTransform.position.z - (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.5f), _playerTransform.position.y + 0.75f, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.5f), _playerTransform.position.y + 0.75f, _playerTransform.position.z + (PlayerWidth - 0.5f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y + 0.75f, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y + 0.75f, _playerTransform.position.z - (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 0.75f), _playerTransform.position.y + 0.75f, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 0.75f), _playerTransform.position.y + 0.75f, _playerTransform.position.z + (PlayerWidth - 0.75f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 0.75f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 0.75f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 0.75f, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 0.75f, _playerTransform.position.z + (PlayerWidth - 1f)))
        #endregion
            )
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void OnEnable()
    {
        if (this.enabled)
        {

        }
    }


}
