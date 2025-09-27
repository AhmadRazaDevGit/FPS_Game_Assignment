using UnityEngine;
using System;

/// <summary>
/// Mobile swipe look provider: tracks one touch and returns pixel delta since last frame.
/// Attach this to a scene object (e.g. Input/Providers) and reference it in CameraController.
/// </summary>
[DisallowMultipleComponent]
public class MobileSwipeLook : MonoBehaviour, ILookInputProvider
{
    [SerializeField] private LookConfig lookConfig; // optional, used only to decide right-half logic; provider remains pure input
    private LookInputState _state;

    // internal tracking
    private int _trackingFingerId = -1;
    private Vector2 _lastPos;

    public LookInputState GetLook()
    {
        _state.Delta = Vector2.zero;
        _state.Active = false;

        int count = Input.touchCount;
        if (count == 0) return _state;

        // If already tracking a finger, prefer it
        if (_trackingFingerId != -1)
        {
            for (int i = 0; i < count; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId == _trackingFingerId)
                {
                    if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                    {
                        Vector2 delta = t.deltaPosition;
                        // small deadzone filter if present
                        if (delta.sqrMagnitude > (lookConfig != null ? lookConfig.deadzonePixels * lookConfig.deadzonePixels : 1f))
                        {
                            _state.Delta = delta;
                            _state.Active = true;
                        }
                        _lastPos = t.position;
                    }
                    else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        _trackingFingerId = -1;
                    }
                    return _state;
                }
            }

            // tracked finger not found this frame -> clear
            _trackingFingerId = -1;
        }

        // Not tracking: find a touch to start tracking
        for (int i = 0; i < count; i++)
        {
            Touch t = Input.GetTouch(i);

            if (t.phase == TouchPhase.Began)
            {
                // optional right-half-only rule (config-driven)
                if (lookConfig != null && lookConfig.mobileRightHalfOnly)
                {
                    if (t.position.x < (Screen.width * 0.5f)) continue; // ignore left-half touches
                }
                // start tracking this finger for look
                _trackingFingerId = t.fingerId;
                _lastPos = t.position;
                _state.Active = false;
                _state.Delta = Vector2.zero;
                return _state;
            }
        }

        return _state;
    }
}
