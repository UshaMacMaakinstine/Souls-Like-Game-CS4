using UnityEngine;
using UnityEngine.UI;

public class StatisticBar : MonoBehaviour
{
    [SerializeField]
    private float stat;
    [SerializeField]
    private float max;
    [SerializeField]
    private Vector2 size;
    [SerializeField]
    private Vector2 position;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // set size of the bar based on size and borderWidth
        barBorder.rectTransform.sizeDelta = size + new Vector2(borderWidth * 2, borderWidth * 2);
        barBorder.rectTransform.anchoredPosition = position;
        barBG.rectTransform.sizeDelta = size;
        barBG.rectTransform.anchoredPosition = position;
        barFill.rectTransform.sizeDelta = new Vector2(size.x * Mathf.Clamp(stat / max,0,max), size.y);
        barFill.rectTransform.anchoredPosition = position;
        barFX.rectTransform.sizeDelta = size;
        barFX.rectTransform.anchoredPosition = position;
    }

    // Update is called once per frame
    void Update()
    {
        barFill.rectTransform.sizeDelta = new Vector2(size.x * Mathf.Clamp(stat / max,0,max), size.y);
    }

    // set the stat value and update the bar
    public void SetStat(float newStat)
    {
        stat = newStat;
        barFill.rectTransform.sizeDelta = new Vector2(size.x * Mathf.Clamp(stat / max,0,max), size.y);
        // update the bar based on the new stat value
    }
}
