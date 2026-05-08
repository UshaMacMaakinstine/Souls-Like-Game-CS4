using UnityEngine;

public class Sweep : MonoBehaviour
{
    [SerializeField] private MrWatsonAI ai;
    void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.CompareTag("Player") && ai.sweeping)
        {
            other.transform.root.GetComponent<ThirdPersonController>().ApplyKnockback(transform.position, 20f);
        }
    }
}
