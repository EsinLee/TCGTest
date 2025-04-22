using UnityEngine;
using UnityEngine.EventSystems;

public class CardOfCardListPrefab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string id;

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
