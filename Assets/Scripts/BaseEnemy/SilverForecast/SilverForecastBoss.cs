using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;
using Unity.Jobs;
using System.Collections.Generic;

public class SilverForecastBoss : BaseEnemy
{
    public enum BossPhase { Heavy, Light }
    public BossPhase currentPhase = BossPhase.Heavy;

    [Header("Phase Settings")]
    public float phaseTransitionThreshold = 0.5f;
    public float heavySpeed = 2.5f;
    public float lightSpeed = 6.5f;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;

    private NavMeshAgent agent;
    public Animator anim;
    private bool isTransitioning = false;
    private bool isAttacking = false;
    private Transform playerTransform;
    public GameObject javelin;
    public GameObject shockwaveIndecator;
    public GameObject icicle;

    protected override void InitializeEnemy()
    {
        // This runs at the end of the BaseEnemy Start()
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        SetupPhase(BossPhase.Heavy);
    }

    protected override void HandleStateMachine()
    {
        if (isTransitioning || isAttacking || currentState == EnemyState.Dead) return;

        // Follow the base logic for Chasing
        base.HandleStateMachine();

        // Add Boss-specific attack trigger
        if (currentPhase == BossPhase.Light && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                StartCoroutine(DecideLightAttack());
            }
        }
    }

    public override void MoveToPlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(playerTransform.position);

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Transition to attack if close enough
        if (distance <= (stats.attackRadius + 0.5f) && !isAttacking)
        {
            AttackSequence();
        }
    }

    void AttackSequence()
    {
        if(currentPhase == BossPhase.Heavy)
            StartCoroutine(DecideLightAttack());
    }

    IEnumerator DecideLightAttack()
    {
        isAttacking = true;
        currentState = EnemyState.Attacking;
        agent.isStopped = true;

        StartCoroutine(GaleForceRepel());

        // float rand = Random.value;
        // if (rand > 0.7f)
        //     yield return StartCoroutine(AtmosphericErasure());
        // else
        //     yield return StartCoroutine(RapidStabAttack(5));

        yield return new WaitForSeconds(1f);

        agent.isStopped = false;
        isAttacking = false;
        currentState = EnemyState.Chasing;
    }

    public IEnumerator RapidStabAttack(int stabCount)
    {
        for (int i = 0; i < stabCount; i++)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y + 90, transform.rotation.z);
            if (anim != null) anim.SetTrigger("AttackAgain");
            yield return new WaitForSeconds(1.15f);
        }
    }

    public IEnumerator PressureJavilen()
    {
        anim.SetTrigger("Fling");
        GameObject throwable = Instantiate(javelin, new Vector3(transform.position.x, transform.position.y + 10, transform.position.z), Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z));

        FacePlayerSmoothly(throwable);

        Renderer objRenderer = throwable.GetComponent<Renderer>();
        // Get the current color
        Color color = objRenderer.material.color;
        float startAlpha = 0f;

        for (float t = 0f; t < 3f; t += Time.deltaTime)
        {
            float normalizedTime = t / 3f;
            // Smoothly interpolate the alpha
            color.a = Mathf.Lerp(startAlpha, 1f, normalizedTime);
            objRenderer.material.color = color;
            yield return null;
        }

        // Ensure we hit the exact target at the end
        color.a = 1f;
        objRenderer.material.color = color;

        FacePlayerSmoothly(throwable);

        throwable.GetComponent<WatsonProjectile>().Launch();
    }

    public IEnumerator AtmosphericErasure()
    {
        if (anim != null) anim.SetTrigger("SummonFog");
        yield return new WaitForSeconds(2.46f);

        Vector3 dashPos = playerTransform.position + (playerTransform.forward * -1.5f);
        transform.position = dashPos;

        if (anim != null) anim.SetTrigger("DashStab");
        yield return new WaitForSeconds(1.5f);
    }

    public IEnumerator GaleForceRepel()
    {
        anim.SetTrigger("Dance");
        shockwaveIndecator.SetActive(true);
        yield return new WaitForSeconds(3f);
        if(Vector3.Distance(playerTransform.position, transform.position) < 5f)
        {
            playerTransform.gameObject.GetComponent<PlayerProperties>().TakeDamage(new DamageData { damageAmount = 25f });
            playerTransform.gameObject.GetComponent<ThirdPersonController>().ApplyKnockback(transform.position, 100f);
        }
    }

    public IEnumerator HailstormValley()
    {
        anim.SetTrigger("Ice");
        List<WatsonProjectile> spawnedPointers = new List<WatsonProjectile>();
        for (int amount = 0; amount > 4; amount++)
        {
            GameObject go = Instantiate(icicle, new Vector3(playerTransform.position.x + Random.Range(-10f, 10f), playerTransform.position.y + 10f, playerTransform.position.z + Random.Range(-10f, 10f)), Quaternion.identity);
            spawnedPointers.Add(go.GetComponent<WatsonProjectile>());
        }

        yield return new WaitForSeconds(1.5f);

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
    }

    public IEnumerator VortexSpin()
    {
        anim.SetBool("spin", true);
        yield return new WaitForSeconds(1f);
        for(float t = 0f; t < 50f; t+=Time.deltaTime)
        {
            float rotation = Mathf.Lerp(transform.rotation.y, transform.rotation.y + 1000f, t);
            transform.rotation = Quaternion.Euler(transform.rotation.x, rotation, transform.rotation.z);
            yield return null;
        }
        anim.SetBool("spin", false);
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator MirageStep()
    {
        Vector3 dashPos = playerTransform.position + (playerTransform.forward * -1.5f);
        transform.position = dashPos;

        dashPos = playerTransform.position + (playerTransform.forward * 1.5f);
        transform.position = dashPos;

        dashPos = playerTransform.position + (new Vector3(0, 0, 1) * 1.5f);
        transform.position = dashPos;

        anim.SetTrigger("DashStab");
        yield return new WaitForSeconds(3f);
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount); // Use BaseEnemy's health reduction and death check

        // Use 'stats.maxHealth' instead of 'maxHealth' to match your base script
        if (currentPhase == BossPhase.Heavy && currentHealth <= stats.maxHealth * phaseTransitionThreshold)
        {
            StartCoroutine(TransitionToLightPhase());
        }
    }

    IEnumerator TransitionToLightPhase()
    {
        isTransitioning = true;
        agent.isStopped = true;
        currentPhase = BossPhase.Light;

        if (anim != null) anim.SetTrigger("OnPhaseTransition");
        yield return new WaitForSeconds(2f);

        SetupPhase(BossPhase.Light);
        agent.isStopped = false;
        isTransitioning = false;
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

    void SetupPhase(BossPhase phase)
    {
        if (agent == null) return;
        agent.speed = (phase == BossPhase.Heavy) ? heavySpeed : lightSpeed;
        agent.acceleration = (phase == BossPhase.Heavy) ? 8f : 15f;
    }
}