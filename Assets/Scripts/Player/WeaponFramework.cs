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
        if (player != null && player.isAttacking)
        {
            float damageToApply = CalculateDamage();
            if (damageToApply <= 0) return;

            // UNIVERSAL CHECK 1: Is this a specialized Hitbox/Limb? (Watson, etc.)
            if (other.TryGetComponent<BossHitbox>(out var limb))
            {
                limb.bossController.TakeDamage(damageToApply, limb.limbType);
                StartCoroutine(ChangeColorFeedback(other.gameObject));
                return; 
            }

            // UNIVERSAL CHECK 2: Is this a standard Enemy?
            if (other.CompareTag("Enemy"))
            {
                BaseEnemy enemy = other.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damageToApply);
                    StartCoroutine(ChangeColorFeedback(other.gameObject));
                }
            }
        }
    }

    private float CalculateDamage()
    {
        switch (attackMode)
        {
            case AttackMode.lightAttack: return weaponStats.lightAttackDamage;
            case AttackMode.heavyAttack: return weaponStats.heavyAttackDamage;
            case AttackMode.runningHeavy: return weaponStats.runningHeavyAttackDamage;
            default: return 0;
        }
    }

    IEnumerator ChangeColorFeedback(GameObject target)
    {
        if (target != null)
        {
            Renderer targetRenderer = target.GetComponent<Renderer>();
            if (targetRenderer == null) targetRenderer = target.GetComponentInChildren<Renderer>();
            if (targetRenderer == null) yield break;

            Color originalColor = targetRenderer.material.color;
            Color feedbackColor = Color.white;

            switch (attackMode)
            {
                case AttackMode.lightAttack: feedbackColor = Color.red; break;
                case AttackMode.heavyAttack: feedbackColor = Color.green; break;
                case AttackMode.runningHeavy: feedbackColor = Color.blue; break;
            }
    
            targetRenderer.material.color = feedbackColor;
            yield return new WaitForSeconds(0.2f);
            targetRenderer.material.color = originalColor;
        }
    }
}