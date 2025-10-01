using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/WeaponPrefabs", fileName = "Weapons")]
public class Weapons : ScriptableObject
{
    [Tooltip("Weapons list that can player used")]
    public GameObject[] weapons;
}
