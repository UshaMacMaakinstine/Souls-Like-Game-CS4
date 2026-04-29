// using UnityEngine;

// public class PerformanceManager : MonoBehaviour
// {
//     public float currentGauge = 0f;
//     public float decayRate = 1f;
//     public MrWatsonController boss;
//     public SkinnedMeshRenderer bossRenderer;

//     void Update()
//     {
//         if (boss.currentState == BossState.Phase1 || boss.currentState == BossState.Transitioning) return;

//         currentGauge = Mathf.Max(0, currentGauge - decayRate * Time.deltaTime);
        
//         // Lerp color to Red based on gauge
//         float redness = currentGauge / 100f;
//         bossRenderer.material.SetColor("_EmissionColor", Color.red * redness);

//         if (currentGauge >= 100f) boss.TriggerWheelCutscene();
//     }

//     public void AddPerformance(float amount) => currentGauge += amount;
//     public void ResetGauge() => currentGauge = 0f;
// }