using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    public EnemyStats stats; // Drag your ScriptableObject here
    protected float currentHealth;

    // THE STATE MACHINE
    public enum EnemyState { Idle, Chasing, Attacking, Staggered, Dead }
    public EnemyState currentState = EnemyState.Idle;

    private void Start()
    {
        currentHealth = stats.maxHealth;
        InitializeEnemy();
    }

    // VIRTUAL: This allows Watson or the Rat to add their own unique start logic
    protected virtual void InitializeEnemy() { }

    private void Update()
    {
        if (currentState == EnemyState.Dead) return;
        HandleStateMachine();
    }

    private void HandleStateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                CheckForPlayer();
                break;
            case EnemyState.Chasing:
                MoveToPlayer();
                break;
            case EnemyState.Attacking:
                // Logic handled by Animation Events later
                break;
        }
    }

    // EMPTY NUBS: These are for Kyler and the others to fill in later
    public virtual void CheckForPlayer() { /* Rat logic here */ }
    public virtual void MoveToPlayer() { /* NavMesh logic here */ }
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(stats.enemyName + " took " + amount + " damage!");
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        currentState = EnemyState.Dead;
        Debug.Log(stats.enemyName + " has been defeated.");
    }
}