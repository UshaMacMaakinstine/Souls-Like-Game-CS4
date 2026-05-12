using UnityEngine;

public class Shockwave : MonoBehaviour
{
    public float expandSpeed = 15f;
    public float damage = 20f;
    public float thickness = 2f;
    public float maxScale = 25000f;

    private Transform playerTransform;
    private ThirdPersonController playerController;
    private PlayerProperties playerProps;
    private MeshRenderer ringRenderer; // To get the actual mesh size
    private bool hasHitPlayer = false;

    void Start()
    {
        ringRenderer = GetComponent<MeshRenderer>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<ThirdPersonController>();
            playerProps = player.GetComponent<PlayerProperties>();
        }
    }

    void Update()
    {
        // Expand visuals (using your original logic)
        transform.localScale += new Vector3(10, 10, 0) * expandSpeed * Time.deltaTime;

        if (!hasHitPlayer && playerTransform != null)
        {
            CheckRingCollision();
        }

        if (transform.localScale.x > maxScale)
        {
            Destroy(gameObject);
        }
    }

    private void CheckRingCollision()
    {
        // Get the actual world-space radius from the mesh bounds
        float currentRadius = ringRenderer.bounds.extents.x;

        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(playerTransform.position.x, 0, playerTransform.position.z)
        );

        if (distance >= currentRadius - thickness && distance <= currentRadius + thickness)
        {
            if (playerController != null && playerController.animator.GetBool("IsGrounded"))
            {
                ApplyDamage();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (ringRenderer == null) ringRenderer = GetComponent<MeshRenderer>();
        if (ringRenderer == null) return;

        // Use the same bounds logic for the Gizmo
        float currentRadius = ringRenderer.bounds.extents.x;

        Gizmos.color = Color.red;
        DrawWireDisk(transform.position, currentRadius + thickness);

        Gizmos.color = Color.yellow;
        DrawWireDisk(transform.position, currentRadius - thickness);
    }

    private void DrawWireDisk(Vector3 center, float radius)
    {
        float angle = 0f;
        Vector3 lastPoint = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= 32; i++)
        {
            angle = i * (Mathf.PI * 2) / 32;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }

    private void ApplyDamage()
    {
        hasHitPlayer = true;
        if (playerProps != null)
        {
            playerProps.TakeDamage(new DamageData { damageAmount = damage });
        }
    }
}