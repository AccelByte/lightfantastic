﻿// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

#pragma warning disable 0649

using System;
using System.Collections.Generic;
using ABRuntimeLogic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UITools;
using UnityEngine;

public class AccelByteAgreementLogic : MonoBehaviour
{
    private UiUtilities uiUtilities;
    private UIAgreementLogicComponent uiAgreement;
    private UIElementHandler uiElementHandler;
    private Agreement abAgreement;
    private List<PoliciesInfo> policies = new List<PoliciesInfo>(0);

    public class PoliciesInfo
    {
        public AcceptAgreementRequest info;
        public string documentContent;
        public string policyName;
    }
    
    void Awake()
    {
        uiUtilities = gameObject.GetComponent<UiUtilities>();
        uiElementHandler = GameObject.FindGameObjectWithTag("UIHandler").GetComponent<UIElementHandler>();
        
        uiAgreement = uiElementHandler.GetComponent<UIAgreementLogicComponent>();
        abAgreement = AccelBytePlugin.GetAgreement();
        
        uiAgreement.GetAcceptAllButton().onClick.RemoveAllListeners();
        uiAgreement.GetAcceptAllButton().onClick.AddListener(FulfillEligibility);
    }

    #region AccelByte Agreement Functions
    /// <summary>
    /// Use this function to get the user agreements that are available
    /// both mandatory and optional user agreements are included
    /// </summary>
    public void GetUserPolicy()
    {
        GetUserPolicy(() =>
        {
            uiAgreement.SetPolicyInformation(policies);
        });
        uiAgreement.ShowPanel(true);
    }
    
    private void GetUserPolicy(Action action)
    {
        abAgreement.GetLegalPolicies(AgreementPolicyType.LEGAL_DOCUMENT_TYPE, false, result =>
        {
            if (result.IsError)
            {
                // handle error
                action();
            }
            else
            {
                foreach (var entry in result.Value)
                {
                    if (entry.isMandatory)
                    {
                        foreach (var entryPolicyVersion in entry.policyVersions)
                        {
                            foreach (var entryLocal in entryPolicyVersion.localizedPolicyVersions)
                            {
                                var currentPolicy = new PoliciesInfo();
                                string docBucketPath = entryLocal.attachmentLocation;
                                
                                AcceptAgreementRequest documentInfo = new AcceptAgreementRequest();
                                documentInfo.policyId = entry.id;
                                documentInfo.policyVersionId = entryPolicyVersion.id;
                                documentInfo.localizedPolicyVersionId = entryLocal.id;
                                documentInfo.isAccepted = true;

                                currentPolicy.info = documentInfo;
                                currentPolicy.policyName = entry.policyName;
                                uiUtilities.GetText(docBucketPath, textResult =>
                                {
                                    currentPolicy.documentContent = textResult;
                                    uiAgreement.Refresh();
                                });
                                policies.Add(currentPolicy);
                            }
                        }
                    }
                }
                action();
            }
        });
    }

    /// <summary>
    /// Accept & sign all the user agreements available from GetUserPolicy
    /// </summary>
    public void FulfillEligibility()
    {
        uiElementHandler.ShowLoadingPanel();
        List<AcceptAgreementRequest> requests = new List<AcceptAgreementRequest>(policies.Count);
        foreach (var policyInfo in policies)
        {
            requests.Add(policyInfo.info);
        }
        abAgreement.BulkAcceptPolicyVersions(requests.ToArray(), OnFulfillEligibility);
    }
    #endregion // AccelByte Agreement Functions

    #region AccelByte Agreement Callbacks
    /// <summary>
    /// Use callback from accept agreement response to re-login to the game
    /// </summary>
    /// <param name="agreementResult"> Callback result from accept agreement response </param>
    private void OnFulfillEligibility(Result<AcceptAgreementResponse> agreementResult)
    {
        if (agreementResult.IsError)
        {
            uiAgreement.ShowPanel(false);
        }
        else
        {
            uiAgreement.ShowPanel(false);
            policies.Clear();
            uiAgreement.ClearPolicyInformation();
            
            // Need to re-login to refresh sesion.
            // New session required to obtain latest user data. Then, the user is eligible.
            gameObject.GetComponent<AccelByteAuthenticationLogic>().Login();
        }
        uiElementHandler.HideLoadingPanel();
    }
    #endregion // AccelByte Agreement Callbacks
}
