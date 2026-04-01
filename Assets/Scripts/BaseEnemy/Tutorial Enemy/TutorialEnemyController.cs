using UnityEngine;
using UnityEngine.AI; // We need this for NavMesh movement

public class TutorialEnemyController : BaseEnemy
{
    [Header("Boss Specific Setup")]
    public EnemyHitbox attackHitbox; // Assign the child object with the trigger here
    public MeshRenderer bossRender; // Assign this to make the rat flash during telegraphs

    private NavMeshAgent agent;
    private Transform player;
    private Color originalColor;

    protected override void InitializeEnemy()
    {
        // Setup the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        agent.speed = stats.moveSpeed;
        agent.stoppingDistance = stats.attackRadius - 0.5f; // Stop slightly before the bite hits

        if (bossRender != null) originalColor = bossRender.material.color;

        // Temporary: Find the player by tag
        // Hussain needs to make sure the Player object is tagged "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    public override void CheckForPlayer()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= stats.aggroRadius)
        {
            currentState = EnemyState.Chasing;
            Debug.Log("Enemy spotted the Roach!");
        }
    }

    public override void MoveToPlayer()
    {
        if (player == null || currentState == EnemyState.Attacking) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Tell the NavMesh to walk toward the player
        agent.SetDestination(player.position);

        // If close enough, stop and attack
        if (distance <= stats.attackRadius)
        {
            StartAttackSequence();
        }
    }

    private void StartAttackSequence()
    {
        currentState = EnemyState.Attacking;
        agent.isStopped = true; // Don't slide while biting

        // 1. THE TELEGRAPH (The "Wind-up")
        // Give the player 0.5s to see the 'hiss' or 'glow' and ROLL
        if (bossRender != null) bossRender.material.color = Color.indianRed;

        Invoke(nameof(ExecuteBite), 0.5f);
    }

    private void ExecuteBite()
    {
        if (currentState == EnemyState.Dead) return;

        // 2. THE HITBOX (Active Frames)
        if (attackHitbox != null) attackHitbox.SetActive(true);

        // Hold the bite active for a short window
        Invoke(nameof(EndBite), 0.5f);
    }

    private void EndBite()
    {
        if (attackHitbox != null) attackHitbox.SetActive(false);
        if (bossRender != null) bossRender.material.color = originalColor;

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
        if (attackHitbox != null) attackHitbox.gameObject.SetActive(false);
        if (bossRender != null) bossRender.material.color = Color.gray; // Gray out on death
    }
}