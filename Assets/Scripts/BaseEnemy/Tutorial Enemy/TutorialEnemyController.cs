using UnityEngine;
using UnityEngine.AI; // We need this for NavMesh movement

public class TutorialEnemyController : BaseEnemy
{
    private NavMeshAgent agent;
    private Transform player;

    protected override void InitializeEnemy()
    {
        // Setup the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        agent.speed = stats.moveSpeed;

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
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Tell the NavMesh to walk toward the player
        agent.SetDestination(player.position);

        // If close enough, stop and attack
        if (distance <= stats.attackRadius)
        {
            currentState = EnemyState.Attacking;
            StartAttack();
        }
    }

    private void StartAttack()
    {
        Debug.Log("Enemy is Attacking!");

        PlayerProperties playerComp = player.gameObject.GetComponent<PlayerProperties>();

        if (playerComp != null)
        {
            // 1. Create the package
            DamageData data = new DamageData
            {
                damageAmount = 20f, // Your damage value
                origin = transform.position,
                knockbackForce = 5f // Add some kick to it!
            };

            // 2. Send the package (Now the arguments match!)
            playerComp.TakeDamage(data);
        }

        // After attacking, wait for cooldown
        Invoke("ResetFromAttack", stats.attackCooldown);
    }

    private void ResetFromAttack()
    {
        if (currentState != EnemyState.Dead)
            currentState = EnemyState.Idle;
    }
}