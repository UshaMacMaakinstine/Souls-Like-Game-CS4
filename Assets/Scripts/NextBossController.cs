using UnityEngine;

public class NextBossController : MonoBehaviour
{
    public GameObject bossHealthBar;
    public GameObject[] bosses;
    public int currentBoss;
    public bool readyForNext;

    // Update is called once per frame
    void Update()
    {
        if(readyForNext)
        {
            bosses[currentBoss].SetActive(false);
            currentBoss++;
            bosses[currentBoss].SetActive(true);
            readyForNext = false;
        }
    }
}
