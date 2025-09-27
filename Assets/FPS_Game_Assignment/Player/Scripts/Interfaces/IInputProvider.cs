using UnityEngine;

public struct PlayerInputState
{
    public Vector2 Move;      // x = strafe, y = forward
    public bool Jump;
}

public interface IInputProvider
{
    PlayerInputState GetInput();
}
