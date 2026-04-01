using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public float damage = 10f;
    public float knockback = 5f;
    [SerializeField]
    private bool isActive = false;
    private Collider hitboxCollider;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        // Force the collider off at the start so the green box disappears
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

        // Use InParent to find PlayerProperties even if we hit the 'cockroach' child
        PlayerProperties playerProps = other.GetComponentInParent<PlayerProperties>();

        if (playerProps != null)
        {
            ThirdPersonController controller = playerProps.player;

            if (controller != null && controller.isInvincible)
            {
                Debug.Log("Player is invincible! Attack whiffed.");
                return;
            }

            DamageData data = new DamageData
            {
                damageAmount = damage,
                origin = transform.position,
                knockbackForce = knockback
            };

            playerProps.TakeDamage(data);
            SetActive(false); // Turn off after hit
        }
    }
}