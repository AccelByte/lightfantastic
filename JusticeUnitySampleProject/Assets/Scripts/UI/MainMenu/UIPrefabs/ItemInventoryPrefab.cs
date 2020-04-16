// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

#pragma warning disable 0649

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using AccelByte.Models;
using Image = UnityEngine.UI.Image;

public class ItemInventoryPrefab : MonoBehaviour
{
    
    #region UI COMPONENT
    [SerializeField]
    public Image image;
    [SerializeField]
    public Image overlay;
    [SerializeField]
    public Image checkmark;
    [SerializeField]
    public Image defaultBorder;
    [SerializeField]
    public Button button;
    #endregion

    private const float FADE_SELECTION_DURATION = 0.05f;
    
    private ItemInfo abItemInfo;
    private bool isSelected = false;

    public void Init(UnityAction onItemSelected, ItemInfo itemInfo)
    {
        abItemInfo = itemInfo;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { onItemSelected(); });
        Unselect();
    }

    public ItemInfo GetItemInfo()
    {
        return abItemInfo;
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    private void UiDisableItem()
    {
        defaultBorder.CrossFadeAlpha(255, FADE_SELECTION_DURATION, true);
        overlay.CrossFadeAlpha(0, FADE_SELECTION_DURATION, true);
        checkmark.CrossFadeAlpha(0, FADE_SELECTION_DURATION, true);
    }

    private void UiEnabledMode()
    {
        overlay.CrossFadeAlpha(255, FADE_SELECTION_DURATION, true);
        checkmark.CrossFadeAlpha(255, FADE_SELECTION_DURATION, true);
        defaultBorder.CrossFadeAlpha(0, FADE_SELECTION_DURATION, true);
    }

    public void Unselect()
    {
        isSelected = false;
        UiDisableItem();
    }

    public void Select()
    {
        isSelected = true;
        UiEnabledMode();
    }
}
