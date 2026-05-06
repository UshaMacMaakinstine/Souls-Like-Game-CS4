using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class RatController : BaseEnemy
{
    [Header("Rat Specific Setup")]
    public EnemyHitbox biteHitbox;
    // Changed to Renderer to be flexible with SkinnedMeshRenderers on children
    public Renderer ratRenderer;

    [Header("Visual Feedback")]
    public Color telegraphColor = Color.yellow;
    public float telegraphDuration = 0.5f;
    public float biteActiveDuration = 0.2f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Color originalColor;
    private bool isAttacking = false;
    public Animator anim;
    private bool isMoving;

    protected override void InitializeEnemy()
    {
        agent = GetComponent<NavMeshAgent>();

        // Find the renderer on the child 'meshes[0]' if not assigned in Inspector
        if (ratRenderer == null)
        {
            Transform meshChild = transform.Find("meshes[0]");
            if (meshChild != null)
            {
                ratRenderer = meshChild.GetComponent<Renderer>();
            }
            else
            {
                // Fallback: search all children if "meshes[0]" name is missing
                ratRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            }
        }

        if (stats != null)
        {
            agent.speed = stats.moveSpeed;
            agent.stoppingDistance = stats.attackRadius - 0.2f;
            currentHealth = stats.maxHealth;
        }

        if (ratRenderer != null)
        {
            originalColor = ratRenderer.material.color;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        if (biteHitbox != null) biteHitbox.SetActive(false);
    }

    private void Update()
    {
        // FIX: If the rat is idle, he needs to check if the player is close enough to start chasing
        if (currentState == EnemyState.Idle && !isAttacking)
        {
            CheckForPlayer();
        }

        HandleStateMachine();
        UpdateAnimator();
    }

    protected override void HandleStateMachine()
    {
        // Don't let the base state machine override us if we are mid-attack or dead
        if (isAttacking || currentState == EnemyState.Dead) return;

        base.HandleStateMachine();

        // Keep looking at the player if they are within a reasonable distance
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= stats.attackRadius + 1.5f)
            {
                LookAtPlayer();
            }
        }
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
        if (playerTransform == null || isAttacking || currentState == EnemyState.Dead)
        {
            isMoving = false;
            return;
        }

        isMoving = true;
        agent.isStopped = false;
        agent.SetDestination(playerTransform.position);

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Transition to attack if close enough
        if (distance <= (stats.attackRadius + 0.5f) && !isAttacking)
        {
            StartCoroutine(AttackSequence());
        }
    }

    private IEnumerator AttackSequence()
    {
        isMoving = false;
        isAttacking = true;
        currentState = EnemyState.Attacking;
        agent.isStopped = true;

        // Visual telegraph (turning yellow)
        if (ratRenderer != null) ratRenderer.material.color = telegraphColor;

        anim.SetTrigger("IsAttacking");
        float timer = 0;
        while (timer < telegraphDuration)
        {
            LookAtPlayer();
            timer += Time.deltaTime;
            yield return null;
        }

        // Deal Damage
        if (currentState != EnemyState.Dead)
        {
            if (biteHitbox != null) biteHitbox.SetActive(true);
            yield return new WaitForSeconds(biteActiveDuration);
            if (biteHitbox != null) biteHitbox.SetActive(false);
        }

        // Reset visual
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

        if (biteHitbox != null) biteHitbox.SetActive(false);
        if (ratRenderer != null) ratRenderer.material.color = Color.gray;
    }

    void UpdateAnimator()
    {
        if (anim != null)
        {
            // Update the animator based on whether the NavMeshAgent is actually moving
            bool moving = agent.velocity.magnitude > 0.1f && !agent.isStopped;
            anim.SetBool("IsMoving", moving);
        }
    }
}