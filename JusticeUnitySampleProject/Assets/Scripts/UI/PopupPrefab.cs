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
    [Tooltip("Set true if you want to show a small close icon")]
    public bool showExitButton;
    [SerializeField]
    [Tooltip("Called when close button is clicked. SUGGESTION: use this to hide the prompt panel")]
    public UnityEvent onExitButtonClicked;

    [SerializeField]
    private PromptPanelComponent component;

    void Start()
    {
        component.primaryButton.text = primaryButtonText;
        component.primaryButton.getButton().onClick.AddListener(primaryButtonAction);

        component.secondaryButton.text = secondaryButtonText;
        component.secondaryButton.getButton().onClick.AddListener(secondaryButtonAction);

        component.closeButton.getButton().onClick.AddListener(onExitButtonClicked.Invoke);

        component.closeButtonGameObject.SetActive(showExitButton);
        component.headerText.text = header;
        component.descriptionText.text = description;
    }

    private void Update()
    {
        component.closeButtonGameObject.SetActive(showExitButton);
        component.primaryButton.text = primaryButtonText;
        component.secondaryButton.text = secondaryButtonText;
        component.headerText.text = header;
        component.descriptionText.text = description;
    }

    public void Show()
    {
        component.gameObject.SetActive(true);
        component.promptPanelCanvasGroup.alpha = 1;
        component.promptPanelCanvasGroup.blocksRaycasts = true;
        component.promptPanelCanvasGroup.interactable = true;
    }

    public void Hide()
    {
        component.gameObject.SetActive(false);
        component.promptPanelCanvasGroup.alpha = 0;
        component.promptPanelCanvasGroup.blocksRaycasts = false;
        component.promptPanelCanvasGroup.interactable = false;
    }

    public void SetText(string _header, string _description)
    {
        header = _header;
        description = _description;
    }
}
