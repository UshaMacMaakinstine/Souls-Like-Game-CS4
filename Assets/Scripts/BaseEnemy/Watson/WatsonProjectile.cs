using UnityEngine;

public class WatsonProjectile : MonoBehaviour
{
    public float speed = 50f;
    public float lifetime = 5f;
    public float damage = 10f;

    private Vector3 moveDirection;
    private bool initialized = false;

    void Start()
    {
        Destroy(gameObject, lifetime);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 1. Calculate the target point (5 units above the player)
            Vector3 targetPoint = new Vector3(player.transform.position.x, player.transform.position.y + 3f, player.transform.position.z);
            
            // 2. Calculate the direction from the projectile's spawn to that point
            moveDirection = (targetPoint - transform.position).normalized;
            
            // 3. Optional: Make the projectile "look" where it's going
            transform.forward = moveDirection;
            
            initialized = true;
        }
    }

    void Update()
    {
        if (initialized)
        {
            // 4. Move in that saved direction forever
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            Debug.Log("Hit the Player");
            other.transform.root.GetComponent<PlayerProperties>().TakeDamage(new DamageData { damageAmount = damage });
        }

        Destroy(gameObject);
    }
}