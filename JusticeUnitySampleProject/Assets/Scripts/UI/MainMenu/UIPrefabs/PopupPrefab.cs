using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PopupPrefab : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Main prompt question")]
    public string header;
    [SerializeField]
    [Tooltip("[Optional] Description of this prompt")]
    public string description;

    [SerializeField]
    [Tooltip("Primary (Suggested) button text")]
    public string primaryButtonText;
    [SerializeField]
    [Tooltip("Action from the primary button")]
    public UnityAction primaryButtonAction;

    [SerializeField]
    [Tooltip("Secondary button text")]
    public string secondaryButtonText;
    [SerializeField]
    [Tooltip("Action from the secondary button")]
    public UnityAction secondaryButtonAction;

    [SerializeField]
    [Tooltip("Primary (Suggested) button text")]
    public string primarySingleButtonText;
    [SerializeField]
    [Tooltip("Action from the primary button")]
    public UnityAction primarySingleButtonAction;

    [SerializeField]
    [Tooltip("Set true if you want to show a small close icon")]
    public bool showExitButton;
    [SerializeField]
    [Tooltip("Called when close button is clicked. SUGGESTION: use this to hide the prompt panel")]
    public UnityEvent onExitButtonClicked;

    [SerializeField]
    private PopupPanelComponent component;

    void Start()
    {
        component.primaryButton.text = primaryButtonText;
        component.primaryButton.getButton().onClick.AddListener(primaryButtonAction);
        component.primaryButton.getButton().onClick.AddListener(Hide);

        component.secondaryButton.text = secondaryButtonText;
        component.secondaryButton.getButton().onClick.AddListener(secondaryButtonAction);
        component.secondaryButton.getButton().onClick.AddListener(Hide);

        component.primarySingleButton.text = primarySingleButtonText;
        component.primarySingleButton.getButton().onClick.AddListener(primarySingleButtonAction);
        component.primarySingleButton.getButton().onClick.AddListener(Hide);

        component.closeButton.getButton().onClick.AddListener(onExitButtonClicked.Invoke);
        component.closeButton.getButton().onClick.AddListener(Hide);

        component.closeButtonGameObject.SetActive(showExitButton);
        component.headerText.text = header;
        component.descriptionText.text = description;
    }

    private void Update()
    {
        component.closeButtonGameObject.SetActive(showExitButton);
        component.primaryButton.text = primaryButtonText;
        component.secondaryButton.text = secondaryButtonText;
        component.primarySingleButton.text = primarySingleButtonText;
        component.headerText.text = header;
        component.descriptionText.text = description;
    }

    public void Show()
    {
        component.gameObject.SetActive(true);
        component.PopupPanelCanvasGroup.alpha = 1;
        component.PopupPanelCanvasGroup.blocksRaycasts = true;
        component.PopupPanelCanvasGroup.interactable = true;
    }

    public void Hide()
    {
        component.gameObject.SetActive(false);
        component.PopupPanelCanvasGroup.alpha = 0;
        component.PopupPanelCanvasGroup.blocksRaycasts = false;
        component.PopupPanelCanvasGroup.interactable = false;
    }

    public void SelectPopupType(E_PopupType type)
    {
        switch (type)
        {
            case E_PopupType.Popup_Default:
                component.primaryButton.gameObject.SetActive(true);
                component.secondaryButton.gameObject.SetActive(true);
                component.primarySingleButton.gameObject.SetActive(false);
                break;
            case E_PopupType.Popup_SingleButton:
                component.primarySingleButton.gameObject.SetActive(true);
                component.primaryButton.gameObject.SetActive(false);
                component.secondaryButton.gameObject.SetActive(false);
                break;
            default:
                Debug.Log("Popupprefab SelectPopupType there is no popuptype of " + type);
                break;
        }
    }

    public void SetText(string _header, string _description)
    {
        header = _header;
        description = _description;
    }
}
