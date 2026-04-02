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

    private void OnTriggerStay(Collider other) // Changed to Stay for reliability
    {
        if (!isActive) return;

        // 1. Try to find the PlayerProperties on the object hit, or any parent
        PlayerProperties player = other.GetComponentInParent<PlayerProperties>();

        // 2. If that fails, check if the collider belongs to a Rigidbody that has the script
        if (player == null && other.attachedRigidbody != null)
        {
            player = other.attachedRigidbody.GetComponent<PlayerProperties>();
        }

        if (player != null)
        {
            Debug.Log($"Hit confirmed on: {other.name}"); // Tells you exactly what part you hit
            
            DamageData data = new DamageData
            {
                damageAmount = damage,
                origin = transform.position,
                knockbackForce = knockback
            };

            player.TakeDamage(data);
            isActive = false; 
        }
    }
}