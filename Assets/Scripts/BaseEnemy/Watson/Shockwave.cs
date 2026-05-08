using System;
using UnityEngine;
public class Shockwave : MonoBehaviour
{
    public float expandSpeed = 15f;
    public float damage = 20f;

    void Update()
    {
        transform.localScale += new Vector3(10, 10, 0) * expandSpeed * Time.deltaTime;
        if (transform.localScale.x > 25000f) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<ThirdPersonController>();
            // Only hit if player is NOT jumping (isGrounded)
            if (controller != null && controller.animator.GetBool("IsGrounded"))
            {
                other.GetComponent<PlayerProperties>().TakeDamage(new DamageData { damageAmount = damage });
            }
        }
    }
}