using System.Collections;
using UnityEngine;

public class WeaponFramework : MonoBehaviour
{
    public int lightAttackDamage;
    public int heavyAttackDamage;
    public int range;

    ThirdPersonController player;


    void Start()
    {
        player = gameObject.GetComponentInParent<ThirdPersonController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(player.isAttacking)
        {
            if(other.CompareTag("Enemy") && gameObject.CompareTag("Player"))
            {
                GameObject enemy = other.gameObject;

                Enemy enemyComponent = enemy.GetComponent<Enemy>();

                if(player.isLightAttack)
                    enemyComponent.health -= lightAttackDamage;
                else
                    enemyComponent.health -= heavyAttackDamage;

                enemyComponent.Die();

                StartCoroutine(changeColor(enemy, player));
            }
        }
    }

    IEnumerator changeColor(GameObject enemy, ThirdPersonController player)
    {
        if(enemy != null)
        {
            if(player.isLightAttack)
            {
                enemy.GetComponent<Renderer>().material.color = Color.red;
                yield return new WaitForSeconds(0.5f);
                enemy.GetComponent<Renderer>().material.color = Color.white;
            }
            else
            {
                enemy.GetComponent<Renderer>().material.color = Color.green;
                yield return new WaitForSeconds(0.5f);
                enemy.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }
}
