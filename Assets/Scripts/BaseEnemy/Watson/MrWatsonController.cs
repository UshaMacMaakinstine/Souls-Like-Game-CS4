using UnityEngine;
using System.Collections;
using TMPro;

public class MrWatsonController : MonoBehaviour
{
    [Header("Health Settings")]
    public float bodyHealth;
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
    public AudioClip[] singingLines;

    private MrWatsonAI ai;

    private bool tooMuchDmg;
    private float damageTakenToHead;

    void Start()
    {
        PlayVoice(introLine);
        
        ai = GetComponent<MrWatsonAI>();
        if (bossHealthBar != null) bossHealthBar.SetMax(bodyHealth);
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
        if (performanceGauge >= maxPerformance && currentState != BossState.Transitioning && !isDown && !ai.isAttacking)
        {
            PlayVoice(KarokeLine);
            SetBossMode(BossState.Karaoke);
        }

        if(bodyHealth < phase2Health && currentState == BossState.Phase1 && !isDown && !ai.isAttacking)
        {
            StartCoroutine(phaseChange());
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
            if(currentState != BossState.Phase1)
                performanceGauge += damage * 0.05f;
            if (currentLegDamage >= legHealth) StartCoroutine(DownedSequence());
        }
        else if (limb == LimbType.Head && isDown && !tooMuchDmg)
        {
            bodyHealth -= (damage * 2f); // Massive damage window
            damageTakenToHead += damage * 2f;
            UpdateStackedDamage(damage * 2f);
            if (currentState != BossState.Phase1)
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

    public void KickKnockBack()
    {
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