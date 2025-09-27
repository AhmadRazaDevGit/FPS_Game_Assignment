using UnityEngine;

/// <summary>Look input for camera: delta in screen pixels since last frame</summary>
public struct LookInputState
{
    public Vector2 Delta;   // pixel delta; X = right, Y = up
    public bool Active;     // whether look input is currently active (touching / mouse held)
}

public interface ILookInputProvider
{
    LookInputState GetLook();
}
