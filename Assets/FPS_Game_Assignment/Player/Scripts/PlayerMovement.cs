using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    #region Config & Dependencies
    [Header("Config (data-only)")]
    [SerializeField] private MovementConfig movementConfig;

    [Header("Dependencies")]
    [Tooltip("Component implementing IInputProvider. Can be KeyboardMouseInput or MobileInputProvider")]
    [SerializeField] private MonoBehaviour inputProviderComponent;
    #endregion

    #region Crouch / Camera
    [Header("Crouch/Camera")]
    [Tooltip("Camera will be moved down/up with crouch")]
    [SerializeField] private Transform playerCamera;

    [SerializeField, Tooltip("Capsule height when not crouched. If zero, will use CharacterController.height as default")]
    private float standingHeight = 0f;

    [SerializeField, Tooltip("CharacterController radius override (0 to use existing)")]
    private float controllerRadius = 0f;
    #endregion

    #region Settings
    [Header("Settings")]
    [SerializeField, Tooltip("Height from player origin to sphere used to check grounding")]
    private float groundCheckDistance = 0.3f;
    #endregion

    #region Internal state
    private IInputProvider _inputProvider;
    private CharacterController _controller;
    private Transform _transform;

    // movement state
    private Vector3 _horizontalVelocity;
    private bool _isGrounded;
    private float _verticalSpeed;
    private bool _wasGroundedLastFrame; // previous frame grounding for jump buffering

    // crouch smoothing state
    private float _targetHeight;
    private float _currentHeight;
    private Vector3 _targetCenter;
    private Vector3 _currentCenter;
    private Vector3 _cameraLocalPosStanding;
    private Vector3 _cameraLocalPosCrouched;

    // immutable baseline
    private Vector3 _originalControllerCenter;

    // thresholds / debug tracking
    private const float HeightApplyEpsilon = 0.01f;
    private const float CenterApplyEpsilon = 0.01f;
    private float _lastAppliedHeight;
    private Vector3 _lastAppliedCenter;
    #endregion

    #region Unity lifecycle
    private void Awake()
    {
        InitializeComponents();
        InitializeHeightsAndCamera();
        ApplyInitialControllerBaseline();
    }

    private void Update()
    {
        // Read input & preserve grounding for buffering logic
        var input = ProcessInput();
        _wasGroundedLastFrame = _isGrounded;

        UpdateGround();

        UpdateHorizontalMovement(input);

        TryJump(input);

        ApplyGravity();

        UpdateCrouch(input);

        SetCameraPos(input);

        PerformMove();
    }
    #endregion

    #region Initialization helpers
    private void InitializeComponents()
    {
        _controller = GetComponent<CharacterController>();
        _transform = transform;

        if (GetComponent<CapsuleCollider>() != null)
        {
            Debug.LogWarning("[PlayerMovement] Found a CapsuleCollider on the same GameObject as a CharacterController. Remove CapsuleCollider when using CharacterController.", this);
        }

        if (movementConfig == null) Debug.LogError($"[{nameof(PlayerMovement)}] MovementConfig is not assigned.", this);
        if (inputProviderComponent == null) Debug.LogError($"[{nameof(PlayerMovement)}] InputProviderComponent is not assigned.", this);

        _inputProvider = inputProviderComponent as IInputProvider;
        if (_inputProvider == null) Debug.LogError($"[{nameof(PlayerMovement)}] Input provider does not implement IInputProvider.", this);

        if (controllerRadius > 0f) _controller.radius = controllerRadius;
    }

    private void InitializeHeightsAndCamera()
    {
        if (standingHeight <= 0f) standingHeight = _controller.height;

        // init heights
        _currentHeight = standingHeight;
        _targetHeight = standingHeight;

        // preserve original center baseline (immutable)
        _originalControllerCenter = _controller.center;
        Vector3 forcedCenter = new Vector3(_originalControllerCenter.x, standingHeight / 2f, _originalControllerCenter.z);
        _currentCenter = forcedCenter;
        _targetCenter = forcedCenter;

        if (playerCamera != null && movementConfig != null)
        {
            _cameraLocalPosStanding = playerCamera.localPosition;
            _cameraLocalPosCrouched = new Vector3(_cameraLocalPosStanding.x,
                                                  _cameraLocalPosStanding.y * movementConfig.crouchHeightMultiplier,
                                                  _cameraLocalPosStanding.z);
        }

        _lastAppliedHeight = _controller.height;
        _lastAppliedCenter = _controller.center;
    }

    private void ApplyInitialControllerBaseline()
    {
        // Apply baseline immediately to avoid drift from other code
        _controller.height = standingHeight;
        _controller.center = _currentCenter;
    }
    #endregion

    #region Input
    private PlayerInputState ProcessInput()
    {
        return _inputProvider.GetInput();
    }
    #endregion

    #region Movement: horizontal
    private void UpdateHorizontalMovement(PlayerInputState input)
    {
        Vector3 desiredLocal = new Vector3(input.Move.x, 0f, input.Move.y);
        desiredLocal = Vector3.ClampMagnitude(desiredLocal, 1f);

        float baseSpeed = movementConfig != null ? movementConfig.walkSpeed : 3.5f;
        float speedMultiplier = 1f;
        if (input.Crouch) speedMultiplier *= movementConfig != null ? movementConfig.crouchSpeedMultiplier : 1f;

        float targetSpeed = baseSpeed * speedMultiplier;
        Vector3 desiredWorld = _transform.TransformDirection(desiredLocal) * targetSpeed;

        float acceleration = _isGrounded ? movementConfig.groundAcceleration : movementConfig.airAcceleration;
        _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, new Vector3(desiredWorld.x, 0, desiredWorld.z), acceleration * Time.deltaTime);

        if (!_isGrounded)
        {
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, new Vector3(desiredWorld.x, 0, desiredWorld.z), movementConfig.airControl * Time.deltaTime);
        }
    }
    #endregion

    #region Jump & gravity
    private void TryJump(PlayerInputState input)
    {
        // Allow jump if grounded OR if we were grounded last frame (simple buffer)
        bool canJump = (_isGrounded || _wasGroundedLastFrame) && !input.Crouch;
        if (canJump && input.Jump)
        {
            _verticalSpeed = movementConfig.jumpForce;
            _isGrounded = false;
            Debug.Log($"[PlayerMovement] Jump triggered! Force: {movementConfig.jumpForce}, Grounded: {_isGrounded}, WasGrounded: {_wasGroundedLastFrame}", this);
        }
        else if (input.Jump && !canJump)
        {
            Debug.Log($"[PlayerMovement] Jump input received but cannot jump. Grounded: {_isGrounded}, WasGrounded: {_wasGroundedLastFrame}, Crouch: {input.Crouch}", this);
        }
    }

    private void ApplyGravity()
    {
        if (_isGrounded)
        {
            // only apply slight downward force when landing to keep controller grounded
            if (_verticalSpeed < 0 && _verticalSpeed > -2f)
            {
                _verticalSpeed = -0.5f;
            }
        }
        else
        {
            _verticalSpeed -= movementConfig.gravity * Time.deltaTime;
        }
    }
    #endregion

    #region Crouch (height & center)
    private void UpdateCrouch(PlayerInputState input)
    {
        float desiredCrouchHeight = movementConfig != null ? movementConfig.crouchHeightMultiplier * standingHeight : standingHeight * 0.5f;
        _targetHeight = input.Crouch ? desiredCrouchHeight : standingHeight;

        // center computed relative to immutable baseline X/Z
        _targetCenter = new Vector3(_originalControllerCenter.x, _targetHeight / 2f, _originalControllerCenter.z);

        if (movementConfig != null)
        {
            _currentHeight = Mathf.Lerp(_currentHeight, _targetHeight, movementConfig.crouchTransitionSpeed * Time.deltaTime);
            _currentCenter = Vector3.Lerp(_currentCenter, _targetCenter, movementConfig.crouchTransitionSpeed * Time.deltaTime);
        }
        else
        {
            _currentHeight = _targetHeight;
            _currentCenter = _targetCenter;
        }

        bool heightChanged = Mathf.Abs(_controller.height - _currentHeight) > HeightApplyEpsilon;
        bool centerChanged = Vector3.Distance(_controller.center, _currentCenter) > CenterApplyEpsilon;

        if (heightChanged || centerChanged)
        {
            if (_targetHeight > _controller.height)
            {
                float checkOffset = (_targetHeight - _controller.height) + 0.05f;
                Vector3 origin = transform.position + Vector3.up * (_controller.height / 2f);
                if (!Physics.SphereCast(origin, _controller.radius * 0.9f, Vector3.up, out _, checkOffset, movementConfig.groundLayers, QueryTriggerInteraction.Ignore))
                {
                    ApplyControllerHeightCenter(_currentHeight, _currentCenter);
                }
                else
                {
                    // blocked: remain crouched but sync center to avoid misalignment
                    ApplyControllerHeightCenter(Mathf.Min(_controller.height, _currentHeight), _currentCenter);
                }
            }
            else
            {
                ApplyControllerHeightCenter(_currentHeight, _currentCenter);
            }
        }
    }

    private void ApplyControllerHeightCenter(float height, Vector3 center)
    {
        _controller.height = height;
        _controller.center = center;

        if (Mathf.Abs(_lastAppliedHeight - height) > HeightApplyEpsilon ||
            Vector3.Distance(_lastAppliedCenter, center) > CenterApplyEpsilon)
        {
            //Debug.Log($"[PlayerMovement] Applied CharacterController height={height:F3} center={center} at time={Time.time}", this);
            _lastAppliedHeight = height;
            _lastAppliedCenter = center;
        }
    }
    #endregion

    #region Camera
    private void SetCameraPos(PlayerInputState input)
    {
        if (playerCamera != null && movementConfig != null)
        {
            Vector3 camTarget = input.Crouch ? _cameraLocalPosCrouched : _cameraLocalPosStanding;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, camTarget, movementConfig.crouchTransitionSpeed * Time.deltaTime);
        }
    }
    #endregion

    #region Grounding & movement execution
    private void UpdateGround()
    {
        Vector3 origin = _transform.position + Vector3.up * 0.1f;
        float radius = Mathf.Max(0.1f, _controller.radius * 0.8f);

        _isGrounded = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, groundCheckDistance + 0.1f, movementConfig.groundLayers, QueryTriggerInteraction.Ignore)
                      && Vector3.Angle(hit.normal, Vector3.up) <= movementConfig.maxSlopeAngle;

        if (!_isGrounded && _verticalSpeed <= 0.1f)
        {
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit2, groundCheckDistance + 0.2f, movementConfig.groundLayers, QueryTriggerInteraction.Ignore))
            {
                _isGrounded = Vector3.Angle(hit2.normal, Vector3.up) <= movementConfig.maxSlopeAngle;
            }
        }

        if (!_isGrounded && _controller.isGrounded)
        {
            _isGrounded = true;
        }
    }

    private void PerformMove()
    {
        Vector3 finalVel = _horizontalVelocity + Vector3.up * _verticalSpeed;
        _controller.Move(finalVel * Time.deltaTime);
    }
    #endregion

    #region Utility
    public float CurrentHorizontalSpeed => new Vector3(_horizontalVelocity.x, 0, _horizontalVelocity.z).magnitude;
    #endregion
}
