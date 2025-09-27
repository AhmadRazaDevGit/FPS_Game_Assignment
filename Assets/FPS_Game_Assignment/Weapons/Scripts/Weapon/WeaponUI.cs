// WeaponUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private Text ammoText;
    [SerializeField] private Image reloadProgressImage; // fill image

    private Weapon _weapon;
    private Coroutine _reloadCoroutine;

    public void BindWeapon(Weapon w)
    {
        if (_weapon != null)
        {
            _weapon.OnAmmoChanged -= HandleAmmoChanged;
            _weapon.OnStartReload -= HandleStartReload;
            _weapon.OnEndReload -= HandleEndReload;
        }

        _weapon = w;

        if (_weapon != null)
        {
            _weapon.OnAmmoChanged += HandleAmmoChanged;
            _weapon.OnStartReload += HandleStartReload;
            _weapon.OnEndReload += HandleEndReload;
            HandleAmmoChanged(_weapon.CurrentAmmo, _weapon.ReserveAmmo);
        }
    }

    private void HandleAmmoChanged(int current, int reserve)
    {
        if (ammoText != null) ammoText.text = $"{current} / {reserve}";
    }

    private void HandleStartReload()
    {
        if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
        _reloadCoroutine = StartCoroutine(ReloadProgressRoutine());
    }

    private IEnumerator ReloadProgressRoutine()
    {
        if (_weapon == null) yield break;
        float start = Time.time;
        float duration = _weapon.IsReloading ? _weapon.config.reloadTime : 0f; // config used only as data - accessible via weapon? if not expose reloadTime
        // To keep Weapon UI fully decoupled from Weapon internals, Weapon should expose ReloadTime. For brevity, we'll assume config is accessible:
        duration = _weapon.config.reloadTime;

        while (Time.time - start < duration)
        {
            float p = (Time.time - start) / duration;
            if (reloadProgressImage != null) reloadProgressImage.fillAmount = p;
            yield return null;
        }
        if (reloadProgressImage != null) reloadProgressImage.fillAmount = 0;
        _reloadCoroutine = null;
    }

    private void HandleEndReload()
    {
        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }
        if (reloadProgressImage != null) reloadProgressImage.fillAmount = 0;
        HandleAmmoChanged(_weapon.CurrentAmmo, _weapon.ReserveAmmo);
    }
}
