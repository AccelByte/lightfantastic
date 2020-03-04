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
        abAgreement.GetPolicies(result =>
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

    public void FulfillEligibility()
    {
        uiElementHandler.ShowLoadingPanel();
        List<AcceptAgreementRequest> requests = new List<AcceptAgreementRequest>(policies.Count);
        foreach (var policyInfo in policies)
        {
            requests.Add(policyInfo.info);
        }
        abAgreement.SignUserLegalEligibilites(requests.ToArray(), OnFulfillEligibility);
    }

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
}
