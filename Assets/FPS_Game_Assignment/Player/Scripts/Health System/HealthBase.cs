using System;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class HealthBase : MonoBehaviour, IHealth
{
    [Header("Health")]
    [SerializeField, Min(1f)]
    protected float maxHealth = 100f;


    protected float currentHealth;
    protected float lastDamageTime = -999f;


    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead { get; protected set; }


    public event Action<float, float> OnHealthChanged = delegate { };


    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float amount, GameObject source = null)
    {
        if (IsDead) return;

        lastDamageTime = Time.time;


        float old = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, amount));


        if (Mathf.Approximately(currentHealth, old) == false)
            OnHealthChanged.Invoke(currentHealth, maxHealth);


        if (currentHealth <= 0f)
        {
            Die(source);
        }
    }


    public virtual void Heal(float amount)
    {
        if (IsDead) return; // cannot heal dead things by default


        float old = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0f, amount));


        if (Mathf.Approximately(currentHealth, old) == false)
            OnHealthChanged.Invoke(currentHealth, maxHealth);
    }

    protected virtual void Die(GameObject source)
    {
        if (IsDead) return;
        IsDead = true;
        OnHealthChanged.Invoke(0f, maxHealth);
        OnDeath(source);
    }


    
    // Override this in concrete classes for specialized death handling (disable movement, play anim, etc.)
    
    protected abstract void OnDeath(GameObject source);
}