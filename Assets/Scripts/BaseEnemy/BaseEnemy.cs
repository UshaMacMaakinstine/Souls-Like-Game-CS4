using UnityEngine;
using System.Collections;

public class BaseEnemy : MonoBehaviour
{
    public EnemyStats stats;
    protected float currentHealth;

    public enum EnemyState { Idle, Chasing, Attacking, Staggered, Dead }
    public EnemyState currentState = EnemyState.Idle;

    [Header("Death Settings")]
    public GameObject deathEffect;
    public float maxSinkSpeed = 0.05f;
    public float dissolveTime = 3.0f;
    public float postBlackSinkTime = 5.0f;

    [Header("Dynamic Particles")]
    public float maxEmissionBase = 100f;
    public float maxSizeMultiplier = 0.15f;
    public float particleLifetime = 6.0f; // Increased for "staying around longer"

    private void Start()
    {
        if (stats != null) currentHealth = stats.maxHealth;
        InitializeEnemy();
    }

    protected virtual void InitializeEnemy() { }

    private void Update()
    {
        if (currentState == EnemyState.Dead) return;
        HandleStateMachine();
    }

    protected virtual void HandleStateMachine()
    {
        // Since it's a Coliseum, we skip Idle and go straight to Chasing
        if (currentState == EnemyState.Idle) currentState = EnemyState.Chasing;

        switch (currentState)
        {
            case EnemyState.Chasing: MoveToPlayer(); break;
        }
    }

    public virtual void TakeDamage(float amount)
    {
        if (currentState == EnemyState.Dead) return;
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        if (currentState == EnemyState.Dead) return;
        currentState = EnemyState.Dead;

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        var charController = GetComponent<CharacterController>();
        if (charController != null) charController.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.linearVelocity = Vector3.zero; }

        if (deathEffect != null)
        {
            // We use Quaternion.identity to keep it world-aligned (Up is Up)
            GameObject fx = Instantiate(deathEffect, transform.position, Quaternion.identity);

            ParticleSystem fxPS = fx.GetComponent<ParticleSystem>();
            if (fxPS != null) StartCoroutine(DeathSequence(fxPS));
        }
        else
        {
            StartCoroutine(DeathSequence(null));
        }
    }

    private IEnumerator DeathSequence(ParticleSystem ps)
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        Color startColor = (rend != null) ? rend.material.color : Color.white;

        var emissionModule = (ps != null) ? ps.emission : default;
        var mainModule = (ps != null) ? ps.main : default;
        var shapeModule = (ps != null) ? ps.shape : default;

        float enemyScale = (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3f;

        // Force the shape to be a Box that matches the enemy's footprint
        if (ps != null) shapeModule.shapeType = ParticleSystemShapeType.Box;

        float elapsed = 0;
        while (elapsed < dissolveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dissolveTime;

            if (ps != null)
            {
                emissionModule.rateOverTime = t * (maxEmissionBase * enemyScale);
                mainModule.startSize = t * (enemyScale * maxSizeMultiplier);
                mainModule.startLifetime = particleLifetime; // Apply longer life

                // Keep the box matching the enemy's world scale
                shapeModule.scale = transform.localScale;
            }

            transform.Translate(Vector3.down * maxSinkSpeed * Time.deltaTime, Space.World);

            if (rend != null)
                rend.material.color = Color.Lerp(startColor, Color.black, t);

            yield return null;
        }

        if (rend != null) rend.material.color = Color.black;

        float postElapsed = 0;
        while (postElapsed < postBlackSinkTime)
        {
            postElapsed += Time.deltaTime;
            transform.Translate(Vector3.down * maxSinkSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        if (ps != null)
        {
            ps.Stop();
            // Cleanup time increased to allow long-lived particles to fade out
            Destroy(ps.gameObject, particleLifetime + 1f);
        }
        Destroy(gameObject);
    }

    public virtual void CheckForPlayer() { }
    public virtual void MoveToPlayer() { }
}