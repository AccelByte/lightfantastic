using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class PromptPanel : MonoBehaviour
{
    [SerializeField][Tooltip("Main prompt question")] 
    private string header;
    [SerializeField][Tooltip("[Optional] Description of this prompt")]
    private string description;
    
    [SerializeField][Tooltip("Primary (Suggested) button text")]
    private string primaryButtonText;
    [SerializeField][Tooltip("Action from the primary button")]
    private UnityEvent primaryButtonAction;
    
    [SerializeField][Tooltip("Secondary button text")]
    private string secondaryButtonText;
    [SerializeField][Tooltip("Action from the secondary button")]
    private UnityEvent secondaryButtonAction;
    
    [SerializeField][Tooltip("Set true if you want to show a small close icon")]
    private bool showExitButton;
    [SerializeField][Tooltip("Called when close button is clicked. SUGGESTION: use this to hide the prompt panel")]
    private UnityEvent onExitButtonClicked;
    
    [SerializeField]
    private PromptPanelComponent component;
    
    void Start()
    {
        component.primaryButton.text = primaryButtonText;
        component.primaryButton.getButton().onClick.AddListener(primaryButtonAction.Invoke);
        
        component.secondaryButton.text = secondaryButtonText;
        component.secondaryButton.getButton().onClick.AddListener(secondaryButtonAction.Invoke);
        
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
