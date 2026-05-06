using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class TargetLock : MonoBehaviour
{
    [Header("References")]
    public CinemachineFreeLook freeLookCamera;
    public CinemachineTargetGroup targetGroup;
    public float lockRadius = 30f;
    public LayerMask enemyLayer;

    [Header("Settings")]
    public bool isLocked;
    private Transform currentTarget;
    private ThirdPersonController controller;

    void Start()
    {
        controller = GetComponent<ThirdPersonController>();
    }

    // Connect this to your Input Action (e.g., Middle Mouse or R3)
    public void OnToggleLock(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (isLocked)
        {
            Unlock();
        }
        else
        {
            AttemptLock();
        }
    }

    void AttemptLock()
    {
        // Find nearby enemies
        Collider[] enemies = Physics.OverlapSphere(transform.position, lockRadius, enemyLayer);
        float closestDist = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                // Look for a specific "LockOnPoint" child, otherwise use main transform
                Transform point = enemy.transform.Find("LockOnPoint");
                bestTarget = point != null ? point : enemy.transform;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            isLocked = true;

            // Update Target Group
            targetGroup.m_Targets[1].target = currentTarget;

            // Point Camera at the Group
            freeLookCamera.LookAt = targetGroup.transform;
            
            // Lock the X/Y Axis so the player can't manual rotate the camera away
            freeLookCamera.m_XAxis.m_InputAxisName = "";
            freeLookCamera.m_YAxis.m_InputAxisName = "";
        }
    }

    public void Unlock()
    {
        isLocked = false;
        currentTarget = null;
        targetGroup.m_Targets[1].target = null;

        // Reset Camera to follow player normally
        freeLookCamera.LookAt = transform.Find("CameraFollow"); // Your original LookAt
        
        // Give control back to the mouse
        freeLookCamera.m_XAxis.m_InputAxisName = "Mouse X";
        freeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y";
    }

    public Transform GetTarget() => currentTarget;

    void Update()
    {
        // Auto-unlock if boss dies or gets too far
        if (isLocked)
        {
            if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > lockRadius + 5f)
            {
                Unlock();
            }
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw the lock-on search radius (Green if locked, White if not)
        Gizmos.color = isLocked ? Color.green : Color.black;
        
        // Draw a wire circle at the player's feet
        DrawWireDisk(transform.position, lockRadius);

        // If locked, draw a line to the current target to show the connection
        if (isLocked && currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up, currentTarget.position);
        }
    }

    // Helper to draw a flat circle on the ground (matches the Watson AI style)
    private void DrawWireDisk(Vector3 center, float radius)
    {
        float angleStep = 10f;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        for (float i = angleStep; i <= 360f; i += angleStep)
        {
            float rad = i * Mathf.Deg2Rad;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(rad) * radius, 0, Mathf.Sin(rad) * radius);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
#endif
}