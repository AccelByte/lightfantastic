#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
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
    private Button button;
    [SerializeField]
    private Text buttonText;
    [SerializeField]
    private OnMouseHoverComponent mouseHoverComponent;
    
    [SerializeField]
    public string text;

    private void SetHoverColor()
    {
        button.image.color = TINT_HOVER_IMAGE;
        buttonText.color = TINT_HOVER_TEXT; 
    }

    private void SetNormalColor()
    {
        button.image.color = TINT_NORMAL_IMAGE;
        buttonText.color = TINT_NORMAL_TEXT; 
    }

    private void SetDisableColor()
    {
        button.image.color = TINT_DISABLED_IMAGE;
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
        button.interactable = enable;
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

    public Button getButton()
    {
        return button;
    }

    private void Update()
    {
        buttonText.text = text;
    }
}
