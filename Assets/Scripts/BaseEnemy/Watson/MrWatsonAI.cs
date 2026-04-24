using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MrWatsonAI : MonoBehaviour
{
    private MrWatsonController controller;
    private NavMeshAgent agent;
    public Transform playerTransform;

    [Header("Combat Settings")]
    public float attackCooldown = 3f;
    public float chargeSpeed = 30f;
    private float lastAttackTime;
    private bool isAttacking = false;

    void Start()
    {
        controller = GetComponent<MrWatsonController>();
        agent = GetComponent<NavMeshAgent>();
        agent.isStopped = true; // Start stationary
    }

    void Update()
    {
        if (controller.isDown || controller.currentState == BossState.Transitioning) return;

        UpdateMovementAnimations();

        // ONLY rotate smoothly if we aren't currently mid-attack
        if (agent.isStopped && !isAttacking)
        {
            FacePlayerSmoothly();
        }

        if (Time.time > lastAttackTime + attackCooldown && !isAttacking)
        {
            DecideNextMove();
        }
    }

    void UpdateMovementAnimations()
    {
        // Forward Speed (Y-Axis in 2D Blend Tree)
        float speed = agent.velocity.magnitude;
        controller.anim.SetFloat("Speed", speed);

        // Turn Calculation (X-Axis in 2D Blend Tree)
        Vector3 targetDir = (playerTransform.position - transform.position).normalized;
        float angle = Vector3.SignedAngle(transform.forward, targetDir, Vector3.up);
        
        // Clamps turn between -1 (Left) and 1 (Right)
        float turnValue = Mathf.Clamp(angle / 45f, -1f, 1f);
        
        // Smoothly lerp the turn parameter so the animation transitions nicely
        float currentTurn = controller.anim.GetFloat("TurnSpeed");
        controller.anim.SetFloat("TurnSpeed", Mathf.Lerp(currentTurn, turnValue, Time.deltaTime * 5f));
    }

    void FacePlayerSmoothly()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);
        }
    }

    void DecideNextMove()
    {
        // Simple 50/50 chance for logic testing
        if (Random.value > 0.5f)
            StartCoroutine(MeleeChargeSequence());
        else
            ExecuteRanged();
    }

    IEnumerator MeleeChargeSequence()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        Vector3 targetPos = playerTransform.position;
        agent.isStopped = false;
        agent.speed = chargeSpeed;
        agent.SetDestination(targetPos);

        // FIX: Added "agent.enabled" check to the while loop
        while (agent.enabled && (agent.pathPending || agent.remainingDistance > 1.5f))
        {
            yield return null;
        }

        // Double check agent is still enabled before calling commands
        if (agent.enabled)
        {
            agent.isStopped = true;
            controller.anim.SetTrigger("MeleeAttack");
        }

        yield return new WaitForSeconds(1.5f);
        isAttacking = false;
    }

    void ExecuteRanged()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        agent.isStopped = true;

        // 1. Calculate direction to player
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0; // Keep him upright

        // 2. Create the rotation to face the player
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);

        // 3. APPLY THE SIDEWAYS OFFSET
        // If he is pointing to his RIGHT, rotate him -90 degrees.
        // If he is pointing to his LEFT, rotate him 90 degrees.
        Quaternion offset = Quaternion.Euler(0, 90, 0); 
        transform.rotation = lookRotation * offset;

        // 4. Trigger the animation
        controller.anim.SetTrigger("AtTeTeTe");

        StartCoroutine(ResetAttackAfterDelay(2.5f));
    }

    IEnumerator ResetAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
    }
}