using System.Collections;
using UnityEngine;

public class WeaponFramework : MonoBehaviour
{
    public int lightAttackDamage;
    public int heavyAttackDamage;
    public int range;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy") && gameObject.CompareTag("Player"))
        {
            GameObject enemy = other.gameObject;

            ThirdPersonController player = gameObject.GetComponentInParent<ThirdPersonController>();

            Enemy enemyComponent = enemy.GetComponent<Enemy>();

            if(player.isLightAttack)
                enemyComponent.health -= lightAttackDamage;
            else
                enemyComponent.health -= heavyAttackDamage;

            enemyComponent.Die();

            StartCoroutine(changeColor(enemy, player));
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
