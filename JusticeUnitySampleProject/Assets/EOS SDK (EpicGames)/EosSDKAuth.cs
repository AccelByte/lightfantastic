// Copyright (c) 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using ABRuntimeLogic;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Platform;
using System;
using UnityEngine;

public class EosSDKAuth : MonoBehaviour
{
    // this data obtained from AccelByte's Epic Dev Portal, project name "Test"
    private string productName = "Light Fantastic";
    private string productVersion = "0.10";
    private string productId = "3a8112abdb0d47d3b8c3797ac5e1bb7a";
    private string sandboxId = "79af5e809cba48468ddafd1fe99298e2";
    private string deployId = "d498322002d34da4af587494c52af9ba";
    private string clientId = "xyza7891VgNARHnsLvUE9i7h0d7nZ6mX";
    private string clientSecret = "zcFtzs/Q3EkimAqX9PP5NLcj6RVcHcjJ489KuoM/4Fk";

    private PlatformInterface platformInterface;
    private const float platformTickInterval = 0.2f;
    private float platformTickTimer = 0f;

    private Token token;
    public Token Token { get { return token; } private set { } }

    public LoginCredentialType loginCredentialType;
    public string loginCredId;
    public string loginCredToken;

    private Action onSuccessLogin;
    public Action OnSuccessLogin { set { onSuccessLogin = value; } }

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
            onSuccessLogin?.Invoke();
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
