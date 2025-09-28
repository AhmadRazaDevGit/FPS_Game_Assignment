using System;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
}

public interface IHealth
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsDead { get; }

    event Action<float, float> OnHealthChanged;

    void TakeDamage(float amount, GameObject source = null);
    void Heal(float amount);
}
