using System.Collections;
using UnityEngine;

public class WeaponFramework : MonoBehaviour
{
    public enum AttackMode { lightAttack, heavyAttack, runningHeavy, None }

    public AttackMode attackMode;
    public WeaponStats weaponStats;
    private ThirdPersonController player;
    

    void Start()
    {
        player = gameObject.GetComponentInParent<ThirdPersonController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ensure player exists and is currently in an attack state
        if (player != null && player.isAttacking)
        {
            if (other.CompareTag("Enemy") && gameObject.CompareTag("PlayerWeapon"))
            {
                BaseEnemy enemyComponent = other.GetComponent<BaseEnemy>();

                if (enemyComponent != null)
                {
                    float damageToApply = 0;

                    switch (attackMode)
                    {
                        case AttackMode.lightAttack:
                            damageToApply = weaponStats.lightAttackDamage;
                            break;
                        case AttackMode.heavyAttack:
                            damageToApply = weaponStats.heavyAttackDamage;
                            break;
                        case AttackMode.runningHeavy:
                            damageToApply = weaponStats.runningHeavyAttackDamage;
                            break;
                    }

                    if (damageToApply > 0)
                    {
                        enemyComponent.TakeDamage(damageToApply);
                        StartCoroutine(ChangeColorFeedback(other.gameObject));
                    }
                }
            }
        }
    }

    IEnumerator ChangeColorFeedback(GameObject enemy)
    {
        if (enemy != null)
        {
            Renderer enemyRenderer = enemy.GetComponent<Renderer>();
            if (enemyRenderer == null) yield break;

            Color feedbackColor = Color.white;
            switch (attackMode)
            {
                case AttackMode.lightAttack: feedbackColor = Color.red; break;
                case AttackMode.heavyAttack: feedbackColor = Color.green; break;
                case AttackMode.runningHeavy: feedbackColor = Color.blue; break;
            }
    
            enemyRenderer.material.color = feedbackColor;
            yield return new WaitForSeconds(0.2f);
            enemyRenderer.material.color = Color.white;
        }
    }
}