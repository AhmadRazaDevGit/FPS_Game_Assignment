using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Tooltip("ScriptableObject that holds an array of enemy prefabs")]
    public Enimies enemiesPrefab;

    [Tooltip("If true, spawns automatically in Start()")]
    [SerializeField] private bool spawnOnStart = true;

    [Header("Time")]
    [Tooltip("Delay (seconds) between each spawn. 0 = spawn instantly, one after another.")]
    [SerializeField] private float delayBetweenSpawns = 0f;

    public Transform[] wayPoints;

    [Tooltip("Enemy Factory Refrence")]
    public EnemyFactory factory;

    private IEnemyFactory _factoryInterface;

    private void Awake()
    {
        _factoryInterface = factory as IEnemyFactory;
    }

    private void Start()
    {
        if (spawnOnStart)
            StartCoroutine(SpawnAllEnemy(delayBetweenSpawns));
    }

    /// <summary>
    /// Spawn all enemies instantly (no delay).
    /// </summary>
    public void SpawnAllNow()
    {
        if (enemiesPrefab == null || enemiesPrefab.enimies == null) return;

        foreach (var prefab in enemiesPrefab.enimies)
        {
            if (prefab == null) continue;
            SpawnOne(prefab);
        }
    }

    /// <summary>
    /// Start coroutine to spawn all enemies with given delay between each.
    /// </summary>
    public void SpawnAllWithDelay(float delay)
    {
        StartCoroutine(SpawnAllEnemy(delay));
    }

    private IEnumerator SpawnAllEnemy(float delay)
    {
        if (enemiesPrefab == null || enemiesPrefab.enimies == null) yield break;

        WaitForSeconds wait = (delay > 0f) ? new WaitForSeconds(delay) : null;

        for (int i = 0; i < enemiesPrefab.enimies.Length; i++)
        {
            var prefab = enemiesPrefab.enimies[i];
            if (prefab == null) continue;

            SpawnOne(prefab);

            if (wait != null)
                yield return wait;

        }
    }

    private GameObject SpawnOne(GameObject prefab)
    {
        if (_factoryInterface != null)
        {
            return _factoryInterface.Create(prefab, transform.position, transform.rotation, null, wayPoints);
        }
        GameObject enemy = Instantiate(prefab, transform.position, transform.rotation, transform);
        var assignable = enemy.GetComponent<IWayPointAssignable>();
        if (assignable != null) assignable.AssignWayPoints(wayPoints);
        enemy.SetActive(true);
        return enemy;
    }

    public void SpawnPrefab(GameObject prefab, int amount)
    {
        if (prefab == null) return;
        int safeAmount = Mathf.Max(0, amount);
        for (int i = 0; i < safeAmount; i++) SpawnOne(prefab);
    }

    [ContextMenu("FindWayPoints")]
    public void FindWayPoints()
    {
        WayPoint waypoint = FindObjectOfType<WayPoint>();
        wayPoints = waypoint.wayPoints;
        if (wayPoints == null) Debug.LogError("Could not found waypoints");
    }
}
