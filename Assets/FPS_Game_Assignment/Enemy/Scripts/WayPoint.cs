using System.Linq;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    public Transform[] wayPoints;

    [ContextMenu("GetWayPoints")]
    public void GetWayPoints()
    {
        // includeInactive: true if you want inactive children too
        wayPoints = GetComponentsInChildren<Transform>(true)
                     .Where(t => t != transform)
                     .ToArray();
    }
}
