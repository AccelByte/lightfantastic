using UnityEngine;
using UnityEngine.UI;

public class PopupPanelComponent : MonoBehaviour
{
    [SerializeField] public AccelByteButtonScriptStyle primaryButton;
    [SerializeField] public AccelByteButtonScriptStyle secondaryButton;
    [SerializeField] public AccelByteButtonScriptStyle primarySingleButton;
    [SerializeField] public GameObject closeButtonGameObject;
    [SerializeField] public AccelByteButtonScriptStyle closeButton;
    [SerializeField] public Text headerText;
    [SerializeField] public Text descriptionText;
    [SerializeField] public CanvasGroup PopupPanelCanvasGroup;
}
