using TMPro;
using UnityEngine;
using System.Collections;

public enum BossState { Phase1, Transitioning, Teacher, Violin, Karaoke }

public class MrWatsonController : MonoBehaviour
{
    public Animator anim;
    private UnityEngine.AI.NavMeshAgent agent;
    public BossState currentState = BossState.Phase1;
    public bool isDown = false;

    [Header("Stats")]
    public float bodyHealth = 1000f;
    public float legHealth = 100f;
    private float currentLegDamage = 0f;

    [Header("Props")]
    public GameObject violinProp; // Assign in Inspector
    public GameObject micProp;    // Assign in Inspector

    [Header("Attack References")]
    public Transform fingerGunMuzzle; // Where bullets come from
    public GameObject bulletPrefab;    // Your projectile
    public float stompDamage = 20f;
    public float stompRadius = 5f;

    public GameObject healthBar;
    public TMP_Text damageText;

    [Header("Stacking Damage UI")]
    private float accumulatedDamage = 0f;
    private Coroutine damageStackCoroutine;
    public float stackResetTime = 1.5f; // How long to wait before resetting the stack

    void Start()
    {
        // Get the agent component once at the start
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
    
    public void TakeDamage(float damage, LimbType limb)
    {
        if (limb == LimbType.Leg && !isDown)
        {
            currentLegDamage += damage;
            //anim.SetTrigger("Light_Flinch"); // Reactive hit animation
            bodyHealth -= damage;
            

            if (currentLegDamage >= legHealth)
            {
                StartCoroutine(DownedSequence());
                currentLegDamage = 0f;
            }
        }
        else if (limb == LimbType.Head && isDown)
        {
            bodyHealth -= damage * 2f; // Bonus damage for headshots
            //anim.SetTrigger("Head_Hit_Flinch");
        }

        UpdateStackedDamage(damage);

        // Check for Phase Transition
        if (bodyHealth <= 600f && currentState == BossState.Phase1)
        {
            StartPhase2();
        }

        UpdateUI();
    }

    public void FireBullet()
    {
        StartCoroutine(FireBurst(3, 0.15f)); // 3 bullets, 0.15s apart
    }

    private IEnumerator FireBurst(int count, float delay)
    {
        for (int i = 0; i < count; i++)
        {
            if (bulletPrefab && fingerGunMuzzle)
            {
                // Spawn the bullet
                GameObject projectile = Instantiate(bulletPrefab, fingerGunMuzzle.position, fingerGunMuzzle.rotation);
                
                // Optional: Add a tiny bit of random spread so the bullets aren't perfectly pixel-perfect
                projectile.transform.Rotate(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
            }
            
            yield return new WaitForSeconds(delay);
        }
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

    // 2. RECEIVER FOR: Stomp / Melee
    public void ApplyMeleeDamage()
    {
        // Simple logic: Check if player is close enough when the foot hits
        float dist = Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position);
        
        if (dist <= stompRadius)
        {
            // Replace with your actual Player Health script call
            // player.TakeDamage(stompDamage);
            Debug.Log("Watson STOMPED the player!");
        }
    }

    void StartPhase2()
    {
        currentState = BossState.Transitioning;
        anim.SetTrigger("EnterPhase2");
        anim.SetInteger("Phase", 1); // Updates the Animator's logic
    }

    private void UpdateUI()
    {
        healthBar.GetComponent<StatisticBar>().stat = bodyHealth;
    }

    public IEnumerator DownedSequence()
    {
        isDown = true;
    
        // 1. Get both the Agent and the AI script
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        MrWatsonAI aiScript = GetComponent<MrWatsonAI>();

        // 2. STOPS the AI from running its Update/Coroutines
        if (aiScript != null) aiScript.enabled = false; 

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false; // Physically removes the upright capsule
        }

        // 3. Play the animation
        anim.SetBool("isDown", true);
        anim.SetTrigger("FallOver");

        float elapsed = 0f;

        while (elapsed < 1.133f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 1.133f);
            float rotation = Mathf.Lerp(0, 90, t);
            float up = Mathf.Lerp(transform.position.y, (transform.position.y + 10), t);
            transform.rotation = Quaternion.Euler(rotation, 0f, 0f);
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        anim.SetTrigger("GetUp");
        anim.SetBool("isDown", false);

        elapsed = 0f;

        while (elapsed < 2.33f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 2.33f);
            float rotation = Mathf.Lerp(90, 0, t);
            float up = Mathf.Lerp((transform.position.z + 10), transform.position.z, t);
            transform.rotation = Quaternion.Euler(rotation, 0f, 0f);
            transform.position = new Vector3(transform.position.x, transform.position.y, up);
            yield return null;
        }

        yield return new WaitForSeconds(2f); 

        // 4. Turn everything back on
        if (agent != null) agent.enabled = true;
        if (aiScript != null) aiScript.enabled = true;
        
        isDown = false;
    }

    // Called by Animation Event at the end of PhaseChange_Anim
    public void FinishPhaseTransition()
    {
        currentState = BossState.Teacher;
    }
    
    public void TriggerWheelCutscene()
    {
        currentState = BossState.Transitioning;
        anim.SetTrigger("SpinWheel");
    }

    // Call this via Animation Event at the end of the SpinWheel animation
    // or directly from PerformanceManager after a delay
    public void SwitchToMode(BossState newMode)
    {
        currentState = newMode;
        
        // Deactivate all props first
        if(violinProp) violinProp.SetActive(false);
        if(micProp) micProp.SetActive(false);

        // Reset Animator Ints
        anim.SetInteger("Mode", 0);

        // Activate specific mode
        switch (newMode)
        {
            case BossState.Violin:
                anim.SetInteger("Mode", 1);
                if(violinProp) violinProp.SetActive(true);
                break;
            case BossState.Karaoke:
                anim.SetInteger("Mode", 2);
                if(micProp) micProp.SetActive(true);
                break;
            case BossState.Teacher:
                anim.SetInteger("Phase", 1); // Ensure we stay in Phase 2 logic
                break;
        }
    }
}