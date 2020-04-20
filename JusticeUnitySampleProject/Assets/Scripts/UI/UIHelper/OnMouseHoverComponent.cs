// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

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
