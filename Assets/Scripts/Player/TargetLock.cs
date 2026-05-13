using UnityEngine;
using Cinemachine;

public class TargetLock : MonoBehaviour
{
    public CinemachineTargetGroup targetGroup;
    public Transform currentTarget;

    private float lastLockTime;
    private float lockCooldown = 0.2f; // 200 milliseconds
    public CinemachineVirtualCamera lockOnVirtualCamera;

    public void AssignTarget(Transform enemy)
    {
        currentTarget = enemy;
        targetGroup.AddMember(enemy, 1f, 1f);

        // Switch to the Lock-On camera by giving it high priority
        lockOnVirtualCamera.Priority = 20;
    }

    public void RemoveTarget()
    {
        // Switch back to normal camera by lowering priority
        lockOnVirtualCamera.Priority = 5;

        if (targetGroup.m_Targets.Length > 1)
        {
            targetGroup.RemoveMember(targetGroup.m_Targets[1].target);
        }
        currentTarget = null;
    }

    // TargetLock.cs
    public void OnLockOnPressed()
    {
        Debug.Log("Attempting Lock-On...");

        if (Time.time < lastLockTime + lockCooldown) return;
        lastLockTime = Time.time;

        if (currentTarget != null)
        {
            Debug.Log("Clearing Target");
            RemoveTarget();
            return;
        }

        Collider[] enemies = Physics.OverlapSphere(transform.position, 100f);
        Debug.Log("Found " + enemies.Length + " objects in range.");

        foreach (var enemy in enemies)
        {
            Debug.Log("Checking object: " + enemy.transform.root.name + " Tag: " + enemy.transform.root.tag);
            if (enemy.transform.root.CompareTag("Enemy"))
            {
                AssignTarget(enemy.transform.root);
                Debug.Log("Locked onto: " + enemy.transform.root.name);
                return;
            }
        }
    }
}