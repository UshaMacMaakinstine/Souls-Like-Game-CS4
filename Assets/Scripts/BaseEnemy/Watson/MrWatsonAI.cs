using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;

public enum BossState 
{ 
    Phase1, 
    Phase2,
    Transitioning, 
    Teacher, 
    Violin, 
    Karaoke 
}

public class MrWatsonAI : MonoBehaviour
{
    private MrWatsonController controller;
    private NavMeshAgent agent;
    public Transform playerTransform;

    [Header("Combat Radius")]
    public float meleeRadius = 7f;
    public float chargeRadius = 22f;
    public float attackCooldown = 3f;

    private float lastAttackTime;
    private bool isAttacking = false;

    void Start()
    {
        controller = GetComponent<MrWatsonController>();
        agent = GetComponent<NavMeshAgent>();
        agent.isStopped = true;
    }

    void Update()
    {
        animationSpeedAdjustment();
        if (controller.isDown || controller.currentState == BossState.Transitioning) return;

        // CALCULATE TURN SPEED
        // This compares his current rotation to where he wants to look
        float angle = Vector3.SignedAngle(transform.forward, agent.desiredVelocity, Vector3.up);
        float normalizedTurnSpeed = Mathf.Clamp(angle / 45f, -1f, 1f); // -1 is hard left, 1 is hard right

        // SYNC ANIMATOR
        // agent.velocity.magnitude gives the actual movement speed
        controller.anim.SetFloat("Speed", agent.velocity.magnitude);
        controller.anim.SetFloat("TurnSpeed", normalizedTurnSpeed);

        if (!isAttacking)
        {
            FacePlayerSmoothly();
            float dist = Vector3.Distance(transform.position, playerTransform.position);

            if (Time.time > lastAttackTime + attackCooldown)
                DecideAttack(dist);
        }
    }

    void DecideAttack(float dist)
    {
        // Gimmick: Paws Up (Phase 2+)
        if (controller.currentState != BossState.Phase1 && Random.value < 0.15f)
        {
            StartCoroutine(PawsUpRoutine());
            return;
        }

        if (dist <= meleeRadius) StartCoroutine(MeleeSequence());
        else if (dist <= chargeRadius) StartCoroutine(ChargeSequence());
        else StartCoroutine(RangedSequence());
    }

    IEnumerator MeleeSequence()
    {
        isAttacking = true;
        
        // 1. Get the target position (where the player is right now)
        Vector3 jumpTarget = playerTransform.position;
        Vector3 startPos = transform.position;

        // 2. Trigger the animation
        string animKey = (controller.currentState == BossState.Phase1) ? "NearAttack" : "PointerAttack";
        controller.anim.SetTrigger(animKey);

        // If it's the jump attack, handle the manual movement
        if (animKey == "NearAttack")
        {
            agent.enabled = false; // Disable NavMesh so we can move vertically

            float jumpDuration = 3.167f; 
            float jumpHeight = 12f; // Adjust based on how high the animation looks
            float timer = 0;

            while (timer < jumpDuration)
            {
                timer += Time.deltaTime;
                float t = timer / jumpDuration; // 0 to 1

                // Parabola math for the arc
                float height = 4 * jumpHeight * t * (1 - t);

                // Move horizontally and vertically
                Vector3 currentPos = Vector3.Lerp(startPos, jumpTarget, t);
                transform.position = new Vector3(currentPos.x, startPos.y + height, currentPos.z);

                yield return null;
            }

            transform.position = jumpTarget; // Ensure clean landing
            agent.enabled = true; // Turn NavMesh back on
        }
        else
        {
            // If it's just the PointerAttack, just wait for the animation
            yield return new WaitForSeconds(2f);
        }

        EndAttack();
    }

    IEnumerator ChargeSequence()
    {
        isAttacking = true;
        controller.anim.SetTrigger("ChargeStart");
        yield return new WaitForSeconds(0.6f);

        agent.isStopped = false;
        agent.speed = 25f;

        // 1. CALCULATE THE "LEFT" POSITION
        // Direction from Watson to Player
        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
        
        // The vector pointing 90 degrees to the left of that direction
        Vector3 leftDirection = Quaternion.Euler(0, 90, 0) * dirToPlayer;
        
        // Target is: Player Position + (Left Direction * 20 feet)
        Vector3 chargeTarget = playerTransform.position + (leftDirection * 6f);

        float timer = 0;
        // Charge toward the calculated offset point
        while(timer < 1.5f && Vector3.Distance(transform.position, chargeTarget) > 2f)
        {
            agent.SetDestination(chargeTarget);
            timer += Time.deltaTime;
            yield return null;
        }

        // 2. SMASH AFTER CHARGE
        agent.isStopped = true;
        agent.speed = 3.5f;
        controller.anim.SetTrigger("MeleeAttack"); 
        yield return new WaitForSeconds(1.5f);
        EndAttack();
    }

    IEnumerator RangedSequence()
    {
        isAttacking = true;
        // Rotate sideways for the Finger Gun animation
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 90, 0);

        controller.anim.SetTrigger("AtTeTeTe");
        yield return new WaitForSeconds(2.5f);
        EndAttack();
    }

    IEnumerator PawsUpRoutine()
    {
        isAttacking = true;
        controller.anim.SetTrigger("PawsUp");
        yield return new WaitForSeconds(1f); // Warning period

        float timer = 2f;
        ThirdPersonController player = playerTransform.GetComponent<ThirdPersonController>();
        
        while(timer > 0)
        {
            // If player moves or attacks during Paws Up
            if (player.isAttacking || player.GetComponent<CharacterController>().velocity.magnitude > 0.1f)
            {
                player.GetComponent<PlayerProperties>().TakeDamage(new DamageData { damageAmount = 25f });
                break;
            }
            timer -= Time.deltaTime;
            yield return null;
        }
        EndAttack();
    }

    void FacePlayerSmoothly()
    {
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 4f);
        }
    }

    void EndAttack() { isAttacking = false; lastAttackTime = Time.time; }

    void animationSpeedAdjustment()
    {

        // Reset to normal speed if not attacking
        if (!isAttacking) 
        {
            controller.anim.speed = 1f;
            return;
        }

        controller.anim.speed = 0.5f;
        return;
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 1. Melee Radius (Red) - The "Danger Zone"
        Gizmos.color = Color.red;
        DrawWireDisk(transform.position, meleeRadius);

        // 2. Charge Radius (Yellow) - The "Hunt Zone"
        Gizmos.color = Color.blue;
        DrawWireDisk(transform.position, chargeRadius);

        // 3. Current Target Line (Cyan)
        if (playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up, playerTransform.position + Vector3.up);
            
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Dist to Player: " + dist.ToString("F1"));
        }
    }

    // Helper to draw a flat circle on the ground
    private void DrawWireDisk(Vector3 center, float radius)
    {
        float angleStep = 10f;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        for (float i = angleStep; i <= 360f; i += angleStep)
        {
            float rad = i * Mathf.Deg2Rad;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(rad) * radius, 0, Mathf.Sin(rad) * radius);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
#endif
}