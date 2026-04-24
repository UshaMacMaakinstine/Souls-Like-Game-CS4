using UnityEngine;

public class WatsonProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public float damage = 10f;

    void Start()
    {
        Destroy(gameObject, lifetime); // Clean up
    }

    void Update()
    {
        // Move forward relative to how it was spawned
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // other.GetComponent<PlayerHealth>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}