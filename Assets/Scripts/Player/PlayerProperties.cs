using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProperties : MonoBehaviour
{
    public float currentHealth = 100f;
    public float maxHealth;
    public bool controlsReversed = false; // NEW
    public StatisticBar healthBar; // Ensure this is assigned in Inspector
    public WeaponFramework currentWeapon;
    public ThirdPersonController player;
    public BoxCollider[] hurtboxes;
    public TMP_Text healthText;
    public GameObject HUD;

    [Header("Death UI")]
    public GameObject deathScreenCanvas;

    private void Start()
    {
        maxHealth = currentHealth;
        player = GetComponent<ThirdPersonController>();
        currentWeapon = GetComponentInChildren<WeaponFramework>();

        if (deathScreenCanvas != null) deathScreenCanvas.SetActive(false);
        UpdateUI();
    }

    public bool Heal(float potionAmount)
    {
        if (currentHealth <= 0) return false;
        if (currentHealth >= maxHealth) return false;
        if (potionAmount > (maxHealth - currentHealth))
        {
            currentHealth = maxHealth;
            return true;
        }
        currentHealth += potionAmount;
        UpdateUI();
        return true;
    }

    public void TakeDamage(DamageData data)
    {
        // Use the player reference already cached in Start()
        if (currentHealth <= 0 || (player != null && player.isInvincible)) return;

        currentHealth -= data.damageAmount;
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Tell the controller to interrupt everything for the hit animation
            if (player != null)
            {
                player.ClearActionsForDamage();
            }
        }
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
        if (player != null)
        {
            player.enabled = false;
            // Kill physics so the character stops moving
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            if (player.animator != null)
            {
                player.animator.SetLayerWeight(1, 0);
                player.animator.SetTrigger("Die");
            }
        }

        // Trigger the smooth fade
        if (deathScreenCanvas != null)
        {
            StartCoroutine(FadeInDeathScreen());
        }

        // Unlock cursor so they can click restart
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Optional: Extreme slow motion
        //Time.timeScale = 0.2f;
    }

    private IEnumerator FadeInDeathScreen()
    {
        // 1. Get the Canvas Group from the death screen
        CanvasGroup cg = deathScreenCanvas.GetComponent<CanvasGroup>();
        CanvasGroup hg = HUD.GetComponent<CanvasGroup>();

        float duration = 2.0f; // Seconds to fade
        float currentTime = 0f;

        deathScreenCanvas.SetActive(true);

        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime; // Use unscaled so it works if Time.timeScale is 0
            cg.alpha = Mathf.Lerp(0, 1, currentTime / duration);
            hg.alpha = Mathf.Lerp(1, 0, currentTime / duration);
            yield return null;
        }

        // 3. Enable buttons once fully visible
        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        hg.alpha = 0;
        hg.interactable = false;
        hg.blocksRaycasts = false;

        HUD.SetActive(false);
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