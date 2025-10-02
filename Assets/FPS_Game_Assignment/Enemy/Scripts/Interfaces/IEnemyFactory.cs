using UnityEngine;

public interface IEnemyFactory
{
    GameObject Create(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, Transform[] wayPoints = null);
}
