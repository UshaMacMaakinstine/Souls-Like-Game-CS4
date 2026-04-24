using UnityEngine;

public enum BossState { Phase1, Transitioning, Teacher, Violin, Karaoke }

public class MrWatsonController : MonoBehaviour
{
    public Animator anim;
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

    public void TakeDamage(float damage, LimbType limb)
    {
        if (limb == LimbType.Leg && !isDown)
        {
            currentLegDamage += damage;
            //anim.SetTrigger("Light_Flinch"); // Reactive hit animation

            if (currentLegDamage >= legHealth)
            {
                StartCoroutine(DownedSequence());
            }
        }
        else if (limb == LimbType.Head && isDown)
        {
            bodyHealth -= damage * 2f; // Bonus damage for headshots
            //anim.SetTrigger("Head_Hit_Flinch");
        }

        // Check for Phase Transition
        if (bodyHealth <= 600f && currentState == BossState.Phase1)
        {
            StartPhase2();
        }
    }

    public void FireBullet()
    {
        if (bulletPrefab && fingerGunMuzzle)
        {
            Instantiate(bulletPrefab, fingerGunMuzzle.position, fingerGunMuzzle.rotation);
            // Add a small muzzle flash or sound effect here if you have one
        }
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

    System.Collections.IEnumerator DownedSequence()
    {
        isDown = true;
        anim.SetTrigger("FallOver");
        anim.SetBool("isDown", true);
        
        yield return new WaitForSeconds(5f); // Stay down for 5 seconds
        
        anim.SetTrigger("GetUp");
        anim.SetBool("isDown", false);
        currentLegDamage = 0;
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