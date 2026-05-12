using UnityEngine;

public class WatsonProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float launchSpeed = 80f;
    public float returnSpeed = 120f;
    public float damage = 10f;
    public float lifetime = 5f;

    private Vector3 moveDirection;
    private bool isLaunched = false;
    private bool isReturning = false;
    private Transform bossReturnAnchor;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // New Helper Method to find the player at the moment of launch
    private void SetDirectionToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Aim for the player's chest/center (3 units up)
            Vector3 targetPoint = player.transform.position + Vector3.up * 4.5f;
            moveDirection = (targetPoint - transform.position).normalized;
            transform.forward = moveDirection;
        }
    }

    // --- MODE 1: Standard Launch (For Pointers & Spheres) ---
    public void Launch()
    {
        SetDirectionToPlayer(); // Recalculate target right now!
        isLaunched = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = moveDirection * launchSpeed;
        }

        Destroy(gameObject, lifetime);
    }

    // --- MODE 2: Violent Return (For the Mic) ---
    public void LaunchWithReturn(Transform returnPoint)
    {
        SetDirectionToPlayer(); // Recalculate target right now!
        bossReturnAnchor = returnPoint;
        isLaunched = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(moveDirection * launchSpeed, ForceMode.VelocityChange);
        }

        Invoke("StartReturn", 0.7f);
    }

    void StartReturn()
    {
        isReturning = true;
        isLaunched = false;
        if (rb != null) rb.isKinematic = true;
    }

    void Update()
    {
        if (isLaunched && rb == null)
        {
            transform.position += moveDirection * launchSpeed * Time.deltaTime;
        }

        if (isReturning && bossReturnAnchor != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, bossReturnAnchor.position, returnSpeed * Time.deltaTime);
            transform.Rotate(Vector3.right * 2000 * Time.deltaTime);

            if (Vector3.Distance(transform.position, bossReturnAnchor.position) < 2f)
            {
                bossReturnAnchor.gameObject.SetActive(true); 
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            other.transform.root.GetComponent<PlayerProperties>().TakeDamage(new DamageData { damageAmount = damage });
            if (!bossReturnAnchor) Destroy(gameObject);
        }
    }
}