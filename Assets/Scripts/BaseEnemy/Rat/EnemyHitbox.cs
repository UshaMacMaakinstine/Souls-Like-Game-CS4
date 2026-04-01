using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public float damage = 10f;
    public float knockback = 5f;
    [SerializeField]
    private bool isActive = false;

    public void SetActive(bool active) => isActive = active; 

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Look for the PlayerProperties script specifically
        PlayerProperties player = other.GetComponentInParent<PlayerProperties>();

        if (player != null)
        {
            DamageData data = new DamageData
            {
                damageAmount = damage,
                origin = transform.position,
                knockbackForce = knockback
            };

            player.TakeDamage(data); // This now matches the script above!
            isActive = false;
        }
    }
}