using UnityEngine;

public class MobileInputProvider : MonoBehaviour, IInputProvider
{
    [Tooltip("Reference to the VirtualJoystick that provides movement input. Leave null to disable movement.")]
    [SerializeField] private VirtualJoystick moveJoystick;

    [Tooltip("If true, sprint should be triggered by double-tapping the joystick. Note: double-tap handling must be implemented separately.")]
    [SerializeField] private bool sprintButtonUsesDoubleTap = false;

    [Tooltip("Optional UI Button to toggle sprint on/off. If assigned, its onClick will toggle sprint state.")]
    [SerializeField] private UnityEngine.UI.Button sprintButton; // optional

    [Tooltip("Optional UI Button used to trigger a jump. When pressed, Jump will be true for the next GetInput() call.")]
    [SerializeField] private UnityEngine.UI.Button jumpButton; // optional

    private PlayerInputState _state;

    private void Awake()
    {
        if (sprintButton != null) sprintButton.onClick.AddListener(OnSprintToggle);
        if (jumpButton != null) jumpButton.onClick.AddListener(OnJumpPressed);
    }

    private void OnDestroy()
    {
        if (sprintButton != null) sprintButton.onClick.RemoveListener(OnSprintToggle);
        if (jumpButton != null) jumpButton.onClick.RemoveListener(OnJumpPressed);
    }

    private bool _sprintToggle;

    private void OnSprintToggle()
    {
        _sprintToggle = !_sprintToggle;
    }

    private void OnJumpPressed()
    {
        _state.Jump = true; // consumed when read by mover
    }

    public PlayerInputState GetInput()
    {
        Vector2 mv = moveJoystick != null ? moveJoystick.Output : Vector2.zero;
        _state.Move = mv;
        _state.Sprint = sprintButton != null ? _sprintToggle : false;
        // _state.Jump might be set by button; reset after returning to avoid sticky jump
        var outState = _state;
        _state.Jump = false;
        return outState;
    }
}
