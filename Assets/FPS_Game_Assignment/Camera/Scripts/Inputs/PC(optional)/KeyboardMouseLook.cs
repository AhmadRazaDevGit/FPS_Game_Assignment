using UnityEngine;

[DisallowMultipleComponent]
public class KeyboardMouseLook : MonoBehaviour, ILookInputProvider
{
    [SerializeField] private string mouseX = "Mouse X";
    [SerializeField] private string mouseY = "Mouse Y";
    private LookInputState _state;

    public LookInputState GetLook()
    {
        // Using GetAxis is fine for mouse; for raw, use Input.GetAxisRaw or new Input System.
        float dx = Input.GetAxis(mouseX);
        float dy = Input.GetAxis(mouseY);
        // Unity's Input.GetAxis returns normalized values; scale later by sensitivity in controller.
        _state.Delta = new Vector2(dx, dy);
        _state.Active = Mathf.Abs(dx) > 0.0001f || Mathf.Abs(dy) > 0.0001f;
        return _state;
    }
}
