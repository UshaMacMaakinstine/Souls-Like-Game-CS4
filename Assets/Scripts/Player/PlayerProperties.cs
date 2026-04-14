using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProperties : MonoBehaviour
{
    public float currentHealth = 100f;
    public WeaponFramework currentWeapon;
    public ThirdPersonController player;
    public BoxCollider[] hurtboxes;
    public TMP_Text healthText;
    public GameObject healthBar;

    [Header("Death UI")]
    public GameObject deathScreenCanvas;

    private void Start()
    {
        player = GetComponent<ThirdPersonController>();
        currentWeapon = GetComponentInChildren<WeaponFramework>();

        if (deathScreenCanvas != null) deathScreenCanvas.SetActive(false);
        UpdateUI();
    }

    public void Heal(float potionAmount)
    {
        if (currentHealth <= 0) return;
        currentHealth += potionAmount;
        UpdateUI();
    }

    public void WheelOfFortune(float multiplier)
    {
        //random effect of healing, +atk, +iframes, or the reverse
        switch(Random.Range(0, 1))
        {
            //HP Change
            case 0: // Heal
                Heal(10f * multiplier);
                break;
            case 1: // Hurt
                Heal(-10f * multiplier);
                break;
        }
    }

    public void TakeDamage(DamageData data)
    {
        if (currentHealth <= 0) return;

        // Merge: Check for invincibility first
        if (player != null && !player.isInvincible)
        {
            currentHealth -= data.damageAmount;

            // Optional: You can use data.knockbackForce here later

            UpdateUI();

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                UpdateUI();
                Die();
            }
        }
    }

    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = "Health : " + Mathf.Max(0, currentHealth);
            healthBar.GetComponent<StatisticBar>().stat = currentHealth;
    }

    protected virtual void Die()
    {
        Debug.Log("Player has been defeated.");

        if (player != null)
        {
            player.enabled = false;
            // Check if Die trigger exists to avoid errors
            if (player.animator != null && HasParameter("Die", player.animator))
            {
                player.animator.SetTrigger("Die");
            }
        }

        if (deathScreenCanvas != null)
        {
            deathScreenCanvas.SetActive(true);
        }
    }

    private bool HasParameter(string paramName, Animator anim)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}