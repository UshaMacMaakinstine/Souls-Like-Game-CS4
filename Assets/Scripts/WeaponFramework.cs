using System.Collections;
using UnityEngine;

public class WeaponFramework : MonoBehaviour
{
    public enum AttackMode { lightAttack, heavyAttack, runningHeavy, None }

    public AttackMode attackMode;
    public int lightAttackDamage;
    public int heavyAttackDamage;
    public int runningHeavyAttackDamage;
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

                switch(attackMode)
                {
                    case AttackMode.lightAttack:
                        //enemyComponent.health -= lightAttackDamage;
                        StartCoroutine(changeColor(enemy, player));
                        break;
                    case AttackMode.heavyAttack:
                        //enemyComponent.health -= heavyAttackDamage;
                        StartCoroutine(changeColor(enemy, player));
                        break;
                    case AttackMode.runningHeavy:
                        //enemyComponent.health -= runningHeavyAttackDamage;
                        StartCoroutine(changeColor(enemy, player));
                        break;
                    default:
                        break;
                }

                enemyComponent.Die();
            }
        }
    }

    IEnumerator changeColor(GameObject enemy, ThirdPersonController player)
    {
        if(enemy != null)
        {
            switch(attackMode)
            {
                case AttackMode.lightAttack:
                    enemy.GetComponent<Renderer>().material.color = Color.red;
                    yield return new WaitForSeconds(0.5f);
                    enemy.GetComponent<Renderer>().material.color = Color.white;
                    break;
                case AttackMode.heavyAttack:
                    enemy.GetComponent<Renderer>().material.color = Color.green;
                    yield return new WaitForSeconds(0.5f);
                    enemy.GetComponent<Renderer>().material.color = Color.white;
                    break;
                case AttackMode.runningHeavy:
                    enemy.GetComponent<Renderer>().material.color = Color.blue;
                    yield return new WaitForSeconds(0.5f);
                    enemy.GetComponent<Renderer>().material.color = Color.white;
                    break;
                default:
                    break;
            }
        }
    }
}
