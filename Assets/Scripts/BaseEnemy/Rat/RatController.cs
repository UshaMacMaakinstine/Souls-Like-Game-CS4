using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class RatController : BaseEnemy
{
    [Header("Rat Specific Setup")]
    public EnemyHitbox biteHitbox;
    public MeshRenderer ratRenderer;

    [Header("Visual Feedback")]
    public Color telegraphColor = Color.red;
    public float telegraphDuration = 0.5f;
    public float biteActiveDuration = 0.2f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Color originalColor;
    private bool isAttacking = false;

    protected override void InitializeEnemy()
    {
        agent = GetComponent<NavMeshAgent>();

        if (stats != null)
        {
            agent.speed = stats.moveSpeed;
            agent.stoppingDistance = stats.attackRadius - 0.5f;
            currentHealth = stats.maxHealth;
        }

        if (ratRenderer != null)
            originalColor = ratRenderer.material.color;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        if (biteHitbox != null) biteHitbox.SetActive(false);
    }

    public override void CheckForPlayer()
    {
        if (playerTransform == null || isAttacking || currentState == EnemyState.Dead) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= stats.aggroRadius)
        {
            currentState = EnemyState.Chasing;
        }
    }

    public override void MoveToPlayer()
    {
        if (playerTransform == null || currentState == EnemyState.Attacking || currentState == EnemyState.Dead) return;

        agent.isStopped = false;
        agent.SetDestination(playerTransform.position);

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= stats.attackRadius && !isAttacking)
        {
            StartCoroutine(AttackSequence());
        }
    }

    private IEnumerator AttackSequence()
    {
        isAttacking = true;
        currentState = EnemyState.Attacking;
        agent.isStopped = true;

        if (ratRenderer != null) ratRenderer.material.color = telegraphColor;

        float timer = 0;
        while (timer < telegraphDuration)
        {
            LookAtPlayer();
            timer += Time.deltaTime;
            yield return null;
        }

        if (currentState != EnemyState.Dead)
        {
            if (biteHitbox != null) biteHitbox.SetActive(true);
            yield return new WaitForSeconds(biteActiveDuration);
            if (biteHitbox != null) biteHitbox.SetActive(false);
        }

        if (ratRenderer != null) ratRenderer.material.color = originalColor;
        yield return new WaitForSeconds(stats.attackCooldown);

        ResetFromAttack();
    }

    private void LookAtPlayer()
    {
        if (playerTransform == null) return;
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

    private void ResetFromAttack()
    {
        if (currentState == EnemyState.Dead) return;
        isAttacking = false;
        agent.isStopped = false;
        currentState = EnemyState.Idle;
    }

    protected override void Die()
    {
        StopAllCoroutines();
        base.Die();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (biteHitbox != null) biteHitbox.SetActive(false);
        if (ratRenderer != null) ratRenderer.material.color = Color.gray;
    }
}