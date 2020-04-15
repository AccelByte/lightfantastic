#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class AccelByteButtonScriptStyle : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private Text buttonText;
    [SerializeField]
    private OnMouseHoverComponent mouseHoverComponent;
    
    [SerializeField]
    public string text;

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
//        if (enable)
//        {
//            SetNormalColor();
//            mouseHoverComponent.OnMouseHoverAction = SetHoverColor;
//           mouseHoverComponent.OnMouseExitAction = SetNormalColor;
//        }
//        else
//        {
//            SetDisableColor();
//            mouseHoverComponent.OnMouseHoverAction = null;
//            mouseHoverComponent.OnMouseExitAction = null;
//        }
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
