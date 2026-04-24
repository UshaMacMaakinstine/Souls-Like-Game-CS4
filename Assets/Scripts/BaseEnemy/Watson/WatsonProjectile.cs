using System.Diagnostics;
using UnityEngine;

public class WatsonProjectile : MonoBehaviour
{
    public float speed = 50f;
    public float lifetime = 5f;
    public float damage = 10f;

    private GameObject player;

    void Start()
    {
        Destroy(gameObject, lifetime); // Clean up
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // Move forward relative to how it was spawned
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
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