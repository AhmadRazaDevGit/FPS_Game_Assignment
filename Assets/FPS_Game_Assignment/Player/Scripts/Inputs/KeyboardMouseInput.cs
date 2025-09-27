using UnityEngine;

[DisallowMultipleComponent]
public class KeyboardMouseInput : MonoBehaviour, IInputProvider
{
    [Header("Bindings")]
    [Tooltip("Name of the Input Manager axis for horizontal movement (e.g. \"Horizontal\").")]
    [SerializeField] private string horizontalAxis = "Horizontal";

    [Tooltip("Name of the Input Manager axis for vertical movement (e.g. \"Vertical\").")]
    [SerializeField] private string verticalAxis = "Vertical";

    [Tooltip("Key to press to jump (checked with Input.GetKeyDown).")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    private PlayerInputState _state;

    public PlayerInputState GetInput()
    {
        // No memory allocation; use existing struct
        _state.Move.x = Input.GetAxisRaw(horizontalAxis);
        _state.Move.y = Input.GetAxisRaw(verticalAxis);
        _state.Jump = Input.GetKeyDown(jumpKey);
        return _state;
    }
}
