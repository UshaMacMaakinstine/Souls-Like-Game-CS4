using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

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

    public GameObject pointerStick;

    private float lastAttackTime;
    public bool isAttacking = false;
    public bool sweeping;
    public bool swipe;
    [SerializeField] private GameObject spawnBox;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject micPrefab;
    public GameObject shockwave;

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
            FacePlayerSmoothly(gameObject);
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

        switch(controller.currentState)
        {
            case BossState.Phase1:
                if (dist <= meleeRadius) StartCoroutine(Phase1MeleeSequence());
                else if (dist <= chargeRadius) StartCoroutine(Phase1ChargeSequence());
                else StartCoroutine(Phase1RangedSequence());
                break;
            case BossState.Teacher:
                if (dist <= meleeRadius) StartCoroutine(TeacherMeleeSequence());
                else if (dist <= chargeRadius) StartCoroutine(TeacherChargeSequence());
                else StartCoroutine(TeacherRangedSequence());
                break;
            case BossState.Karaoke:
                if (dist <= meleeRadius) StartCoroutine(KaraokeMeleeSequence());
                else if (dist <= chargeRadius) StartCoroutine(KaraokeChargeSequence());
                else StartCoroutine(KaraokeRangedSequence());
                break;
            
        }
    }

    IEnumerator Phase1MeleeSequence()
    {
        isAttacking = true;
        float number = Random.value;

        if(number < 0.33f)
        {
            // 1. Get the target position (where the player is right now)
            Vector3 jumpTarget = playerTransform.position;
            Vector3 startPos = transform.position;

            controller.anim.SetTrigger("NearAttack");
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
        else if(0.33f < number && number < 0.66f)
        {
            sweeping = true;
            controller.anim.SetTrigger("Sweep");
            yield return new WaitForSeconds(3f);
            sweeping = false;
        }
        else
        {
            sweeping = true;
            controller.anim.SetTrigger("Kick");
            yield return new WaitForSeconds(1f);
            sweeping = false;
        }

        EndAttack();
    }

    IEnumerator Phase1ChargeSequence()
    {
        if(Random.value < 0.5f)
        {
            isAttacking = true;
            controller.anim.SetTrigger("ChargeStart");
            yield return new WaitForSeconds(0.6f);

            agent.isStopped = false;
            agent.speed = 25f;
            Vector3 dirToPlayer;
            Vector3 leftDirection;
            Vector3 chargeTarget;

            if(controller.currentState == BossState.Phase1)
            {
                // 1. CALCULATE THE "LEFT" POSITION
                // Direction from Watson to Player
                dirToPlayer = (playerTransform.position - transform.position).normalized;
                
                // The vector pointing 90 degrees to the left of that direction
                leftDirection = Quaternion.Euler(0, 90, 0) * dirToPlayer;
                
                // Target is: Player Position + (Left Direction * 20 feet)
                chargeTarget = playerTransform.position + (leftDirection * 8f);
            }
            else
            {
                chargeTarget = playerTransform.position;
            }

            // Charge toward the calculated offset point
            while(Vector3.Distance(transform.position, chargeTarget) > 25f)
            {
                agent.SetDestination(chargeTarget);
                yield return null;
            }

            // 2. SMASH AFTER CHARGE
            agent.isStopped = true;
            agent.speed = 3.5f;
            controller.anim.SetTrigger("MeleeAttack");
            yield return new WaitForSeconds(1.5f);
            EndAttack();
        }
        else
        {
            StartCoroutine(Phase1RangedSequence());
        }
    }

    IEnumerator Phase1RangedSequence()
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
    IEnumerator TeacherMeleeSequence()
    {
        isAttacking = true;
        
        if(Random.value < 0.5f)
        {
            sweeping = true;
            controller.anim.SetTrigger("Sweep");
            yield return new WaitForSeconds(3f);
            sweeping = false;
        }
        else
        {
            sweeping = true;
            controller.anim.SetTrigger("Sweep");
            yield return new WaitForSeconds(3f);
            sweeping = false;
            controller.anim.SetTrigger("MeleeAttack");
            yield return new WaitForSeconds(1.5f);
        }

        EndAttack();
    }

    IEnumerator TeacherChargeSequence()
    {
        isAttacking = true;

        if(Random.value < 0.5f)
        {
            controller.anim.SetTrigger("MeleeAttack");
            yield return new WaitForSeconds(1.5f);
        }
        else
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

        EndAttack();
    }

    IEnumerator TeacherRangedSequence()
    {
        isAttacking = true;

        if(Random.value < 0.8f)
        {
            // Rotate sideways for the Finger Gun animation
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 90, 0);

            controller.anim.SetTrigger("AtTeTeTe");
            yield return new WaitForSeconds(2.5f);
        }
        else
        {
            StartCoroutine(PointerFling());
        }
        EndAttack();
    }

    IEnumerator PointerFling()
    {
        controller.pointerStickProp.SetActive(false);
        controller.anim.SetTrigger("Summon");

        // List to keep track of pointers created in this specific attack wave
        List<WatsonProjectile> spawnedPointers = new List<WatsonProjectile>();

        float elapsed = 0f;
        while(elapsed < 2f)
        {
            float randomX = Random.Range(-40f, 40f);
            float randomY = Random.Range(28f, 68f);
            
            Vector3 localSpawnPosition = new Vector3(randomX, randomY, 0f);
            Vector3 worldSpawnPosition = spawnBox.transform.TransformPoint(localSpawnPosition);

            if(Random.value < 0.15f)
            {
                GameObject go = Instantiate(pointerStick, worldSpawnPosition, Quaternion.identity);
                
                go.transform.SetParent(transform);

                // Try to get the script and add it to our "waiting room" list
                if(go.TryGetComponent(out WatsonProjectile script))
                {
                    spawnedPointers.Add(script);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- THE 2 SECONDS ARE UP! ---
        
        swipe = true; // For boss logic/animations

        foreach(WatsonProjectile p in spawnedPointers)
        {
            if(p != null) 
            {
                p.Launch();
                
                // Adjust this value to change how fast they fire one after another
                // 0.05f is a rapid fire, 0.2f is more rhythmic
                yield return new WaitForSeconds(0.1f); 
            }
        }

        // Wait for the last projectile to finish before ending the attack state
        yield return new WaitForSeconds(1f);
    }
    IEnumerator KaraokeMeleeSequence()
    {
        isAttacking = true;
        
        if(Random.value < 0.5)
        {
            sweeping = true;
            controller.anim.SetTrigger("Sweep");
            yield return new WaitForSeconds(3f);
            sweeping = false;
        }
        else
        {
            controller.anim.SetTrigger("StepBack");
            // 1. Calculate the direction (Opposite of where the boss is looking)
            Vector3 backstepDir = -transform.forward;
            
            // 2. Calculate the target position
            Vector3 targetPos = transform.position + (backstepDir * 20f);

            float elapsed = 0;
            float duration = 20f / agent.speed;

            while (elapsed < duration)
            {
                // agent.Move moves the boss while keeping them pinned to the NavMesh
                agent.Move(backstepDir * agent.speed * Time.deltaTime);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            // Rotate sideways for the Finger Gun animation
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 90, 0);

            controller.anim.SetTrigger("AtTeTeTe");
            yield return new WaitForSeconds(2.5f);
        }

        EndAttack();
    }

    IEnumerator KaraokeChargeSequence()
    {
        isAttacking = true;

        float number = Random.value;

        if(number < 0.33f)
        {
            controller.anim.SetTrigger("Throw");

            // Wait for the specific frame in the animation where he "releases" the mic
            yield return new WaitForSeconds(0.5f);

            // 1. Reference the boss's hand-held mic
            GameObject handMic = controller.micProp;
            handMic.SetActive(false); // Hide the one he is holding

            // 2. Spawn the violent version at the hand's position
            GameObject projectile = Instantiate(micPrefab, controller.micProp.transform.position, controller.micProp.transform.rotation);
            controller.micProp.SetActive(false); // Hide hand mic

            if (projectile.TryGetComponent(out WatsonProjectile script))
            {
                script.LaunchWithReturn(controller.micProp.transform); // Calls Mode 2
            }

            controller.anim.SetTrigger("Return");

            yield return new WaitForSeconds(2.5f); // Duration of the throw animation
        }
        else if(0.33f < number && number < 0.66f)
        {
            // Rotate sideways for the Finger Gun animation
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 90, 0);

            controller.anim.SetTrigger("AtTeTeTe");
            yield return new WaitForSeconds(2.5f);
        }
        else
        {
            controller.anim.SetTrigger("Sing");
            float elapsed = 0f;
            while(elapsed < 15f)
            {
                if(Random.value < 0.15f)
                {
                    Instantiate(shockwave, transform.position, Quaternion.identity);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        EndAttack();
    }

    IEnumerator KaraokeRangedSequence()
    {
        isAttacking = true;
        if(Random.value < 0.5f)
        {
            // Rotate sideways for the Finger Gun animation
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 90, 0);

            controller.anim.SetTrigger("AtTeTeTe");
            yield return new WaitForSeconds(2.5f);
        }
        else
        {
            controller.anim.SetTrigger("Sing");
            float elapsed = 0f;
            while(elapsed < 15f)
            {
                if(Random.value < 0.15f)
                {
                    Instantiate(shockwave, transform.position, Quaternion.identity);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
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
            if (player.isAttacking || player.GetComponent<ThirdPersonController>().isMoving)
            {
                player.GetComponent<PlayerProperties>().TakeDamage(new DamageData { damageAmount = 250f });
                break;
            }
            timer -= Time.deltaTime;
            yield return null;
        }
        EndAttack();
    }

    void FacePlayerSmoothly(GameObject thing)
    {
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            thing.transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 4f);
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