using System;
using UnityEngine;
public interface IHealth
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsDead { get; }

    event Action<float, float> OnHealthChanged;

    void TakeDamage(float amount, GameObject source = null);
    void Heal(float amount);
}
