using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RatController : BaseEnemy
{
    [Header("Rat Specific Setup")]
    public EnemyHitbox biteHitbox; // Assign the child object with the trigger here
    public MeshRenderer ratRenderer; // Assign this to make the rat flash during telegraphs

    private NavMeshAgent agent;
    private Transform player;
    private Color originalColor;

    protected override void InitializeEnemy()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = stats.moveSpeed;
        agent.stoppingDistance = stats.attackRadius - 0.5f; // Stop slightly before the bite hits

        if (ratRenderer != null) originalColor = ratRenderer.material.color;

        // Find player by tag (Ensure Hussain's player is tagged "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    // --- STATE LOGIC ---

    public override void CheckForPlayer()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= stats.aggroRadius)
        {
            currentState = EnemyState.Chasing;
        }
    }

    public override void MoveToPlayer()
    {
        if (player == null || currentState == EnemyState.Attacking) return;

        float distance = Vector3.Distance(transform.position, player.position);
        agent.SetDestination(player.position);

        // Transition to Attack if in range
        if (distance <= stats.attackRadius)
        {
            StartAttackSequence();
        }
    }

    // --- COMBAT SEQUENCE (SOULS-LIKE TELEGRAPH) ---

    private void StartAttackSequence()
    {
        currentState = EnemyState.Attacking;
        agent.isStopped = true; // Don't slide while biting

        // 1. THE TELEGRAPH (The "Wind-up")
        // Give the player 0.5s to see the 'hiss' or 'glow' and ROLL
        if (ratRenderer != null) ratRenderer.material.color = Color.red;

        Invoke(nameof(ExecuteBite), 0.5f);
    }

    private void ExecuteBite()
    {
        if (currentState == EnemyState.Dead) return;

        // 2. THE HITBOX (Active Frames)
        if (biteHitbox != null) biteHitbox.SetActive(true);

        // Hold the bite active for a short window
        Invoke(nameof(EndBite), 0.2f);
    }

    private void EndBite()
    {
        if (biteHitbox != null) biteHitbox.SetActive(false);
        if (ratRenderer != null) ratRenderer.material.color = originalColor;

        // 3. RECOVERY (The "Window" for the player to hit back)
        Invoke(nameof(ResetFromAttack), stats.attackCooldown);
    }

    private void ResetFromAttack()
    {
        if (currentState == EnemyState.Dead) return;

        agent.isStopped = false;
        currentState = EnemyState.Idle;
    }

    protected override void Die()
    {
        base.Die();
        agent.isStopped = true;
        agent.enabled = false;
        if (biteHitbox != null) biteHitbox.SetActive(false);
        if (ratRenderer != null) ratRenderer.material.color = Color.gray; // Gray out on death
    }
}