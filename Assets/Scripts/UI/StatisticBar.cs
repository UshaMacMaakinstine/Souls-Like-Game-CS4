using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatisticBar : MonoBehaviour
{
    [SerializeField]
    public float stat;
    [SerializeField]
    public float max;
    [SerializeField]
    private Vector2 size;
    [SerializeField]
    private float borderWidth;
    [SerializeField]
    private Image barBG;
    [SerializeField]
    private Image barBorder;
    [SerializeField]
    private Image barFill;
    [SerializeField]
    private Image barFX;
    [SerializeField]
    private Text statText;
    private float FXStat;
    private float FillStat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // set size of the bar based on size and borderWidth
        barBorder.rectTransform.sizeDelta = size + new Vector2(borderWidth * 2, borderWidth * 2);
        barBG.rectTransform.sizeDelta = size;
        barBG.rectTransform.anchoredPosition = Vector2.zero;
        barFill.rectTransform.sizeDelta = new Vector2(size.x * Mathf.Clamp(stat / max,0,max), size.y);
        barFill.rectTransform.anchoredPosition = Vector2.zero;
        barFX.rectTransform.sizeDelta = size;
        barFX.rectTransform.anchoredPosition = Vector2.zero + new Vector2(-size.x * (1 - Mathf.Clamp(stat / max,0,max)) / 2, 0);
        FXStat = stat;
        FillStat = stat;
    }

    // Update is called once per frame
    void Update()
    {
        if(FillStat != stat)
        {
            if(FillStat < stat)
            {
                FillStat = Mathf.Lerp(FillStat, stat, Time.deltaTime * 10);
                if(Mathf.Abs(FillStat - stat) < 0.01f)
                {
                    FillStat = stat;
                }
            }
            else
            {
                FillStat = stat;
            }
            barFill.rectTransform.sizeDelta = new Vector2(size.x * Mathf.Clamp(FillStat / max,0,max), size.y);
            barFill.rectTransform.anchoredPosition = Vector2.zero + new Vector2(-size.x * (1 - Mathf.Clamp(FillStat / max,0,max)) / 2, 0);
        }

        if(FXStat != stat)
        {
            FXStat = Mathf.Lerp(FXStat, stat, Time.deltaTime * 5);
            barFX.rectTransform.sizeDelta = new Vector2(size.x * Mathf.Clamp(FXStat / max,0,max), size.y);
            barFX.rectTransform.anchoredPosition = Vector2.zero + new Vector2(-size.x * (1 - Mathf.Clamp(FXStat / max,0,max)) / 2, 0);

            if(Mathf.Abs(FXStat - stat) < 0.01f)
            {
                FXStat = stat;
            }

        }
    }

    // set the stat value and update the bar
    public void SetStat(float newStat)
    {
        stat = newStat;
    }

    public float GetStat()
    {
        return stat;
    }

    public void SetMax(float newMax)
    {
        max = newMax;
        barFill.rectTransform.sizeDelta = new Vector2(size.x * Mathf.Clamp(stat / max,0,max), size.y);
        barFill.rectTransform.anchoredPosition = Vector2.zero + new Vector2(-size.x * (1 - Mathf.Clamp(stat / max,0,max)) / 2, 0);
    }

    public float GetMax()
    {
        return max;
    }

    public string GetStatText()
    {
        if(statText == null)
        {
            return "";
        }
        return statText.text.Replace("{stat}", stat.ToString()).Replace("{max}", max.ToString());
    }

    public void SetStatText(string newText)
    {
        if(statText != null)
        {
            statText.text = newText;
        }
        return;
    }
}
