#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

public class AccelByteButtonScriptStyle : MonoBehaviour
{
    [SerializeField]
    private Color TINT_NORMAL_IMAGE;
    [SerializeField]
    private Color TINT_HOVER_IMAGE;
    [SerializeField]
    private Color TINT_NORMAL_TEXT;
    [SerializeField]
    private Color TINT_HOVER_TEXT;
    [SerializeField]
    private Image buttonImage;
    [SerializeField]
    private Text buttonText;
    [SerializeField]
    private OnMouseHoverComponent mouseHoverComponent;
    
    [SerializeField]
    public string text;

    private void SetHoverColor()
    {
        buttonImage.color = TINT_HOVER_IMAGE;
        buttonText.color = TINT_HOVER_TEXT; 
    }

    private void SetNormalColor()
    {
        buttonImage.color = TINT_NORMAL_IMAGE;
        buttonText.color = TINT_NORMAL_TEXT; 
    }

    private void SetText(string text)
    {
        buttonText.text = text;
    }

    private void Start()
    {
        if (mouseHoverComponent != null)
        {
            mouseHoverComponent.OnMouseHoverAction = SetHoverColor;
            mouseHoverComponent.OnMouseExitAction = SetNormalColor;
        }
        SetNormalColor();
        SetText(text);
    }
}
