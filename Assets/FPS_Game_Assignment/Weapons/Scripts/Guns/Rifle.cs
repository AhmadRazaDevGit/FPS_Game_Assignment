public class Rifle : WeaponBase
{
    //// e.g. attach audio clips, recoil parameters etc.
    //[Header("Pistol Extras")]
    //[Tooltip("Recoil strength for camera kick (applies to camera controller if available).")]
    //public float recoilStrength = 1.0f;

    protected override void OnFireEffects()
    {
        base.OnFireEffects();
        // TODO: trigger muzzle flash particle (if attached)
        // TODO: call camera recoil method via an interface (avoid direct reference)
        // Example (pseudo):
        // var cam = Camera.main?.GetComponent<CameraRecoil>();
        // cam?.AddRecoil(recoilStrength);
    }
}
