using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProperties : MonoBehaviour
{
    public float currentHealth = 100f;
    public bool controlsReversed = false; // NEW
    public StatisticBar healthBar; // Ensure this is assigned in Inspector
    public WeaponFramework currentWeapon;
    public ThirdPersonController player;
    public BoxCollider[] hurtboxes;
    public TMP_Text healthText;

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

    public void TakeDamage(DamageData data)
    {
        ThirdPersonController player = GetComponent<ThirdPersonController>();
        if (currentHealth <= 0 || (player != null && player.isInvincible)) return;

        currentHealth -= data.damageAmount;
        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    public void SetReverseControls(bool state) => controlsReversed = state;

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