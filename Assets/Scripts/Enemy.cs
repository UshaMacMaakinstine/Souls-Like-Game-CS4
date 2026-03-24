using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health;

    public void Die()
    {
        if (health <= 0)
            Destroy(gameObject);
    }
}
