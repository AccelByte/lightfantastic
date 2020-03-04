using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingComponentBoolean : MonoBehaviour
{
    private bool state;
    [SerializeField] private string trueStateLabel;
    [SerializeField] private string falseStateLabel;
    [SerializeField] private Text stateLabel;
    [Header("TRUE component")]
    [SerializeField] private Button trueButton;
    [SerializeField] private Image trueTriangleImage;
    [Header("FALSE component")]
    [SerializeField] private Button falseButton;
    [SerializeField] private Image falseTriangleImage;

    public void AddListeners(UnityAction onTrueStateAction, UnityAction onFalseStateAction)
    {
        trueButton.onClick.AddListener(onTrueStateAction);
        falseButton.onClick.AddListener(onFalseStateAction);
    }
    
    public void SetTrue(){ trueButton.onClick.Invoke(); }
    public void SetFalse(){ falseButton.onClick.Invoke(); }
    
    private void Start()
    {
        falseButton.onClick.AddListener(delegate { SetState(false); });
        trueButton.onClick.AddListener(delegate { SetState(true); });
        SetTrue();
    }

    private void SetState(bool state_)
    {
        state = state_;
        stateLabel.text = state ? trueStateLabel : falseStateLabel;
        if (state)
        {
            SetInteractabilityTrueButton(false);
            SetInteractabilityFalseButton(true);
        }
        else
        {
            SetInteractabilityTrueButton(true);
            SetInteractabilityFalseButton(false);
        }
    }

    private void SetInteractabilityTrueButton(bool interactable)
    {
        trueButton.interactable = interactable;
        trueTriangleImage.color = interactable ? Color.white : new Color(1,1,1,0.3f);
    }

    private void SetInteractabilityFalseButton(bool interactable)
    {
        falseButton.interactable = interactable;
        falseTriangleImage.color = interactable ? Color.white : new Color(1,1,1,0.3f);
    }

    public bool GetState()
    {
        return state;
    }

    public void DisableSetting()
    {
        SetInteractabilityTrueButton(false);
        SetInteractabilityFalseButton(false);
    }
}
