using UnityEngine;

public struct PlayerInputState
{
    public Vector2 Move;      // x = strafe, y = forward
    public bool Jump;      // edge: true for 1 frame when pressed
    public bool Crouch;    // true while held (or toggled by provider)
}

public interface IInputProvider
{
    PlayerInputState GetInput();
}
