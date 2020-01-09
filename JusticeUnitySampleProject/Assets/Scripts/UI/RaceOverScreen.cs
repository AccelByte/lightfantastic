using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceOverScreen : BaseHUD
{
    [Header("UI Elements")]
    [SerializeField, Tooltip("The button to return to menu")]
    private Button toMenuButton_;
    [SerializeField, Tooltip("Race over announcer text")]
    private TextMeshProUGUI announcerText_;

    // [SerializeField]
    // private TextMeshPr
    private void Awake()
    {

    }

    public override void OnShow()
    {

    }

    public override void OnHide()
    {

    }

    protected override void AddListeners()
    {

    }

    #region Properties
    public TextMeshProUGUI AnnouncerText { get; }

    #endregion
}
