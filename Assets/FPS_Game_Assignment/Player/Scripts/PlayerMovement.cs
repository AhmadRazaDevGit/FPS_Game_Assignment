using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Config (data-only)")]
    [SerializeField] private MovementConfig movementConfig;

    [Header("Dependencies")]
    [Tooltip("Component implementing IInputProvider. Can be KeyboardMouseInput or MobileInputProvider")]
    [SerializeField] private MonoBehaviour inputProviderComponent;

    [Header("Settings")]
    [SerializeField, Tooltip("Height from player origin to sphere used to check grounding")]
    private float groundCheckDistance = 0.2f;

    private IInputProvider _inputProvider;
    private CharacterController _controller;
    private Transform _transform;

    // Internal state
    private Vector3 _velocity; // world space
    private Vector3 _horizontalVelocity; // velocity xz only
    private bool _isGrounded;
    private float _verticalSpeed;

    private void Reset()
    {
        inputProviderComponent = GetComponentInChildren<KeyboardMouseInput>() as MonoBehaviour;
    }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _transform = transform;

        // validate DI
        if (movementConfig == null) Debug.LogError($"[{nameof(PlayerMovement)}] MovementConfig is not assigned.", this);
        if (inputProviderComponent == null) Debug.LogError($"[{nameof(PlayerMovement)}] InputProviderComponent is not assigned.", this);

        _inputProvider = inputProviderComponent as IInputProvider;
        if (_inputProvider == null) Debug.LogError($"[{nameof(PlayerMovement)}] Input provider does not implement IInputProvider.", this);
    }

    private void Update()
    {
        var input = _inputProvider.GetInput(); // read input (no allocations)

    
        GroundCheck();

        
        Vector3 desiredLocal = new Vector3(input.Move.x, 0f, input.Move.y);
        desiredLocal = Vector3.ClampMagnitude(desiredLocal, 1f); // prevent faster diagonal
        float targetSpeed = movementConfig.walkSpeed * (input.Sprint ? movementConfig.sprintMultiplier : 1f);

        Vector3 desiredWorld = _transform.TransformDirection(desiredLocal) * targetSpeed;

        
        float acceleration = _isGrounded ? movementConfig.groundAcceleration : movementConfig.airAcceleration;
        _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, new Vector3(desiredWorld.x, 0, desiredWorld.z), acceleration * Time.deltaTime);

        // Optional: apply reduced control in air
        if (!_isGrounded)
        {
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, new Vector3(desiredWorld.x, 0, desiredWorld.z), movementConfig.airControl * Time.deltaTime);
        }

        // Vertical (gravity & jump)
        if (_isGrounded)
        {
            if (input.Jump)
            {
                _verticalSpeed = movementConfig.jumpForce;
                _isGrounded = false;
            }
            else
            {
                _verticalSpeed = -0.5f; // small downward to keep grounded on slopes
            }
        }
        else
        {
            _verticalSpeed -= movementConfig.gravity * Time.deltaTime;
        }

        // Compose final velocity and move
        _velocity = _horizontalVelocity + Vector3.up * _verticalSpeed;
        _controller.Move(_velocity * Time.deltaTime);
    }
    private void GroundCheck()
    {
      //  Replace SphereCast ground check with CharacterController.Move velocity checks for some designs — I used SphereCast because it handles edges / small steps better cross - platform.
        Vector3 origin = _transform.position + Vector3.up * 0.1f;
        float radius = Mathf.Max(0.05f, _controller.radius * 0.9f);
        _isGrounded = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, groundCheckDistance + 0.05f, movementConfig.groundLayers, QueryTriggerInteraction.Ignore)
                      && Vector3.Angle(hit.normal, Vector3.up) <= movementConfig.maxSlopeAngle;
    }

    // Allow external systems (aim, camera) to query current horizontal speed
    public float CurrentHorizontalSpeed => new Vector3(_horizontalVelocity.x, 0, _horizontalVelocity.z).magnitude;
}
