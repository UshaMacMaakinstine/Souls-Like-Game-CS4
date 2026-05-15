using UnityEngine;
using System.Collections;
using TMPro;
using GLTFast;

public class MrWatsonController : MonoBehaviour
{
    [Header("Health Settings")]
    public float bodyHealth;
    public float maxBodyHealth;
    public float legHealth;
    public float phase2Health;
    public float damageRadius;

    public GameObject foot;
    private float currentLegDamage = 0f;
    public bool isDown = false;

    [Header("Performance Gauge (Rage)")]
    public float performanceGauge;
    public float maxPerformance;
    public float gaugeDrainRate;

    [Header("References")]
    public BossState currentState = BossState.Phase1;
    public Animator anim;
    public GameObject micProp, pointerStickProp;
    public StatisticBar bossHealthBar;
    public GameObject bulletPrefab;
    public Transform fingerGunMuzzle;

    [Header("Stacking Damage UI")]
    private float accumulatedDamage = 0f;
    private Coroutine damageStackCoroutine;
    public float stackResetTime = 1.5f; // How long to wait before resetting the stack
    public TMP_Text damageText;

    public AudioSource audioSource;

    [Header("Voice Lines")]
    public AudioClip introLine;
    public AudioClip phaseTwoLine;
    public AudioClip KarokeLine;
    public AudioClip DamageLine;
    public AudioClip PointerFling;
    public AudioClip TeacherLine;
    public AudioClip Attete;
    public AudioClip PawsUp;
    public AudioClip playerDiedLine;
    public AudioClip watsonDied;
    public AudioClip[] singingLines;

    private MrWatsonAI ai;

    private bool tooMuchDmg;
    private float damageTakenToHead;
    public GameObject deathEffect;
    public float maxSinkSpeed = 0.05f;
    public float dissolveTime = 3.0f;
    public float postBlackSinkTime = 5.0f;

    [Header("Dynamic Particles")]
    public float maxEmissionBase = 100f;
    public float maxSizeMultiplier = 0.15f;
    public float particleLifetime = 6.0f; // Increased for "staying around longer"

    void Start()
    {
        PlayVoice(introLine);
        
        ai = GetComponent<MrWatsonAI>();
        if (bossHealthBar != null) bossHealthBar.SetMax(maxBodyHealth);
        if (bossHealthBar != null) bossHealthBar.stat = bodyHealth;
        SetBossMode(BossState.Phase1);
    }

