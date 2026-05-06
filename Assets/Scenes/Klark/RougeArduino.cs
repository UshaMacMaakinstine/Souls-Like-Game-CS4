using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class RougeArduino : BaseEnemy
{
    [Header("References")]
    public Transform topSensor;                  // rotating distance sensor
    public GameObject fanWeapon;                 // visual/melee fan (enable on phase 2)
    public EnemyHitbox chainsawHitbox;           // hitbox for chainsaw melee
    public MeshRenderer bodyRenderer;

    [Header("Player / Nav")]
    public Transform playerTransform;
    private NavMeshAgent agent;

    [Header("Phase 1 - Stationary (spinning sensor + dash)")]
    public float sensorSpinSpeed = 180f;
    public float detectRange = 10f;
    public float dashSpeed = 18f;
    public float dashDuration = 0.5f;
    public float dashHitRadius = 1.5f;
    public float dashDamage = 18f;
    public float dashCooldown = 2f;

    [Header("Phase 2 - Mobile (fan/chainsaw)")]
    public bool isAwakened = false;
    public float moveSpeedPhase2 = 3.5f;
    public float chainsawDamage = 3f;            // per hit window
    public float chainsawHitInterval = 0.18f;
    public float fanEffectRadius = 3.0f;         // push/pull radius
    public float fanForce = 6f;                  // push magnitude

    // internal
    private bool canDash = true;
    private bool isDashing = false;
    private bool isAttacking = false;
    private Color originalColor;

    protected override void InitializeEnemy()
    {
        // Prefer TryGetComponent to avoid unexpected GetComponent cost / nulls
        if (!TryGetComponent(out agent))
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = stats != null ? stats.moveSpeed : 3.5f;
            agent.stoppingDistance = 0.5f;
        }

        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (bodyRenderer != null) originalColor = bodyRenderer.material.color;
        if (chainsawHitbox != null && chainsawHitbox.gameObject != null) chainsawHitbox.gameObject.SetActive(false);
        if (fanWeapon != null) fanWeapon.SetActive(false);
    }

    protected override void HandleStateMachine()
    {
        if (currentState == EnemyState.Dead || isDashing) return;

        // Phase-specific behavior
        if (!isAwakened)
        {
            // Stationary: spin sensor
            if (topSensor != null)
                topSensor.Rotate(Vector3.up, sensorSpinSpeed * Time.deltaTime, Space.Self);

            // Detect player and dash
            if (playerTransform != null && canDash)
            {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                if (dist <= detectRange)
                {
                    StartCoroutine(DashAtPlayer());
                }
            }
        }
        else
        {
            // Phase 2: mobile. sensor tracks player
            if (topSensor != null && playerTransform != null)
            {
                Vector3 lookPos = playerTransform.position;
                lookPos.y = topSensor.position.y;
                topSensor.transform.rotation = Quaternion.Slerp(
                    topSensor.transform.rotation,
                    Quaternion.LookRotation((lookPos - topSensor.position).normalized),
                    Time.deltaTime * 6f);
            }

            // chasing behavior using NavMeshAgent
            if (playerTransform != null && agent != null && !isAttacking)
            {
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);

                float dist = Vector3.Distance(transform.position, playerTransform.position);
                // Evaluate attack radius first then compare
                float attackRadius = (stats != null) ? stats.attackRadius : 2.0f;
                if (dist <= attackRadius)
                {
                    // start continuous chainsaw attack
                    StartCoroutine(ChainsawRoutine());
                }
            }
        }
    }

    // Simple external trigger to awaken phase 2 (call from another script or on health threshold)
    public void AwakenPhase2()
    {
        if (isAwakened) return;
        isAwakened = true;
        if (fanWeapon != null) fanWeapon.SetActive(true);
        if (chainsawHitbox != null && chainsawHitbox.gameObject != null) chainsawHitbox.gameObject.SetActive(false); // will enable in routine
        if (agent != null) agent.speed = moveSpeedPhase2;
        if (bodyRenderer != null) bodyRenderer.material.color = Color.red;
    }

    private IEnumerator DashAtPlayer()
    {
        if (playerTransform == null || agent == null || !canDash) yield break;

        canDash = false;
        isDashing = true;
        agent.isStopped = true;

        Vector3 target = playerTransform.position;
        target.y = transform.position.y;

        float elapsed = 0f;
        float duration = Mathf.Max(0.001f, dashDuration);

        // face player quickly
        if (playerTransform != null)
        {
            Vector3 dir = (playerTransform.position - transform.position);
            dir.y = 0f;
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        // Use MoveTowards for stable, frame-rate independent dash movement
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, Mathf.Max(0f, dashSpeed) * Time.deltaTime);
            yield return null;
        }

        // On dash end, check proximity and apply damage
        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= dashHitRadius)
            {
                PlayerProperties pp = playerTransform.GetComponent<PlayerProperties>();
                if (pp != null)
                {
                    DamageData data = new DamageData { damageAmount = dashDamage, origin = transform.position, knockbackForce = dashForceOrDefault() };
                    pp.TakeDamage(data);
                }

                // try to push with rigidbody if present
                Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 push = (playerTransform.position - transform.position).normalized * dashForceOrDefault();
                    rb.AddForce(push, ForceMode.Impulse);
                }
            }
        }

        isDashing = false;
        if (agent != null) agent.isStopped = false;

        // small cooldown
        yield return new WaitForSeconds(Mathf.Max(0f, dashCooldown));
        canDash = true;
    }

    // Chainsaw routine: telegraph briefly, enable hitbox rapidly until player leaves
    private IEnumerator ChainsawRoutine()
    {
        if (chainsawHitbox == null || isAttacking) yield break;

        isAttacking = true;
        if (agent != null) agent.isStopped = true;

        // Telegraphed color
        if (bodyRenderer != null) bodyRenderer.material.color = Color.yellow;
        // spin fan visually if available
        float tele = 0.25f;
        float timer = 0f;
        while (timer < tele)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Start chainsaw active window: repeatedly apply damage via hitbox enabling
        if (chainsawHitbox != null && chainsawHitbox.gameObject != null)
        {
            chainsawHitbox.damage = chainsawDamage;
            chainsawHitbox.gameObject.SetActive(true);
        }

        float lastHit = 0f;
        while (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) <= fanEffectRadius * 1.5f)
        {
            // apply fan force to player (push away)
            ApplyFanEffect();

            // keep time for hit interval if needed by future logic
            lastHit += Time.deltaTime;
            if (lastHit >= chainsawHitInterval)
            {
                lastHit = 0f;
            }
            yield return null;
        }

        if (chainsawHitbox != null && chainsawHitbox.gameObject != null) chainsawHitbox.gameObject.SetActive(false);
        if (bodyRenderer != null) bodyRenderer.material.color = originalColor;
        isAttacking = false;
        if (agent != null) agent.isStopped = false;
    }

    private void ApplyFanEffect()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > fanEffectRadius) return;

        // Direction away from the fan (push) � you can invert to drag closer if desired
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(dir * fanForce * Time.deltaTime, ForceMode.VelocityChange);
        }
        else
        {
            // If player uses a CharacterController/other, we attempt to apply a small damage with knockback
            PlayerProperties pp = playerTransform.GetComponent<PlayerProperties>();
            if (pp != null)
            {
                DamageData data = new DamageData { damageAmount = 0f, origin = transform.position, knockbackForce = fanForce * 0.25f };
                pp.TakeDamage(data); // no health effect but class may later read knockbackForce
            }
        }
    }

    private float dashForceOrDefault()
    {
        // try to use dashHitRadius as proxy; keep value sensible
        return Mathf.Max(3f, dashHitRadius * 2f);
    }

    protected override void Die()
    {
        StopAllCoroutines();
        base.Die();
        if (chainsawHitbox != null && chainsawHitbox.gameObject != null) chainsawHitbox.gameObject.SetActive(false);
        if (fanWeapon != null) fanWeapon.SetActive(false);
        if (bodyRenderer != null) bodyRenderer.material.color = Color.gray;
    }

    private void OnDisable()
    {
        // Ensure coroutines and visual states are cleaned up on disable/destroy
        StopAllCoroutines();
        if (chainsawHitbox != null && chainsawHitbox.gameObject != null) chainsawHitbox.gameObject.SetActive(false);
        if (fanWeapon != null) fanWeapon.SetActive(false);
        if (bodyRenderer != null) bodyRenderer.material.color = originalColor;
    }

    private void OnValidate()
    {
        // Clamp public values to reasonable ranges to avoid runtime surprises set in inspector
        detectRange = Mathf.Max(0f, detectRange);
        dashSpeed = Mathf.Max(0f, dashSpeed);
        dashDuration = Mathf.Max(0.01f, dashDuration);
        dashHitRadius = Mathf.Max(0f, dashHitRadius);
        dashCooldown = Mathf.Max(0f, dashCooldown);
        moveSpeedPhase2 = Mathf.Max(0f, moveSpeedPhase2);
        chainsawHitInterval = Mathf.Max(0.01f, chainsawHitInterval);
        fanEffectRadius = Mathf.Max(0f, fanEffectRadius);
        fanForce = Mathf.Max(0f, fanForce);
    }

    // Optional: debug gizmos to visualize ranges
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fanEffectRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashHitRadius);
    }
<<<<<<< HEAD
}
=======
}
*/
>>>>>>> 9994ce647c3ef707a5013270a5d2bd2a0bebdd86
