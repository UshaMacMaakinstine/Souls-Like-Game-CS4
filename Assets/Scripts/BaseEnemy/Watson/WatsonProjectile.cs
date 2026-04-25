using System;
using UnityEngine;

public class WatsonProjectile : MonoBehaviour
{
    public float speed = 50f;
    public float lifetime = 5f;
    public float damage = 10f;

    private GameObject player;

    private Vector3 pos;

    void Start()
    {
        Destroy(gameObject, lifetime); // Clean up
        player = GameObject.FindGameObjectWithTag("Player");
        pos = player.transform.position;
    }

    void Update()
    {
        // Move forward relative to how it was spawned
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(pos.x, (pos.y + 5f), pos.z), speed * Time.deltaTime);
        if(transform.position == new Vector3(pos.x, (pos.y + 5f), pos.z))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            // other.GetComponent<PlayerHealth>().TakeDamage(damage);
            Destroy(gameObject);
            Debug.Log("Hit the Player");
        }
    }
}