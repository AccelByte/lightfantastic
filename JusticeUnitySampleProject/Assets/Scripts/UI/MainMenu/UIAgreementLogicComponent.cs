// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAgreementLogicComponent : MonoBehaviour
{
    [SerializeField]
    private Text agreementDocumentText;
    [SerializeField]
    private AccelByteButtonScriptStyle nextButton;
    [SerializeField]
    private AccelByteButtonScriptStyle previousButton;
    [SerializeField]
    private AccelByteButtonScriptStyle acceptButton;
    [SerializeField]
    private Text pageInfoTracker;
    [SerializeField]
    private Text policyName;
    [SerializeField] 
    private CanvasGroup agreementPanel;

    private int activePageIndex = 0; // Start from 0
    private int policiesCount = 0;
    private List<AccelByteAgreementLogic.PoliciesInfo> policies;

    private void Start()
    {
        nextButton.SetEnable(true);
        nextButton.getButton().onClick.RemoveAllListeners();
        nextButton.getButton().onClick.AddListener(OnNextButtonClicked);
        
        previousButton.SetEnable(true);
        previousButton.getButton().onClick.RemoveAllListeners();
        previousButton.getButton().onClick.AddListener(OnPreviousButtonClicked);

        RefreshButtonsActivity();
    }

    public void SetPolicyInformation(List<AccelByteAgreementLogic.PoliciesInfo> input)
    {
        policies = input;
        policiesCount = input.Count;
        Refresh();
    }

    public void ClearPolicyInformation()
    {
        policiesCount = 0;
        policies.Clear();
        Refresh();
    }
    
    public void Refresh()
    {
        RefreshButtonsActivity();
        if (policiesCount > 0)
        {
            pageInfoTracker.text = $"{activePageIndex + 1} out of {policiesCount}";
            RefreshTextViewer();
            policyName.text = policies[activePageIndex].policyName;
        }
        else
        {
            pageInfoTracker.text = "0 out of 0";
        }
    }
    
    private void RefreshButtonsActivity()
    {
        nextButton.SetEnable(!IsRightMostIndex());
        previousButton.SetEnable(!IsLeftMostIndex());
    }

    private void RefreshTextViewer()
    {
        agreementDocumentText.text = policies[activePageIndex].documentContent;
    }

    private void OnNextButtonClicked()
    {
        IncrementPageIndex();
        Refresh();
    }

    private bool IsRightMostIndex()
    {
        return activePageIndex == policiesCount - 1 || policiesCount <= 1;
    }

    private void OnPreviousButtonClicked()
    {
        DecrementPageIndex();
        Refresh();
    }

    private bool IsLeftMostIndex()
    {
        return activePageIndex == 0 || policiesCount <= 1;
    }

    private void IncrementPageIndex()
    {
        if (activePageIndex < policiesCount - 1)
        {
            activePageIndex += 1;
        }
    }

    private void DecrementPageIndex()
    {
        if (activePageIndex > 0)
        {
            activePageIndex -= 1;
        }
    }

    public void ShowPanel(bool show)
    {
        agreementPanel.gameObject.SetActive(show);
        agreementPanel.interactable = show;
        agreementPanel.blocksRaycasts = show;
        agreementPanel.alpha = show ? 1 : 0;
    }

    public Button GetAcceptAllButton()
    {
        return acceptButton.getButton();
    }
}
