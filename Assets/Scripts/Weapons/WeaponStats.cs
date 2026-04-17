using UnityEngine;

[CreateAssetMenu(fileName = "WeaponStats", menuName = "Scriptable Objects/WeaponStats")]
public class WeaponStats : ScriptableObject
{
    [SerializeField]
    public int lightAttackDamage;
    public int heavyAttackDamage;
    public int runningHeavyAttackDamage;
    public int range;
    public float attackSpeed;
}
