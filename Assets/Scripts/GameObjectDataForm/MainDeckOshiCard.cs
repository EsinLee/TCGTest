using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainDeckOshiCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string id;
    public Image cardImage;
    public Image dressImage;
    public Image colorImage;
    [SerializeField]
    private Image previewCardImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        previewCardImage.sprite = cardImage.sprite;
        previewCardImage.color = new Color(1,1,1,1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        previewCardImage.color = new Color(0,0,0,0);
    }
}
