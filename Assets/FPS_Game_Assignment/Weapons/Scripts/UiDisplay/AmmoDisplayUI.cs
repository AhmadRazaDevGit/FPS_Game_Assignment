using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

[AddComponentMenu("UI/Ammo Display")]
public class AmmoDisplayUI : MonoBehaviour
{
    [Tooltip("Reference to the WeaponManager in scene.")]
    public WeaponManager weaponManager;

    [Header("Text target (assign one)")]
    [Tooltip("Assign either TextMeshProUGUI or UnityEngine.UI.Text. TMP preferred.")]
    public Text legacyText;



    [Tooltip("Format string for display. Use {0}=current, {1}=spare, {2}=magazineSize")]
    public string format = "{0}/{1}"; // default: "12/36"

    private WeaponBase _currentWeapon;

    private void OnEnable()
    {
        if (weaponManager != null)
            weaponManager.OnWeaponEquipped += OnWeaponEquipped;

        // subscribe to currently equipped if already available
        SubscribeToWeapon(weaponManager?.GetCurrentWeapon());
    }

    private void OnDisable()
    {
        if (weaponManager != null)
            weaponManager.OnWeaponEquipped -= OnWeaponEquipped;

        UnsubscribeFromWeapon();
    }

    private void OnWeaponEquipped(WeaponBase weapon)
    {
        SubscribeToWeapon(weapon);
    }

    private void SubscribeToWeapon(WeaponBase weapon)
    {
        UnsubscribeFromWeapon();
        _currentWeapon = weapon;
        if (_currentWeapon != null)
        {
            _currentWeapon.AmmoChanged += OnAmmoChanged;
            // push immediate update
            OnAmmoChanged(_currentWeapon.CurrentAmmo, _currentWeapon.SpareAmmo);
        }
        else
        {
            UpdateTextEmpty();
        }
    }

    private void UnsubscribeFromWeapon()
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.AmmoChanged -= OnAmmoChanged;
            _currentWeapon = null;
        }
    }

    private void OnAmmoChanged(int current, int spare)
    {
        int magSize = _currentWeapon != null && _currentWeapon.data != null ? _currentWeapon.data.magazineSize : 0;
        string text = string.Format(format, current, spare, magSize);
        SetText(text);
    }

    private void UpdateTextEmpty()
    {
        SetText("--/--");
    }

    private void SetText(string t)
    {
        if (legacyText != null)
        {
            legacyText.text = t;
            return;
        }
#if TMP_PRESENT
        if (tmpText != null)
        {
            tmpText.text = t;
            return;
        }
#endif
        // fallback: try to find Text on this or children
        var txt = GetComponentInChildren<UnityEngine.UI.Text>();
        if (txt != null) txt.text = t;
    }
}
