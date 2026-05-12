using UnityEngine;

public class BossWeapons : MonoBehaviour
{
    [SerializeField] private int damage;
    bool damaged = false;
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.CompareTag("Player") && !damaged)
        {
            other.transform.root.GetComponent<PlayerProperties>().TakeDamage(new DamageData{damageAmount = damage});
            damaged = true;
        }
    }
}
