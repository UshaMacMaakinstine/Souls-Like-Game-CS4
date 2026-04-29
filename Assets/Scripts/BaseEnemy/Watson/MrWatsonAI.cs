using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
        // Pointer stick if Phase 2, Stomp if Phase 1
        string animKey = (controller.currentState == BossState.Phase1) ? "MeleeAttack" : "PointerAttack";
        controller.anim.SetTrigger(animKey);
        yield return new WaitForSeconds(2f);
        EndAttack();
    }

    IEnumerator ChargeSequence()
    {
        isAttacking = true;
        controller.anim.SetTrigger("ChargeStart");
        yield return new WaitForSeconds(0.6f);

        agent.isStopped = false;
        agent.speed = 25f;
        
        float timer = 0;
        while(timer < 1.5f && Vector3.Distance(transform.position, playerTransform.position) > 3.5f)
        {
            agent.SetDestination(playerTransform.position);
            timer += Time.deltaTime;
            yield return null;
        }

        agent.isStopped = true;
        agent.speed = 3.5f;
        controller.anim.SetTrigger("MeleeAttack"); // Smash after charge
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