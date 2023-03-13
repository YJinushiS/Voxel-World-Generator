using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;

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
    private GameObject _uiDebugScreen;
    private World _world;
    public Transform HighlightBlock;
    public Transform PlaceBlock;
    public float CheckIncrement = 0.01f;
    public float Reach = 64f;

    [SerializeField] private float _walkSpeed = 1f;
    [SerializeField] private float _sprintSpeed = 2f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _gravity = -9.8f;

    public float PlayerWidth = 2.4f;
    public int VoxelRadius = 5;
    private float _xRotation;
    private Vector3 _velocity = Vector3.zero;
    private float _verticalMomentum;
    private bool _jumpRequest;
    private VoxelWorldGenerator _inputActions;
    private PlayerInput _playerInput;

    public byte SelectedBlockIndex = 1;
    #endregion
    private void Awake()
    {
        _playerTransform = GetComponent<Transform>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _isCursorLocked = true;
        _world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
        _mouse = Mouse.current;
        _keyboard = Keyboard.current;
        _cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        _inputActions = new VoxelWorldGenerator();
        _inputActions.Enable();
        _uiDebugScreen = GameObject.FindGameObjectWithTag("DebugScreen");
        _uiDebugScreen.SetActive(false);

    }

    private void Update()
    {
        GetPlayerInputs();
        PlaceCursorBlock();
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
        if (_keyboard.shiftKey.wasPressedThisFrame)
        {
            IsSprinting = true;
        }
        if (_keyboard.shiftKey.wasReleasedThisFrame)
        { IsSprinting = false; }
        if (_keyboard.escapeKey.wasPressedThisFrame)
        {
            if (_isCursorLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;
                _isCursorLocked = false;
            }
        }
        if (_mouse.leftButton.wasPressedThisFrame)
        {
            if (!_isCursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
                _isCursorLocked = true;
            }
        }
        if (_keyboard.f3Key.wasPressedThisFrame)
        {
            _uiDebugScreen.SetActive(!_uiDebugScreen.activeSelf);
        }

        if (IsGrounded && _keyboard.spaceKey.wasPressedThisFrame) _jumpRequest = true;

        if (HighlightBlock.gameObject.activeSelf)
        {
            //Destroy Block
            if (_mouse.leftButton.wasPressedThisFrame)
            {

                _world.GetChunkFromVector3(HighlightBlock.position).EditVoxelsInSphere(HighlightBlock.position, VoxelRadius, 0);
            }
            if (_mouse.rightButton.wasPressedThisFrame)
            {
                _world.GetChunkFromVector3(PlaceBlock.position).EditVoxelsInSphere(PlaceBlock.position,VoxelRadius, SelectedBlockIndex);
            }
        }
    }
    private void PlaceCursorBlock()
    {
        float step = CheckIncrement;
        Vector3 lastPos = new Vector3();

        while (step < Reach)
        {
            Vector3 pos = _cameraTransform.position + (_cameraTransform.forward * step);
            if (_world.CheckForVoxel(pos))
            {
                HighlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                PlaceBlock.position = lastPos;

                HighlightBlock.gameObject.SetActive(true);
                PlaceBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += CheckIncrement;
        }

        HighlightBlock.gameObject.SetActive(false);
        PlaceBlock.gameObject.SetActive(false);
    }
    private void EditVoxelsInSphere(Vector3 pos, int radius, byte newID)
    {
        for(int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
                for (int z = -radius; x <= radius; z++)
                {
                    Vector3 newPos = new Vector3(Mathf.FloorToInt(pos.x + x), Mathf.FloorToInt(pos.y + y), Mathf.FloorToInt(pos.z + z));
                    if(Vector3.Distance(pos, newPos) <= (float)radius)
                    {
                        _world.GetChunkFromVector3(newPos).EditVoxel(newPos, 0);
                    }
                }
    }
    private float CheckDownSpeed(float downSpeed)
    {
        if (
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + downSpeed, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + downSpeed, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + downSpeed, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + downSpeed, _playerTransform.position.z + PlayerWidth)) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 1f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y + downSpeed, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y + downSpeed, _playerTransform.position.z + (PlayerWidth - 2f)))

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
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 16f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 16f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 16f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 16f, _playerTransform.position.z + PlayerWidth)) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 16f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 16f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 16f, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 16f, _playerTransform.position.z + (PlayerWidth - 1f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y + 16f, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y + 16f, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y + 16f, _playerTransform.position.z + (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y + 16f, _playerTransform.position.z + (PlayerWidth - 2f)))
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
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 4f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 5f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 6f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 7f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 8f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 9f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 10f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 11f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 12f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 13f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 14f, _playerTransform.position.z + PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 15f, _playerTransform.position.z + PlayerWidth))
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
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 4f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 5f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 6f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 7f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 8f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 9f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 10f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 11f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 12f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 13f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 14f, _playerTransform.position.z - PlayerWidth)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x, _playerTransform.position.y + 15f, _playerTransform.position.z - PlayerWidth))
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
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 4f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 5f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 6f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 7f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 8f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 9f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 10f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 11f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 12f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 13f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 14f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + PlayerWidth, _playerTransform.position.y + 15f, _playerTransform.position.z))
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
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 4f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 5f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 6f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 7f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 8f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 9f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 10f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 11f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 12f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 13f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 14f, _playerTransform.position.z)) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - PlayerWidth, _playerTransform.position.y + 15f, _playerTransform.position.z))
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
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth -1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth -1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth -1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth -1f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y, _playerTransform.position.z + (PlayerWidth - 2f))) ||
            //First Layer       
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 1f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 1f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 1f, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 1f, _playerTransform.position.z + (PlayerWidth - 1f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y + 1f, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y + 1f, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y + 1f, _playerTransform.position.z + (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y + 1f, _playerTransform.position.z + (PlayerWidth - 2f))) ||
            //Second Layer 
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 2f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 2f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 2f, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 2f, _playerTransform.position.z + (PlayerWidth - 1f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y + 2f, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y + 2f, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y + 2f, _playerTransform.position.z + (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y + 2f, _playerTransform.position.z + (PlayerWidth - 2f))) ||
            //Third Layer      
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 3f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 3f, _playerTransform.position.z - (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 1f), _playerTransform.position.y + 3f, _playerTransform.position.z + (PlayerWidth - 1f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 1f), _playerTransform.position.y + 3f, _playerTransform.position.z + (PlayerWidth - 1f))) ||

            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y + 3f, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y + 3f, _playerTransform.position.z - (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x + (PlayerWidth - 2f), _playerTransform.position.y + 3f, _playerTransform.position.z + (PlayerWidth - 2f))) ||
            _world.CheckForVoxel(new Vector3(_playerTransform.position.x - (PlayerWidth - 2f), _playerTransform.position.y + 3f, _playerTransform.position.z + (PlayerWidth - 2f))) 
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
