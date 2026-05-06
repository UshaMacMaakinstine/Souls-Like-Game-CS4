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
    public float performanceGauge = 0f;
    public float maxPerformance = 100f;
    public float gaugeDrainRate = 1.2f;

    [Header("References")]
    public BossState currentState = BossState.Phase1;
    public Animator anim;
    public GameObject violinProp, micProp, pointerStickProp;
    public StatisticBar bossHealthBar;
    public GameObject bulletPrefab;
    public Transform fingerGunMuzzle;

    [Header("Stacking Damage UI")]
    private float accumulatedDamage = 0f;
    private Coroutine damageStackCoroutine;
    public float stackResetTime = 1.5f; // How long to wait before resetting the stack
    public TMP_Text damageText;

    private MrWatsonAI ai;

    void Start()
    {
        ai = GetComponent<MrWatsonAI>();
        if (bossHealthBar != null) bossHealthBar.SetMax(bodyHealth);
        SetBossMode(BossState.Phase1);
    }

    void Update()
    {
        if (isDown || currentState == BossState.Transitioning) return;

        // Rage slowly drains if the player isn't attacking
        if (performanceGauge > 0) 
            performanceGauge -= gaugeDrainRate * Time.deltaTime;

        // Transition to Special Modes
        if (performanceGauge >= maxPerformance && currentState != BossState.Transitioning) 
            StartCoroutine(SpinTheWheelSequence());
    }

    public void TakeDamage(float damage, LimbType limb)
    {
        if (currentState == BossState.Transitioning) return;

        if (limb == LimbType.Leg && !isDown)
        {
            currentLegDamage += damage;
            bodyHealth -= damage; 
            UpdateStackedDamage(damage);
            if (currentLegDamage >= legHealth) StartCoroutine(DownedSequence());
        }
        else if (limb == LimbType.Head && isDown)
        {
            bodyHealth -= (damage * 2f); // Massive damage window
            UpdateStackedDamage(damage * 2f);
            performanceGauge += damage * 0.4f; // Punish the player with Rage for doing high damage
        }
        else
        {
            bodyHealth -= damage;
            performanceGauge += damage * 0.15f;
        }

        if (bossHealthBar != null) bossHealthBar.stat = bodyHealth;
        
        // Visual indicator: He gets faster as he gets angrier
        anim.speed = 1f + (performanceGauge / maxPerformance) * 0.4f;

        if(bodyHealth < phase2Health)
        {
            StartCoroutine(phaseChange());
        }
    }

    IEnumerator phaseChange()
    {
        currentState = BossState.Transitioning;
        anim.SetTrigger("Phase2");
        yield return new WaitForSeconds(1.167f + 2.383f);
        SetBossMode(BossState.Teacher);
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
        if (fingerGunMuzzle != null && bulletPrefab != null)
        {
            // Instantiate the bullet at the muzzle position/rotation
            GameObject bullet = Instantiate(bulletPrefab, fingerGunMuzzle.position, fingerGunMuzzle.rotation);
            
            // Optional: If your bullet has a script to set its damage
            // bullet.GetComponent<BulletScript>().damage = 10f;
        }
    }

    IEnumerator DownedSequence()
    {
        isDown = true;
        ai.enabled = false;
        anim.SetBool("isDown", true);

        yield return new WaitForSeconds(12f); // Window for headshots

        anim.SetBool("isDown", false);
        currentLegDamage = 0f;

        ai.enabled = true;
        isDown = false;
    }

    public void ApplyMeleeDamage()
    {
        // Simple sphere check to see if player is in front of Watson during the slam
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, damageRadius);

        bool hit = false;
        foreach (Collider col in hitPlayers)
        {
            if (col.transform.root.CompareTag("Player") && !hit)
            {
                float damage = 0;
                if(currentState == BossState.Phase1) damage = 250f; 
                if(currentState == BossState.Teacher) damage = 150f;
                if(currentState == BossState.Violin) damage = 400f;
                col.transform.root.GetComponent<PlayerProperties>().TakeDamage(new DamageData { damageAmount = damage });
                hit = true;
                Debug.Log("Watson slammed the player!");
            }
        }
    }

    IEnumerator SpinTheWheelSequence()
    {
        currentState = BossState.Transitioning;
        ai.enabled = false;
        performanceGauge = 0f;

        anim.SetTrigger("SpinWheel");
        yield return new WaitForSeconds(4f); // Duration of the cutscene

        // 50/50 chance for Violin or Karaoke
        BossState nextMode = (Random.value > 0.5f) ? BossState.Violin : BossState.Karaoke;
        SetBossMode(nextMode);
        
        ai.enabled = true;
    }

    public void SetBossMode(BossState newMode)
    {
        currentState = newMode;

        if (pointerStickProp) pointerStickProp.SetActive(newMode == BossState.Teacher);
        if (violinProp) violinProp.SetActive(newMode == BossState.Violin);
        if (micProp) micProp.SetActive(newMode == BossState.Karaoke);
        
        anim.SetInteger("Mode", (int)newMode);
        
        // If he's no longer in Phase 1, ensure the animator knows
        if (newMode != BossState.Phase1) anim.SetInteger("Phase", 1);
        
        Debug.Log("Boss state set to: " + (int)newMode);
    }

    // --- ANIMATION EVENTS ---

    // This is what the 'Spin the Wheel' Animation Event calls
    // It has NO arguments so the error CS1501 will disappear
    public void SwitchToMode()
    {
        Debug.Log("Wheel Animation finished - Mode transition complete.");
    }

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