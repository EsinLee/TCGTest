using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainDeckCardPrefab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string id;
    public Image cardImage;
    public Image DressImage;
    public Image colorImage;
    public Image levelImage;
    public TextMeshProUGUI numText;

    public delegate void PointerEnterDelegate();
    public event PointerEnterDelegate OnPointerEnterEvent;
    public delegate void PointerExitDelegate();
    public event PointerExitDelegate OnPointerExitEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke();
    }
}
