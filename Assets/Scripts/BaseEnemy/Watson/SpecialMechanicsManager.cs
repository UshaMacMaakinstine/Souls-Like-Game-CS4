using UnityEngine;
using System.Collections;

public class SpecialMechanicsHandler : MonoBehaviour
{
    public MrWatsonController boss;
    public ThirdPersonController player;

    // Called via Animation Event: Watson shouts "Paws Up!"
    public void ExecutePawsUp()
    {
        StartCoroutine(PawsUpRoutine());
    }

    IEnumerator PawsUpRoutine()
    {
        float checkDuration = 2.0f;
        while (checkDuration > 0)
        {
            // If player moves while not dodging/rolling
            if (player.CurrentVelocityMagnitude > 0.1f && !player.isInvincible)
            {
                // Penalty: Flash freeze player or high damage
                player.isMovementFrozen = true;
                Debug.Log("Detention! Should've studied better.");
                yield return new WaitForSeconds(1f);
                player.isMovementFrozen = false;
                yield break;
            }
            checkDuration -= Time.deltaTime;
            yield return null;
        }
    }

    // Called when entering Karaoke Mode
    public void SetKaraokeMode(bool active)
    {
        player.controlMultiplier = active ? -1f : 1f;
    }
}