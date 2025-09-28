using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
}

public interface IHealth
{
    void TakeDamage(float amount);
}
