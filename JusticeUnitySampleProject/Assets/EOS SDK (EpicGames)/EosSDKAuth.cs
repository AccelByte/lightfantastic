// Copyright (c) 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using ABRuntimeLogic;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Platform;
using UnityEngine;

public class EosSDKAuth : MonoBehaviour
{
    // this data obtained from AccelByte's Epic Dev Portal, project name "Test"
    private string productName = "Test";
    private string productVersion = "0.1";
    private string productId = "e1cb9512acd148a2b804b0a1f75e7c83";
    private string sandboxId = "5e74a0e4dde243babbc5736719ebdc24";
    private string deployId = "a3571065a6104a2190fe74f47d3ff6c9";
    private string clientId = "xyza7891BtxBFeBZLz9EiPX4xy2Kmc88";
    private string clientSecret = "xNdNkKUvc0T4hGAP4iYdrWmO4fo7f4kSbUjcRWbL+aQ";

    private PlatformInterface platformInterface;
    private const float platformTickInterval = 0.2f;
    private float platformTickTimer = 0f;

    private Token token;
    public Token Token { get { return token; } private set { } }

    public LoginCredentialType loginCredentialType;
    public string loginCredId;
    public string loginCredToken;

    private void Start()
    {
        InitEOSSDK();
    }

    private void InitEOSSDK()
    {
        var initOptions = new InitializeOptions()
        {
            ProductName = productName,
            ProductVersion = productVersion
        };

        var initResult = PlatformInterface.Initialize(initOptions);

        bool isAlreadyeConfigured = Application.isEditor && initResult == Result.AlreadyConfigured;
        if(initResult != Result.Success && !isAlreadyeConfigured)
        {
            Debug.Log("[EOS SDK] Init Failed. Code : " + initResult);
            return;
        }

        Options option = new Options()
        {
            ProductId = productId,
            SandboxId = sandboxId,
            DeploymentId = deployId,
            ClientCredentials = new ClientCredentials()
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            }
        };

        platformInterface = PlatformInterface.Create(option);
        if (platformInterface == null)
        {
            Debug.Log("[EOS SDK] Failed to create platform. ");
        }
    }

    public void LoginEpic()
    {
        var loginOption = new LoginOptions()
        {
            Credentials = new Credentials()
            {
                Type = loginCredentialType,
                Id = loginCredId,
                Token = loginCredToken
            }
        };
        platformInterface.GetAuthInterface().Login(loginOption, null, LoginEpicCallback);
    }

    private void LoginEpicCallback(LoginCallbackInfo info)
    {
        Result result = info.ResultCode;
        if(result == Result.Success)
        {
            Debug.Log("[EOS SDK] Login Succeeded.");
            CopyUserAuthTokenOptions copyOpt = new CopyUserAuthTokenOptions();
            platformInterface.GetAuthInterface().CopyUserAuthToken(copyOpt, info.LocalUserId, out token);
        }
        else
        {
            Debug.Log("[EOS SDK] Login Failed. Result : " + result);
        }
    }

    // Calling tick on a regular interval is required for callbacks to work.
    private void TickCallback()
    {
        if (platformInterface == null)
            return;
        platformTickTimer += Time.deltaTime;
        if(platformTickTimer >= platformTickInterval)
        {
            platformTickTimer = 0;
            platformInterface.Tick();
        }
    }

    private void Update()
    {
        TickCallback();
    }

    private void OnDestroy()
    {
        if(!Application.isEditor && platformInterface != null)
        {
            platformInterface.Release();
            platformInterface = null;
            PlatformInterface.Shutdown();
        }
    }
}
