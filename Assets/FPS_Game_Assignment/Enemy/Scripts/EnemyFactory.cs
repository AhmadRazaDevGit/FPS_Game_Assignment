
using UnityEngine;

public class EnemyFactory : MonoBehaviour, IEnemyFactory
{

    public GameObject Create(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, Transform[] wayPoints = null)
    {
        if (prefab == null) return null;


        GameObject go = Instantiate(prefab, position, rotation, parent);

        var assignable = go.GetComponent<IWayPointAssignable>();
        if (assignable != null && wayPoints != null)
            assignable.AssignWayPoints(wayPoints);

        go.SetActive(true);
        return go;
    }
}
