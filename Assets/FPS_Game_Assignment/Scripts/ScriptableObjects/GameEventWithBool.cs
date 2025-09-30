using System;
using UnityEngine;


[CreateAssetMenu(menuName = "Events/Game Event With Bool")]
public class GameEventWithBool : ScriptableObject
{
    public event Action<bool> OnEventRaised;
    public void Raise(bool value)
    {
        OnEventRaised?.Invoke(value);
    }
}
