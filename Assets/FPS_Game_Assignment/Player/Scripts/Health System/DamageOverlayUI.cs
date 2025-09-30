using System.Threading.Tasks;
using UnityEngine;

public class DamageOverlayUI : MonoBehaviour
{
    [Header("Source")]

    [Tooltip("Assign the GameObject that implements IHealth (player). If left null this will try to auto-find a component implementing IHealth on the same GameObject or parents.")]
    [SerializeField] private HealthBase healthComponent;

    [Tooltip("Damage overlay gameobjec")]
    [SerializeField] private GameObject damageOverlay;

    private IHealth health;
    private void Awake()
    {
        if (healthComponent == null)
            healthComponent = GetComponentInParent<HealthBase>();
        health = healthComponent;

    }
    private void OnEnable()
    {
        health.OnHealthChanged += ShowDamageOverlay;
    }
    private void OnDisable()
    {
        health.OnHealthChanged -= ShowDamageOverlay;
    }

    private async void ShowDamageOverlay(float arg1, float arg2)
    {
        if (damageOverlay != null)
        {
            damageOverlay.SetActive(true);
            await Task.Delay(125);
            damageOverlay.SetActive(false);
        }

    }


}
