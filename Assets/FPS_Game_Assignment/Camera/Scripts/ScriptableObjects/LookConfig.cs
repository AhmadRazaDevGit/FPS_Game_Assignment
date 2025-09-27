using UnityEngine;

[CreateAssetMenu(menuName = "Input/LookConfig", fileName = "LookConfig")]
public class LookConfig : ScriptableObject
{
    [Header("Sensitivity (degrees per pixel)")]
    [Tooltip("Horizontal sensitivity (degrees per pixel)")]
    public float sensitivityX = 0.12f;

    [Tooltip("Vertical sensitivity (degrees per pixel)")]
    public float sensitivityY = 0.12f;

    [Header("Limits")]
    [Tooltip("Minimum pitch (looking down) in degrees, usually negative")]
    public float minPitch = -80f;

    [Tooltip("Maximum pitch (looking up) in degrees")]
    public float maxPitch = 80f;

    [Header("Smoothing")]
    [Tooltip("Apply smoothing to rotations. 0 => no smoothing, larger => smoother/slower")]
    public float smoothing = 8f;

    [Header("Misc")]
    [Tooltip("Invert vertical look")]
    public bool invertY = false;

    [Tooltip("When true, mobile look is only active if touch started on the right half of the screen")]
    public bool mobileRightHalfOnly = true;

    [Tooltip("Ignore tiny delta moves (pixels) to reduce jitter")]
    public float deadzonePixels = 1f;
}
