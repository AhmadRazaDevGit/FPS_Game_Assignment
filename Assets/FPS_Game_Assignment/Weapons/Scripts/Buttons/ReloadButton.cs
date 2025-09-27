using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class ReloadButton : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("Reference to the WeaponManager (scene).")]
    public WeaponManager weaponManager;

    // single-tap reload on pointer down
    public void OnPointerDown(PointerEventData eventData)
    {
        weaponManager?.StartReloadCurrent();
    }

    // alternatively: if  want Button onClick, simply wire to WeaponManager.StartReloadCurrent()
}
