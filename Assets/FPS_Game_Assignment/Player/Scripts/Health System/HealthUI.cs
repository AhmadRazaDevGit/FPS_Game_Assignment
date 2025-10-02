using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Tooltip("The Image used as fill (Image.type = Filled).")]
    [SerializeField] private Image healthFill;


    [Tooltip("Who to observe. Leave null to attempt to find an IHealth on the parent or scene object at Start.")]
    [SerializeField] private GameObject healthObject;


    [Tooltip("Smooth interpolation speed for fill (0 = immediate).")]
    [SerializeField] private float lerpSpeed = 8f;

    [Tooltip("Text that will show max and current health")]
    [SerializeField] Text healthText;

    private float displayedFill = 1f;
    private IHealth iHealth;

    private EnemyHealth enemyHealth;

    void Start()
    {
        if (healthFill == null)
            Debug.LogWarning("HealthUI: healthFill is not assigned.", this);
        if (healthObject == null) healthObject = transform.root.gameObject;
        iHealth = healthObject.GetComponent<IHealth>();
        enemyHealth = healthObject.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
            enemyHealth.OnEnemyDeath += Died;
        if (iHealth != null)
        {
            // initialize UI
            UpdateFillInstant(iHealth.CurrentHealth, iHealth.MaxHealth);
            iHealth.OnHealthChanged += OnHealthChanged;

        }
        else
        {
            Debug.LogWarning("HealthUI: No IHealth found to observe.", this);
        }
    }


    private void OnDisable()
    {
        if (iHealth != null)
            iHealth.OnHealthChanged -= OnHealthChanged;
        if (enemyHealth != null)
            enemyHealth.OnEnemyDeath -= Died;
    }
    private void Died()
    {
        gameObject.SetActive(false);
    }

    private void OnHealthChanged(float current, float max)
    {
        healthText.text = $"{current}/{max}";
        float targetFill = max <= 0f ? 0f : Mathf.Clamp01(current / max);
        StopAllCoroutines();
        StartCoroutine(LerpFill(targetFill));
    }


    private IEnumerator LerpFill(float target)
    {
        // smooth the fill value
        while (!Mathf.Approximately(displayedFill, target))
        {
            displayedFill = Mathf.MoveTowards(displayedFill, target, Time.deltaTime * lerpSpeed);
            if (healthFill != null) healthFill.fillAmount = displayedFill;
            yield return null;
        }
    }


    private void UpdateFillInstant(float current, float max)
    {
        healthText.text = $"{current}/{max}";
        displayedFill = (max <= 0f) ? 0f : Mathf.Clamp01(current / max);
        if (healthFill != null) healthFill.fillAmount = displayedFill;
    }
}