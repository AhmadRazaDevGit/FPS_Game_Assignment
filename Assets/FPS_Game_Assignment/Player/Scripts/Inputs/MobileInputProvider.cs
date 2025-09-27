using UnityEngine;

public class MobileInputProvider : MonoBehaviour, IInputProvider
{
    [Tooltip("Reference to the VirtualJoystick that provides movement input. Leave null to disable movement.")]
    [SerializeField] private VirtualJoystick moveJoystick;

    [Tooltip("Optional UI Button used to trigger a jump. When pressed, Jump will be true for the next GetInput() call.")]
    [SerializeField] private UnityEngine.UI.Button jumpButton;

    private PlayerInputState _state;

    private void Awake()
    {
        if (jumpButton != null) jumpButton.onClick.AddListener(OnJumpPressed);
    }

    private void OnDestroy()
    {
        if (jumpButton != null) jumpButton.onClick.RemoveListener(OnJumpPressed);
    }

    private void OnJumpPressed()
    {
        _state.Jump = true; // consumed when read by mover
    }

    public PlayerInputState GetInput()
    {
        Vector2 mv = moveJoystick != null ? moveJoystick.Output : Vector2.zero;
        _state.Move = mv;
        // reset after returning to avoid sticky jump
        var outState = _state;
        _state.Jump = false;
        return outState;
    }
}
