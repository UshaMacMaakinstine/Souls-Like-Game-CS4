using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    private Animator anim;
    private bool isTransitioning = false;
    private bool isAttacking = false;
    private Transform playerTransform;

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
        if (playerTransform != null && agent != null && agent.enabled)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    IEnumerator DecideLightAttack()
    {
        isAttacking = true;
        currentState = EnemyState.Attacking;
        agent.isStopped = true;

        float rand = Random.value;
        if (rand > 0.7f)
            yield return StartCoroutine(AtmosphericErasure());
        else
            yield return StartCoroutine(RapidStabAttack(5));

        yield return new WaitForSeconds(1f);

        agent.isStopped = false;
        isAttacking = false;
        currentState = EnemyState.Chasing;
    }

    public IEnumerator RapidStabAttack(int stabCount)
    {
        for (int i = 0; i < stabCount; i++)
        {
            if (anim != null) anim.SetTrigger("AttackAgain");
            yield return new WaitForSeconds(0.15f);
        }
    }

    public IEnumerator AtmosphericErasure()
    {
        if (anim != null) anim.SetTrigger("SummonFog");
        yield return new WaitForSeconds(1f);

        Vector3 dashPos = playerTransform.position + (playerTransform.forward * -1.5f);
        transform.position = dashPos;

        if (anim != null) anim.SetTrigger("DashStab");
        yield return new WaitForSeconds(0.5f);
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

    void SetupPhase(BossPhase phase)
    {
        if (agent == null) return;
        agent.speed = (phase == BossPhase.Heavy) ? heavySpeed : lightSpeed;
        agent.acceleration = (phase == BossPhase.Heavy) ? 8f : 15f;
    }
}