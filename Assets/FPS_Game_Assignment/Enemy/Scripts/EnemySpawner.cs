using System;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public struct EnemyEntry
    {
        [Tooltip("Enemy prefab to spawn")]
        public GameObject prefab;

        [Tooltip("How many of this prefab to spawn (must be >= 0)")]
        public int amount;

        [Tooltip("Spawn point where to spawn the enemy (Pos)")]
        public Transform spawnPoint;
    }

    [Header("Spawn list (prefab + amount)")]
    [Tooltip("Add entries here. Example: element 0 -> prefab A, amount 2; element 1 -> prefab B, amount 3.")]
    [SerializeField] private EnemyEntry[] enemies;

    [Tooltip("If true, spawns automatically in Start()")]
    [SerializeField] private bool spawnOnStart = true;

    [Header("Time")]
    [Tooltip("Delay (seconds) between each spawn when using SpawnWithDelay. 0 for instant.)")]
    [SerializeField] private float delayBetweenSpawns = 0f;

    private WaitForSeconds spawnDelayTime;

    private void Start()
    {
        spawnDelayTime = new WaitForSeconds(delayBetweenSpawns);
        if (spawnOnStart)
            StartCoroutine(SpawnAllEnemy(delayBetweenSpawns));
    }

    public void SpawnAllWithDelay(float delayBetweenSpawns)
    {
        StartCoroutine(SpawnAllEnemy(delayBetweenSpawns));
    }

    private IEnumerator SpawnAllEnemy(float delay)
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            var entry = enemies[i];
            if (entry.prefab == null) continue;
            int safeAmount = Mathf.Max(0, entry.amount);
            for (int j = 0; j < safeAmount; j++)
            {
                SpawnOne(entry.prefab, i);
                if (delay > 0f) yield return spawnDelayTime;
            }
        }
    }

    private GameObject SpawnOne(GameObject prefab, int index)
    {
        GameObject go = Instantiate(prefab, enemies[index].spawnPoint.position, enemies[index].spawnPoint.rotation, transform);
        return go;
    }

    /// <summary>
    /// Helper for other scripts: spawn a specific prefab N times.
    /// </summary>
    public void SpawnPrefab(GameObject prefab, int amount)
    {
        if (prefab == null) return;
        int safeAmount = Mathf.Max(0, amount);
        for (int i = 0; i < safeAmount; i++) SpawnOne(prefab, i);
    }

}
