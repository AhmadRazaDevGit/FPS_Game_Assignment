using UnityEngine;
using UnityEngine.UI;

public class MobileInputProvider : MonoBehaviour, IInputProvider
{
    [Tooltip("Reference to the VirtualJoystick that provides movement input. Leave null to disable movement.")]
    [SerializeField] private VirtualJoystick moveJoystick;

    [Tooltip("Button used to trigger a jump. When pressed, Jump will be true for the next GetInput() call.")]
    [SerializeField] private Button jumpButton;

    [Tooltip("Button used to trigger a crouch. When pressed, crouch will be true for the next GetInput() call.")]
    [SerializeField] private Button crouchButton;

    [Tooltip("If true, crouch button toggles crouch. If false, it's treated as hold (press-and-hold).")]
    [SerializeField] private bool crouchToggle = true;

    private PlayerInputState _state;

    // Buffered/event flags to avoid race conditions
    private bool jumpRequested;
    private bool crouchToggledState; // stores toggle state when using toggle mode
    private bool crouchHoldState;    // used when crouch button is held via pointer events

    private void Awake()
    {
        if (jumpButton != null) jumpButton.onClick.AddListener(OnJumpPressed);
        if (crouchButton != null)
        {
            // If using toggle mode, map onClick -> toggle
            if (crouchToggle) crouchButton.onClick.AddListener(OnCrouchToggle);
        }
    }

    private void OnDestroy()
    {
        if (jumpButton != null) jumpButton.onClick.RemoveListener(OnJumpPressed);
        if (crouchButton != null && crouchToggle) crouchButton.onClick.RemoveListener(OnCrouchToggle);
    }

    public void OnJumpPressed()
    {
        jumpRequested = true;
    }
    // Crouch toggle via button onClick
    public void OnCrouchToggle()
    {
        crouchToggledState = !crouchToggledState;
    }


    // For hold-mode, hook these to EventTrigger -> PointerDown/PointerUp
    public void OnCrouchHoldStart()
    {
        crouchHoldState = true;
    }

    public void OnCrouchHoldEnd()
    {
        crouchHoldState = false;
    }
    public PlayerInputState GetInput()
    {
        Vector2 mv = moveJoystick != null ? moveJoystick.Output : Vector2.zero;
        _state.Move = mv;

        // Consume buffered jump (edge)
        _state.Jump = jumpRequested;
        jumpRequested = false;

        // Crouch: either toggle mode or hold mode
        _state.Crouch = crouchToggle ? crouchToggledState : crouchHoldState;

        return _state;
    }
}
