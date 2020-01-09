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
    private Color TINT_DISABLED_IMAGE;
    [SerializeField]
    private Color TINT_DISABLED_TEXT;
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

    private void SetDisableColor()
    {
        buttonImage.color = TINT_DISABLED_IMAGE;
        buttonText.color = TINT_DISABLED_TEXT; 
    }

    private void SetText(string text)
    {
        buttonText.text = text;
    }

    private void Start()
    {
        SetEnable(true);
        SetText(text);
    }

    public void SetEnable(bool enable)
    {
        gameObject.GetComponent<Button>().interactable = enable;
        if (enable)
        {
            SetNormalColor();
            mouseHoverComponent.OnMouseHoverAction = SetHoverColor;
            mouseHoverComponent.OnMouseExitAction = SetNormalColor;
        }
        else
        {
            SetDisableColor();
            mouseHoverComponent.OnMouseHoverAction = null;
            mouseHoverComponent.OnMouseExitAction = null;
        }
    }
}
