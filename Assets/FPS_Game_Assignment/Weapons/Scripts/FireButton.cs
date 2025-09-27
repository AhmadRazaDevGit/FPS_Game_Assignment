// UI/FireButtonBridge.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class FireButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public WeaponManager weaponManager;

    public void OnPointerDown(PointerEventData eventData)
    {
        weaponManager?.GetCurrentWeapon()?.TryStartFire();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        weaponManager?.GetCurrentWeapon()?.StopFire();
    }
}
