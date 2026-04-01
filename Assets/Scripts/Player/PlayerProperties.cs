using TMPro;
using UnityEngine;

public class PlayerProperties : MonoBehaviour
{
    public float currentHealth;
    public WeaponFramework currentWeapon;
    public ThirdPersonController player;
    public BoxCollider[] hurtboxes;
    public TMP_Text healthText;

    private void Start()
    {
        player = GetComponent<ThirdPersonController>();
        currentWeapon = GetComponentInChildren<WeaponFramework>();

        healthText.text = "Health : " + currentHealth;
    }

    public void Heal(float potionAmount)
    {
        currentHealth += potionAmount;
        healthText.text = "Health : " + currentHealth;
    }

    public void TakeDamage(DamageData data) // Changed from 'float amount' to 'DamageData data'
    {
        float amount = data.damageAmount; // Pull the number out of the package

        currentHealth -= amount;
        Debug.Log("Player took " + amount + " damage from " + data.origin);
        healthText.text = "Health : " + currentHealth;

        // You now have access to data.knockbackForce here too!

        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        Debug.Log("Player has been defeated.");
    }
}