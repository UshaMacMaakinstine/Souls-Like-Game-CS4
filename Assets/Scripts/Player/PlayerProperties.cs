using UnityEngine;

public class PlayerProperties : MonoBehaviour
{
    public float currentHealth;
    public WeaponFramework currentWeapon;
    public ThirdPersonController player;
    public BoxCollider[] hurtboxes;

    private void Start()
    {
        player = GetComponent<ThirdPersonController>();
        currentWeapon = GetComponentInChildren<WeaponFramework>();
    }

    public void Heal(float potionAmount)
    {
        currentHealth += potionAmount;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Player took " + amount + " damage!");
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        Debug.Log("Player has been defeated.");
    }
}