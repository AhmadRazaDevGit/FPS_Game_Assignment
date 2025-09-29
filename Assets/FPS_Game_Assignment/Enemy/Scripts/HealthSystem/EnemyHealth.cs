using System;
using UnityEngine;

public class EnemyHealth : HealthBase
{
    private BaseEnemy baseEnemy;
    public event Action OnEnemyDeath;
    protected override void Awake()
    {
        base.Awake();
        baseEnemy = GetComponent<BaseEnemy>();
    }
    public override void TakeDamage(float amount, GameObject source = null)
    {
        base.TakeDamage(amount, source);
        if (currentHealth > 0)
            baseEnemy.OnHit();
    }
    protected override void OnDeath(GameObject source)
    {
        OnEnemyDeath?.Invoke();
    }
}
