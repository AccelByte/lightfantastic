#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

public class CustomScrollView : MonoBehaviour
{
    #region UI Componenet
    [SerializeField]
    private RectTransform scrollViewContentContainer;
    [SerializeField]
    private Image verticalScrollBarHandle;
    #endregion
    
    private bool needScrollBar = false;
    
    private void OnMouseOver()
    {
        Debug.Log("MOUSE OVER");
    }
    
    private void SetImageVisible(bool visble, Image image)
    {
        int alphaChannelShow = visble ? 255 : 0;
        image.color = new Color(255, 255, 255, alphaChannelShow);
    }
    
    public T[] PopulateChild<T>(int count, GameObject prefab)
    {
        T[] populatedEntries = VerticalScrollViewPopulation<T>.Populate(count, prefab, scrollViewContentContainer);
        this.needScrollBar = VerticalScrollViewPopulation<T>.IsVerticalScrollable(scrollViewContentContainer);
        
        if (needScrollBar)
        {
            if (gameObject.GetComponent<OnMouseHoverComponent>() == null)
            {
                gameObject.AddComponent<OnMouseHoverComponent>();
            }
            
            gameObject.GetComponent<OnMouseHoverComponent>().OnMouseHoverAction = () =>
            {
                SetImageVisible(true, verticalScrollBarHandle);
            };
            
            gameObject.GetComponent<OnMouseHoverComponent>().OnMouseExitAction = () =>
            {
                SetImageVisible(false, verticalScrollBarHandle);
            };
        }
        
        return populatedEntries;
    }
}
