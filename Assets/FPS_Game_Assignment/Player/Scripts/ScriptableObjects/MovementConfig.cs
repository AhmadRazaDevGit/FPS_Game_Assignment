using UnityEngine;

[CreateAssetMenu(menuName = "Movement/MovementConfig", fileName = "MovementConfig")]
public class MovementConfig : ScriptableObject
{
    [Header("Speeds")]
    [Tooltip("Ground walk speed (units/sec)")]
    public float walkSpeed = 3.5f;

    [Header("Acceleration")]
    [Tooltip("How quickly velocity approaches target on ground")]
    public float groundAcceleration = 20f;

    [Tooltip("How quickly velocity approaches target in air")]
    public float airAcceleration = 6f;

    [Header("Gravity & Jump")]
    [Tooltip("Gravity applied downward (positive number)")]
    public float gravity = 9.81f;

    [Tooltip("Jump impulse (optional)")]
    public float jumpForce = 6f;

    [Header("Crouch")]
    [Tooltip("Height multiplier applied when crouching (0..1)")]
    [Range(0.3f, 1f)]
    public float crouchHeightMultiplier = 0.5f;

    [Tooltip("Movement speed multiplier while crouching (0..1)")]
    [Range(0.1f, 1f)]
    public float crouchSpeedMultiplier = 0.5f;

    [Tooltip("How fast to interpolate height & camera when crouching")]
    public float crouchTransitionSpeed = 8f;

    [Header("Misc")]
    [Tooltip("How much control player has while in air (0..1)")]
    [Range(0f, 1f)]
    public float airControl = 0.6f;

    [Tooltip("Max slope angle considered walkable")]
    public float maxSlopeAngle = 50f;

    [Tooltip("Layer mask used for ground checks")]
    public LayerMask groundLayers = ~0;
}
