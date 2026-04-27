using UnityEngine;

public enum LimbType { Leg, Head, Body }

public class BossHitbox : MonoBehaviour
{
    public MrWatsonController bossController;
    public LimbType limbType;
}