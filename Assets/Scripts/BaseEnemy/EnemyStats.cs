using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "BossSystem/Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Base Enemy";

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float staggerThreshold = 25f; // Damage needed to 'flinch'

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float aggroRadius = 1500f;    // When it sees the roach
    public float attackRadius = 4f;     // When it bites the roach

    [Header("Combat Settings")]
    public float contactDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Phase Triggers")]
    public float phaseTwoThreshold = 0.5f; // 50% HP
}