    void Update()
    {
        if (isDown || currentState == BossState.Transitioning) return;

        // Rage slowly drains if the player isn't attacking
        if (performanceGauge > 0 && currentState == BossState.Karaoke)
            performanceGauge -= gaugeDrainRate * Time.deltaTime;

        if (performanceGauge <= 0 && currentState == BossState.Karaoke && !isDown && !ai.isAttacking)
        {
            PlayVoice(TeacherLine);
            performanceGauge = 0;
            SetBossMode(BossState.Teacher);
        }

        // Transition to Special Modes
        if (performanceGauge >= maxPerformance && currentState != BossState.Transitioning && !isDown && !ai.isAttacking && currentState != BossState.Karaoke)
        {
            PlayVoice(KarokeLine);
            SetBossMode(BossState.Karaoke);
        }

        if(bodyHealth < phase2Health && currentState == BossState.Phase1 && !isDown && !ai.isAttacking)
        {
            StartCoroutine(phaseChange());
        }

        if(bodyHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    public void PlayVoice(AudioClip clip)
    {
        if (clip == null) return;

        // Stop the previous line if the boss is already talking
        if (audioSource.isPlaying)
            audioSource.Stop();

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void TakeDamage(float damage, LimbType limb)
    {
        if (currentState == BossState.Transitioning) return;

        if (limb == LimbType.Leg && !isDown)
        {
            PlayVoice(DamageLine);
            currentLegDamage += damage;
            bodyHealth -= damage * 0.4f;
            UpdateStackedDamage(damage);
            if(currentState != BossState.Phase1 && currentState != BossState.Karaoke)
                performanceGauge += damage * 0.05f;
            if (currentLegDamage >= legHealth) StartCoroutine(DownedSequence());
        }
        else if (limb == LimbType.Head && isDown && !tooMuchDmg)
        {
            bodyHealth -= (damage * 2f); // Massive damage window
            damageTakenToHead += damage * 2f;
            UpdateStackedDamage(damage * 2f);
            if (currentState != BossState.Phase1 && currentState != BossState.Karaoke)
                performanceGauge += damage * 0.08f; // Punish the player with Rage for doing high damage
            if (damageTakenToHead > 1000f) tooMuchDmg = true;
        }

        if (bossHealthBar != null) bossHealthBar.stat = bodyHealth;
        
        // Visual indicator: He gets faster as he gets angrier
        anim.speed = 1f + (performanceGauge / maxPerformance) * 0.4f;
    }

    IEnumerator phaseChange()
    {
        PlayVoice(phaseTwoLine);
        currentState = BossState.Transitioning;
        anim.SetTrigger("Phase2");
        yield return new WaitForSeconds(1.167f);
        SetBossMode(BossState.Teacher);
        yield return new WaitForSeconds(2.238f);
    }

    private void UpdateStackedDamage(float damage)
    {
        // 1. Add to the total
        accumulatedDamage += damage;
        
        // 2. Show the text (make sure the object is active)
        damageText.gameObject.SetActive(true);
        damageText.text = Mathf.RoundToInt(accumulatedDamage).ToString();

        // 3. Reset the "Close UI" timer
        if (damageStackCoroutine != null)
        {
            StopCoroutine(damageStackCoroutine);
        }
        
        damageStackCoroutine = StartCoroutine(ResetDamageStack());
    }

    IEnumerator Die()
    {
        anim.SetTrigger("Die");
        yield return new WaitForSeconds(3.6f);

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

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
        PlayVoice(watsonDied);
        NextBossController controller = GameObject.Find("NextBossController").GetComponent<NextBossController>();
        controller.readyForNext = true;
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

    public void KickKnockBack()
    {
        if(!ai.playerTransform.gameObject.GetComponent<ThirdPersonController>().isInvincible)
            ai.playerTransform.gameObject.GetComponent<ThirdPersonController>().ApplyKnockback(transform.position, 300f);
    }

    IEnumerator ResetDamageStack()
    {
        // Wait for the player to stop dealing damage
        yield return new WaitForSeconds(stackResetTime);

        // Fade out or just disable
        damageText.gameObject.SetActive(false);
        
        // Reset the counter for the next time they start hitting
        accumulatedDamage = 0f;
        damageStackCoroutine = null;
    }

    public void FireBullet()
    {
        PlayVoice(Attete);
        if (fingerGunMuzzle != null && bulletPrefab != null)
        {
            // Instantiate the bullet at the muzzle position/rotation
            GameObject bullet = Instantiate(bulletPrefab, fingerGunMuzzle.position, fingerGunMuzzle.rotation);
            bullet.GetComponent<WatsonProjectile>().Launch();
            // Optional: If your bullet has a script to set its damage
            // bullet.GetComponent<BulletScript>().damage = 10f;
        }
    }

    IEnumerator DownedSequence()
    {
        isDown = true;
        ai.enabled = false;
        anim.SetBool("isDown", true);

        float elapsed = 0f;
        while(elapsed < 8f)
        {
            if(tooMuchDmg)
            {
                Debug.Log("Took Too Much Damage");
                break;
            }

            elapsed+=Time.deltaTime;
            yield return null;
        }

        anim.SetBool("isDown", false);
        currentLegDamage = 0f;
        damageTakenToHead = 0f;
        tooMuchDmg = false;

        ai.enabled = true;
        isDown = false;
    }

    public void ApplyMeleeDamage()
    {
        if(currentState == BossState.Phase1)
        {
            // Simple sphere check to see if player is in front of Watson during the slam
            Collider[] hitPlayers = Physics.OverlapSphere(transform.position, damageRadius);

            bool hit = false;
            foreach (Collider col in hitPlayers)
            {
                if (col.transform.root.CompareTag("Player") && !hit)
                {
                    float damage = 250f;
                    col.transform.root.GetComponent<PlayerProperties>().TakeDamage(new DamageData { damageAmount = damage });
                    hit = true;
                    Debug.Log("Watson slammed the player!");
                }
            }
        }
    }

    public void SetBossMode(BossState newMode)
    {
        currentState = newMode;

        if (pointerStickProp) pointerStickProp.SetActive(newMode == BossState.Teacher);
        if (micProp) micProp.SetActive(newMode == BossState.Karaoke);
        
        anim.SetInteger("Mode", (int)newMode);
        
        // If he's no longer in Phase 1, ensure the animator knows
        if (newMode != BossState.Phase1) anim.SetInteger("Phase", 1);
        
        Debug.Log("Boss state set to: " + (int)newMode);
    }

    // --- ANIMATION EVENTS ---

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 1. Melee Radius (Red) - The "Danger Zone"
        Gizmos.color = Color.magenta;
        DrawWireDisk(transform.position, damageRadius);
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