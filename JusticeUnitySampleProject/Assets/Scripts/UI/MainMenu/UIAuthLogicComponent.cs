// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIAuthLogicComponent : MonoBehaviour
{
    [System.Serializable]
    public enum OtherPlatformLoginType
    {
        EpicGames,
        Steam,
        Google,
        Facebook
    }

    [System.Serializable]
    public struct OtherPlatformLogin
    {
        public OtherPlatformLoginType LoginType;
        public Button LoginButton;
    }

    #region Register UI Fields
    public InputField registerEmail;
    public InputField registerDisplayName;
    public InputField registerPassword;
    public InputField registerConfirmPassword;
    public InputField registerDobDay;
    public InputField registerDobMonth;
    public InputField registerDobYear;
    public Dropdown registerCountryDropdown;
    public Text registerErrorText;
    #endregion

    #region Verify UI Fields
    public InputField verificationCode;
    #endregion

    #region Login UI Fields
    public InputField loginEmail;
    public InputField loginPassword;

    public GameObject errorPanel;
    public Text errorMessageText;
    #endregion

    #region Debug UI Fields
    public Text displayName;
    public Text userId;
    public Text sessionId;
    #endregion

    public GameObject loginPanel;

    #region Buttons
    public Button loginButton;
    public Text otherPlatformLoginText;
    public OtherPlatformLogin[] otherPlatformLogins;
    public Button signUpButton;
    public Button registerButton;
    public Button fromRegister_BackToLoginButton;
    public Button verifyButton;
    public Button resendVerificationButton;
    public Button logoutButton;
    public Button mainMenuLogoutButton;
    #endregion

    public void AddOtherPlatformLoginListener(OtherPlatformLoginType type, UnityAction act)
    {
        foreach (var item in otherPlatformLogins)
        {
            if(item.LoginType == type)
                item.LoginButton.onClick.AddListener(act);
        }
    }

    public void RemoveOtherPlatformLoginListener()
    {
        foreach (var item in otherPlatformLogins)
        {
            item.LoginButton.onClick.RemoveAllListeners();
        }
    }

    public void SetVisibleOtherPlatformLogin(OtherPlatformLoginType type, bool isVisible)
    {
        foreach (var item in otherPlatformLogins)
        {
            if(item.LoginType == type)
                item.LoginButton.gameObject.SetActive(isVisible);
        }
    }
}
