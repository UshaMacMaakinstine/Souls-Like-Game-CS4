using UnityEngine;

public class NextBossController : MonoBehaviour
{
    public GameObject bossHealthBar;
    public GameObject[] bosses;
    public int currentBoss;
    public bool readyForNext;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(bossHealthBar.GetComponent<StatisticBar>().stat <= 0 && readyForNext)
        {
            bosses[currentBoss].SetActive(false);
            currentBoss++;
            bosses[currentBoss].SetActive(true);
            readyForNext = false;
        }
    }
}
