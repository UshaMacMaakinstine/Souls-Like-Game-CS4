using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public float damage = 10f;
    public float knockback = 5f;
    private bool isActive = false;
    private Collider hitboxCollider;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider != null) hitboxCollider.enabled = false;
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = active;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        PlayerProperties playerProps = other.GetComponent<PlayerProperties>();

        if (playerProps != null)
        {
            // Check for the controller's invincibility state
            ThirdPersonController controller = playerProps.player;

            if (controller != null)
            {
                // We check isInvincible because that is what Hussein used in Roll()
                if (controller.isInvincible)
                {
                    Debug.Log("Player is invincible! Attack whiffed.");
                    return;
                }
            }

            DamageData data = new DamageData
            {
                damageAmount = damage,
                origin = transform.position,
                knockbackForce = knockback
            };

            playerProps.TakeDamage(data);

            // Turn off hitbox after one hit to prevent multiple triggers in one frame
            SetActive(false);
        }
    }
}