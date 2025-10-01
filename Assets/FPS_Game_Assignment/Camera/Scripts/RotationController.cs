using UnityEngine;

[RequireComponent(typeof(Transform))]
public class RotationController : MonoBehaviour
{
    [Header("Config (SO-Properties)")]
    [SerializeField] private LookConfig lookConfig;

    [Header("Dependencies")]
    [Tooltip("Component implementing ILookInputProvider (e.g. MobileSwipeLook or KeyboardMouseLook)")]
    [SerializeField] private MonoBehaviour lookInputProviderComponent;

    [Tooltip("Camera transform (child). Pitch is applied here.")]
    [SerializeField] private Transform playerCamera;

    // runtime references
    private ILookInputProvider _lookInputProvider;
    private Transform _playerTransform;

    // rotation state (degrees)
    private float _yaw;   // world yaw (applied to player transform)
    private float _pitch; // local camera pitch (applied to camera local rotation)

    // smoothing state
    private float _targetYaw;
    private float _targetPitch;

    private void Awake()
    {
        _playerTransform = transform;
        _lookInputProvider = lookInputProviderComponent as ILookInputProvider;
        if (_lookInputProvider == null)
            Debug.LogError($"[{nameof(RotationController)}] Look input provider missing or does not implement ILookInputProvider.", this);

        if (playerCamera == null) Debug.LogError($"[{nameof(RotationController)}] playerCamera is not assigned.", this);

        // initialize angles from current transforms
        Vector3 euler = _playerTransform.eulerAngles;
        _yaw = _targetYaw = euler.y;

        if (playerCamera != null)
        {
            // get camera pitch from localEulerAngles.x, convert to -180..180
            float camX = playerCamera.localEulerAngles.x;
            if (camX > 180f) camX -= 360f;
            _pitch = _targetPitch = camX;
        }
    }

    private void Update()
    {
        if (_lookInputProvider == null || lookConfig == null) return;

        var look = _lookInputProvider.GetLook();

        // If using mouse axes provider, GetLook().Delta may be normalized; scale accordingly.
        Vector2 deltaPixels = look.Delta;


        if (lookInputProviderComponent is KeyboardMouseLook)
        {
            // treat axis in range ~[-1..1] -> scale to pseudo-pixels
            deltaPixels *= 16f;
        }

        // Invert Y if requested
        float inv = lookConfig.invertY ? 1f : -1f;

        // Calculate degrees to rotate (degrees = pixels * sensitivity)
        float yawDelta = deltaPixels.x * lookConfig.sensitivityX;
        float pitchDelta = deltaPixels.y * lookConfig.sensitivityY * inv;

        // deadzone check to avoid tiny jitter
        if (deltaPixels.sqrMagnitude <= lookConfig.deadzonePixels * lookConfig.deadzonePixels)
        {
            yawDelta = 0f;
            pitchDelta = 0f;
        }

        // update targets
        _targetYaw += yawDelta;
        _targetPitch += pitchDelta;

        // clamp pitch
        _targetPitch = Mathf.Clamp(_targetPitch, lookConfig.minPitch, lookConfig.maxPitch);

        
        if (lookConfig.smoothing > 0f)
        {
            float t = 1f - Mathf.Exp(-lookConfig.smoothing * Time.deltaTime); // smooth exponential lerp (frame-rate independent)
            _yaw = Mathf.Lerp(_yaw, _targetYaw, t);
            _pitch = Mathf.Lerp(_pitch, _targetPitch, t);
        }
        else
        {
            _yaw = _targetYaw;
            _pitch = _targetPitch;
        }

        // Apply rotations: yaw to player root (only Y), pitch to camera local (only X)
        _playerTransform.rotation = Quaternion.Euler(0f, _yaw, 0f);

        if (playerCamera != null)
        {
            // set local rotation with pitch; preserve camera local Y/Z as zeroed
            playerCamera.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }

    /// <summary>Expose pitch & yaw for other systems (e.g., weapon sway, aiming)</summary>
    public (float yaw, float pitch) GetRotation() => (_yaw, _pitch);
}
