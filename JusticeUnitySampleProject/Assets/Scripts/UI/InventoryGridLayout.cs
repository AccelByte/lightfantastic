#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

public class InventoryGridLayout : MonoBehaviour
{
    #region UI Componenet
    [SerializeField]
    private RectTransform gridLayoutContentContainer;
    [SerializeField]
    private Scrollbar verticalScrollBar;
    [SerializeField]
    private OnMouseHoverComponent mouseHoverComponent;
    [SerializeField]
    private CanvasGroup canvasGroup;
    #endregion
    
    public T[] PopulateChild<T>(int count, GameObject prefab)
    {
        T[] populatedEntries = VerticalScrollViewPopulation<T>.Populate(count, prefab, gridLayoutContentContainer);
        // Normalize the scale since it uses VerticalScrollViewPopulation
        foreach (T entry in populatedEntries)
        {
            (entry as MonoBehaviour).GetComponent<RectTransform>().localScale = Vector3.one;
        }
        mouseHoverComponent.OnMouseExitAction = () => { verticalScrollBar.targetGraphic.color = new Color(255, 255, 255, 128);};
        mouseHoverComponent.OnMouseHoverAction = () => { verticalScrollBar.targetGraphic.color = Color.clear;};
        return populatedEntries;
    }

    public void SetVisibility(bool isVisible)
    {
        canvasGroup.alpha = isVisible ? 1 : 0;
        canvasGroup.interactable = isVisible;
    }
}
