#pragma warning disable 0649

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnMouseHoverComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action OnMouseHoverAction; 
    public Action OnMouseExitAction; 
    
    private void OnMouseOver()
    {
        if (OnMouseHoverAction != null)
        {
            OnMouseHoverAction();
        }
    }

    private void OnMouseExit()
    {
        if (OnMouseExitAction != null)
        {
            OnMouseExitAction();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseOver();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnMouseExit();
    }
}
