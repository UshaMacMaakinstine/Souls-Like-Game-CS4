using UnityEngine;

public class DefaultActive : MonoBehaviour
{
    public bool useDefault;
    public GameObject[] onObj;
    public GameObject[] offObj;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(useDefault)
        {

            for(int i = 0; i < onObj.Length;i++)
            {
                onObj[i].SetActive(true);
            }
            for(int i = 0; i < offObj.Length;i++)
            {
                offObj[i].SetActive(false);
            }
        }
    }
}
