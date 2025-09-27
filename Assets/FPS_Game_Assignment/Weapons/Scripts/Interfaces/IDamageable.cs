using UnityEngine;
public interface IDamageable
{
    /// <summary>Apply damage. Return true if object died/was destroyed (optional).</summary>
    bool ApplyDamage(float amount, RaycastHit? hit = null);
}
