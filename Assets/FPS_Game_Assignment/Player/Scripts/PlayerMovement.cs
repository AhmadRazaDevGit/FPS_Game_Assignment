using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Config (data-only)")]
    [SerializeField] private MovementConfig movementConfig;

    [Header("Dependencies")]
    [Tooltip("Component implementing IInputProvider. Can be KeyboardMouseInput or MobileInputProvider")]
    [SerializeField] private MonoBehaviour inputProviderComponent;

    [Header("Crouch/Camera")]
    [Tooltip("Camera will be moved down/up with crouch")]
    [SerializeField] private Transform playerCamera;

    [SerializeField, Tooltip("Capsule height when not crouched. If zero, will use CharacterController.height as default")]
    private float standingHeight = 0f;

    [SerializeField, Tooltip("CharacterController radius override (0 to use existing)")]
    private float controllerRadius = 0f;

    [Header("Settings")]
    [SerializeField, Tooltip("Height from player origin to sphere used to check grounding")]
    private float groundCheckDistance = 0.3f;

    private IInputProvider _inputProvider;
    private CharacterController _controller;
    private Transform _transform;

    // Internal state
    private Vector3 _horizontalVelocity;
    private bool _isGrounded;
    private float _verticalSpeed;
    private bool _wasGroundedLastFrame; // Track previous grounded state for jump buffering

    // crouch smoothing state
    private float _targetHeight;
    private float _currentHeight;
    private Vector3 _targetCenter;
    private Vector3 _currentCenter;
    private Vector3 _cameraLocalPosStanding;
    private Vector3 _cameraLocalPosCrouched;

    // baseline preserved from Awake (immutable baseline)
    private Vector3 _originalControllerCenter;

    // small threshold before applying changes to avoid visual jitter
    private const float HeightApplyEpsilon = 0.01f;
    private const float CenterApplyEpsilon = 0.01f;

    // debug: track last applied values so we only log when a real change happens
    private float _lastAppliedHeight;
    private Vector3 _lastAppliedCenter;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _transform = transform;

        // warn about duplicate colliders (common source of visual offset)
        if (GetComponent<CapsuleCollider>() != null)
        {
            Debug.LogWarning("[PlayerMovement] Found a CapsuleCollider on the same GameObject as a CharacterController. Remove CapsuleCollider when using CharacterController.", this);
        }

        if (movementConfig == null) Debug.LogError($"[{nameof(PlayerMovement)}] MovementConfig is not assigned.", this);
        if (inputProviderComponent == null) Debug.LogError($"[{nameof(PlayerMovement)}] InputProviderComponent is not assigned.", this);

        _inputProvider = inputProviderComponent as IInputProvider;
        if (_inputProvider == null) Debug.LogError($"[{nameof(PlayerMovement)}] Input provider does not implement IInputProvider.", this);

        // ensure consistent baseline height/center
        if (standingHeight <= 0f) standingHeight = _controller.height;
        _currentHeight = standingHeight;
        _targetHeight = standingHeight;

        // preserve baseline center and force controller center to match standingHeight/2
        _originalControllerCenter = _controller.center;
        Vector3 forcedCenter = new Vector3(_originalControllerCenter.x, standingHeight / 2f, _originalControllerCenter.z);

        // apply baseline to controller immediately to avoid drift
        _controller.height = standingHeight;
        _controller.center = forcedCenter;

        _currentCenter = forcedCenter;
        _targetCenter = forcedCenter;

        // camera targets (safe-guard movementConfig exists)
        if (playerCamera != null && movementConfig != null)
        {
            _cameraLocalPosStanding = playerCamera.localPosition;
            _cameraLocalPosCrouched = new Vector3(_cameraLocalPosStanding.x,
                                                  _cameraLocalPosStanding.y * movementConfig.crouchHeightMultiplier,
                                                  _cameraLocalPosStanding.z);
        }

        if (controllerRadius > 0f) _controller.radius = controllerRadius;

        // init last-applied trackers
        _lastAppliedHeight = _controller.height;
        _lastAppliedCenter = _controller.center;
    }

    private void Update()
    {
        var input = _inputProvider.GetInput();

        // Store previous grounded state for jump buffering
        _wasGroundedLastFrame = _isGrounded;
        
        GroundCheck();

        // movement input
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

        // Jump (edge) � do not allow jump while crouched
        // Allow jump if grounded OR if we were grounded recently (jump buffering)
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

        // Apply gravity and handle grounded state
        if (_isGrounded)
        {
            // Only apply downward force if we're actually landing (slow downward movement)
            // This prevents the slowdown when falling
            if (_verticalSpeed < 0 && _verticalSpeed > -2f) 
            {
                _verticalSpeed = -0.5f; // Only when actually landing
            }
        }
        else
        {
            // Apply gravity when in air - no interference with falling
            _verticalSpeed -= movementConfig.gravity * Time.deltaTime;
        }

        // ---- CROUCH: compute targets using immutable baseline (no drift) ----
        float desiredCrouchHeight = movementConfig != null ? movementConfig.crouchHeightMultiplier * standingHeight : standingHeight * 0.5f;
        _targetHeight = input.Crouch ? desiredCrouchHeight : standingHeight;

        // compute center relative to original baseline X/Z but using half of targetHeight
        _targetCenter = new Vector3(_originalControllerCenter.x, _targetHeight / 2f, _originalControllerCenter.z);

        // Smooth local values
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

        // Only apply if change bigger than epsilon to avoid jitter
        bool heightChanged = Mathf.Abs(_controller.height - _currentHeight) > HeightApplyEpsilon;
        bool centerChanged = Vector3.Distance(_controller.center, _currentCenter) > CenterApplyEpsilon;

        if (heightChanged || centerChanged)
        {
            // if increasing height (standing), ensure space is clear
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
                    // blocked: don't stand, but keep center in sync to avoid misalignment
                    ApplyControllerHeightCenter(Mathf.Min(_controller.height, _currentHeight), _currentCenter);
                }
            }
            else
            {
                // lowering height or changing center: apply directly
                ApplyControllerHeightCenter(_currentHeight, _currentCenter);
            }
        }

        // Move camera if provided (localY moves similar to crouch)
        if (playerCamera != null && movementConfig != null)
        {
            Vector3 camTarget = input.Crouch ? _cameraLocalPosCrouched : _cameraLocalPosStanding;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, camTarget, movementConfig.crouchTransitionSpeed * Time.deltaTime);
        }

        // Final move
        Vector3 finalVel = _horizontalVelocity + Vector3.up * _verticalSpeed;
        _controller.Move(finalVel * Time.deltaTime);
    }

    private void ApplyControllerHeightCenter(float height, Vector3 center)
    {
        // Apply and log change only if it differs from last-applied tracked values
        _controller.height = height;
        _controller.center = center;

        if (Mathf.Abs(_lastAppliedHeight - height) > HeightApplyEpsilon ||
            Vector3.Distance(_lastAppliedCenter, center) > CenterApplyEpsilon)
        {
            Debug.Log($"[PlayerMovement] Applied CharacterController height={height:F3} center={center} at time={Time.time}", this);
            _lastAppliedHeight = height;
            _lastAppliedCenter = center;
        }
    }

    private void GroundCheck()
    {
        Vector3 origin = _transform.position + Vector3.up * 0.1f;
        float radius = Mathf.Max(0.1f, _controller.radius * 0.8f);
        
        // Use SphereCast for more reliable ground detection
        _isGrounded = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, groundCheckDistance + 0.1f, movementConfig.groundLayers, QueryTriggerInteraction.Ignore)
                      && Vector3.Angle(hit.normal, Vector3.up) <= movementConfig.maxSlopeAngle;
        
        // Additional check: if we're very close to the ground and not moving up, consider grounded
        // Allow ground detection even when falling fast, but only apply slowdown when landing
        if (!_isGrounded && _verticalSpeed <= 0.1f)
        {
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit2, groundCheckDistance + 0.2f, movementConfig.groundLayers, QueryTriggerInteraction.Ignore))
            {
                _isGrounded = Vector3.Angle(hit2.normal, Vector3.up) <= movementConfig.maxSlopeAngle;
            }
        }
        
        // Fallback: Use CharacterController's built-in ground detection as backup
        if (!_isGrounded && _controller.isGrounded)
        {
            _isGrounded = true;
        }
        
    }

    public float CurrentHorizontalSpeed => new Vector3(_horizontalVelocity.x, 0, _horizontalVelocity.z).magnitude;
}
