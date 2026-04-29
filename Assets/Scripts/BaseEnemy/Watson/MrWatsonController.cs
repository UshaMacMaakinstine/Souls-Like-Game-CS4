using UnityEngine;
using System.Collections;

public class MrWatsonController : MonoBehaviour
{
    [Header("Health Settings")]
    public float bodyHealth = 1000f;
    public float legHealth = 7200f;
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

        if(bodyHealth < 30000)
        {
            SetBossMode(BossState.Teacher);
        }

        if (limb == LimbType.Leg && !isDown)
        {
            currentLegDamage += damage;
            bodyHealth -= damage; 
            if (currentLegDamage >= legHealth) StartCoroutine(DownedSequence());
        }
        else if (limb == LimbType.Head && isDown)
        {
            bodyHealth -= (damage * 2f); // Massive damage window
            performanceGauge += (damage * 0.4f); // Punish the player with Rage for doing high damage
        }
        else
        {
            bodyHealth -= damage;
            performanceGauge += (damage * 0.15f);
        }

        if (bossHealthBar != null) bossHealthBar.stat = bodyHealth;
        
        // Visual indicator: He gets faster as he gets angrier
        anim.speed = 1f + (performanceGauge / maxPerformance) * 0.4f;
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
        anim.SetTrigger("FallOver");

        // Logic to tip him over 90 degrees
        float elapsed = 0;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(90, transform.eulerAngles.y, 0);
        while(elapsed < 1.33f) {
            elapsed += Time.deltaTime * 1.33f;
            transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed);
            yield return null;
        }

        yield return new WaitForSeconds(6f); // Time player has to hit the head

        anim.SetTrigger("GetUp");
        anim.SetBool("isDown", false);
        currentLegDamage = 0f;

        // Return upright
        elapsed = 0;
        while(elapsed < 2.33f) {
            elapsed += Time.deltaTime * 2.33f;
            transform.rotation = Quaternion.Lerp(endRot, startRot, elapsed);
            yield return null;
        }

        ai.enabled = true;
        isDown = false;
    }

    public void ApplyMeleeDamage()
    {
        // Simple sphere check to see if player is in front of Watson during the slam
        float damageRadius = 15f;
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position + transform.forward * 2, damageRadius);

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
        
        Debug.Log("Boss state set to: " + newMode);
    }

    // --- ANIMATION EVENTS ---

    // This is what the 'Spin the Wheel' Animation Event calls
    // It has NO arguments so the error CS1501 will disappear
    public void SwitchToMode()
    {
        Debug.Log("Wheel Animation finished - Mode transition complete.");
    }
}