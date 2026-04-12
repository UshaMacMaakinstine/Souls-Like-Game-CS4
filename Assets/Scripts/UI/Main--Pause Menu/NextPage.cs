using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class NextPage : MonoBehaviour
{
    int pageNumber = 0;
    public GameObject[] pages;
    public Button PreviousButton;
    public Button NextButton;
    void Update()
    {
        if(pageNumber >= (pages.Length - 1))
            NextButton.GetComponent<Button>().interactable = false;
        else
            NextButton.GetComponent<Button>().interactable = true;
        if(pageNumber <= 0)
            PreviousButton.GetComponent<Button>().interactable = false;
        else
            PreviousButton.GetComponent<Button>().interactable = true;
    }
    public void PageNext()
    {
        pages[pageNumber].SetActive(false);
        pages[pageNumber + 1].SetActive(true);
        pageNumber++;
    }

    public void PagePrev()
    {
        pages[pageNumber].SetActive(false);
        pages[pageNumber - 1].SetActive(true);
        pageNumber--;
    }
}
