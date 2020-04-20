// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

//Disables the warning messages generated from private [SerializeField]
#pragma warning disable 0649
using System.Globalization;
using UnityEngine;
using AccelByte.Api;
using AccelByte.Models;
using AccelByte.Core;
using UnityEngine.UI;
using UITools;
#if UNITY_STANDALONE
using Steamworks;
#endif
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ABRuntimeLogic
{
    public class AccelByteAuthenticationLogic : MonoBehaviour
    {
        private User abUser;
        private UserData abUserData;

        private GameObject UIHandler;
        private UIAuthLogicComponent UIHandlerAuthComponent;
        private UIElementHandler UIElementHandler;

#if UNITY_STANDALONE
        [SerializeField]
        private SteamAuth steamAuth;
#endif
        public bool useSteam;
        [SerializeField]
        private CommandLineArgs cmdLine;

        private AccelByteLobbyLogic abLobbyLogic;
        private AccelByteUserProfileLogic abUserProfileLogic;
        private AccelByteStatisticLogic abUserStatisticLogic;
        private AccelByteLeaderboardLogic abLeaderboardLogic;

        private E_LoginType loginType;

        private const string AUTHORIZATION_CODE_ENVIRONMENT_VARIABLE = "JUSTICE_AUTHORIZATION_CODE";

        void Awake()
        {
            abLobbyLogic = GetComponent<AccelByteLobbyLogic>();
            abUserProfileLogic = GetComponent<AccelByteUserProfileLogic>();
            abUserStatisticLogic = GetComponent<AccelByteStatisticLogic>();
            abLeaderboardLogic = GetComponent<AccelByteLeaderboardLogic>();

            //Initialize AccelByte Plugin
            abUser = AccelBytePlugin.GetUser();

            useSteam = cmdLine.ParseCommandLine();
        }

        #region UI Listeners
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (UIHandler != null)
            {
                RemoveListeners();
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshUIHandler();
        }

        public void RefreshUIHandler()
        {
            UIHandler = GameObject.FindGameObjectWithTag("UIHandler");
            if (UIHandler == null)
            {
                Debug.LogError("ABAuth RefreshUIHandler no reference to UI Handler!");
                return;
            }
            UIHandlerAuthComponent = UIHandler.GetComponent<UIAuthLogicComponent>();
            UIElementHandler = UIHandler.GetComponent<UIElementHandler>();

            List<Dropdown.OptionData> CountryObjectCacheToDropdown(CountryObject[] result)
            {
                var options = new List<Dropdown.OptionData>();
                foreach (var countryObject in result)
                {
                    options.Add(new Dropdown.OptionData(countryObject.name));
                }
                return options;
            }

            // Get list of countries cache
            if (AccelByteManager.Instance.countryObjectsCache == null)
            {
                var accelByteUserProfileLogic = AccelBytePlugin.GetUserProfiles();
                accelByteUserProfileLogic.GetCountries(result =>
                    {
                        AccelByteManager.Instance.countryObjectsCache = result.Value;
                        UIHandlerAuthComponent.registerCountryDropdown.options = CountryObjectCacheToDropdown(result.Value);
                    }
                );
            }
            else
            {
                UIHandlerAuthComponent.registerCountryDropdown.options = CountryObjectCacheToDropdown(AccelByteManager.Instance.countryObjectsCache);
            }
            
            AddEventListeners();
        }

        void AddEventListeners()
        {
            // Bind Buttons
            UIHandlerAuthComponent.loginButton.onClick.AddListener(Login);
            UIHandlerAuthComponent.registerButton.onClick.AddListener(Register);
            UIHandlerAuthComponent.verifyButton.onClick.AddListener(VerifyRegister);
            UIHandlerAuthComponent.resendVerificationButton.onClick.AddListener(ResendVerification);

            UIHandlerAuthComponent.fromRegister_BackToLoginButton.onClick.AddListener(delegate
            {
                UIElementHandler.ShowExclusivePanel(ExclusivePanelType.LOGIN);
            });
            
            UIHandlerAuthComponent.logoutButton.onClick.AddListener(Logout);
            
            UIHandlerAuthComponent.signUpButton.onClick.AddListener(delegate
            {
                UIElementHandler.ShowExclusivePanel(ExclusivePanelType.REGISTER);
            });
        }

        void RemoveListeners()
        {
            UIHandlerAuthComponent.loginButton.onClick.RemoveListener(Login);
            UIHandlerAuthComponent.registerButton.onClick.RemoveListener(Register);
            UIHandlerAuthComponent.verifyButton.onClick.RemoveListener(VerifyRegister);
            UIHandlerAuthComponent.resendVerificationButton.onClick.RemoveListener(ResendVerification);

            UIHandlerAuthComponent.logoutButton.onClick.RemoveListener(Logout);
        }
        #endregion // UI Listeners

        public void Start()
        {
            if (useSteam)
            {
#if UNITY_STANDALONE
                loginType = E_LoginType.Steam;
                UIHandlerAuthComponent.loginPanel.gameObject.SetActive(false);
                Debug.Log("Valid ABUSER:"+abUser.Session.IsValid());
                Debug.Log("Valid Steam Auth:" + steamAuth.isActiveAndEnabled);
                abUser.LoginWithOtherPlatform(PlatformType.Steam, steamAuth.GetSteamTicket(), OnLogin);
                Debug.Log("USE STEAM");
                UIHandlerAuthComponent.mainMenuLogoutButton.gameObject.SetActive(false);
#endif
            }
            else 
            {
                UIHandlerAuthComponent.gameObject.SetActive(true);

                Debug.Log("Don't USE STEAM");
                // Try to login with launcher
                LoginWithLauncher();
            }
        }

        #region AccelByte Authentication Functions
        
        /// <summary>
        /// Register's a new user with the information from the UIInputs
        /// Uses RegionInfo to get the two letter ISO Region Name
        /// </summary>
        public void Register()
        {
            System.DateTime dob = new System.DateTime(int.Parse(UIHandlerAuthComponent.registerDobYear.text), 
                int.Parse(UIHandlerAuthComponent.registerDobMonth.text), int.Parse(UIHandlerAuthComponent.registerDobDay.text));

            string country = AccelByteManager.Instance.countryObjectsCache[UIHandlerAuthComponent.registerCountryDropdown.value].code;
            UIHandlerAuthComponent.registerErrorText.text = " ";
            abUser.Register(UIHandlerAuthComponent.registerEmail.text, UIHandlerAuthComponent.registerPassword.text, UIHandlerAuthComponent.registerDisplayName.text, 
                country, dob, OnRegister);
        }

        //Sends the verification code to the backend server
        public void VerifyRegister()
        {
            abUser.Verify(UIHandlerAuthComponent.verificationCode.text, OnVerify);
        }

        //Tells the server to send another verification code to the user's email
        public void ResendVerification()
        {
            abUser.SendVerificationCode(OnResendVerification);
        }

        //Attempts to log the user in
        public void Login()
        {
            if (string.IsNullOrEmpty(UIHandlerAuthComponent.loginEmail.text))
            {
                if (string.IsNullOrEmpty(UIHandlerAuthComponent.loginPassword.text))
                    ShowErrorMessage(true, "Please fill your email address and password");
                else
                    ShowErrorMessage(true, "Please fill your email address");
            }
            else if (string.IsNullOrEmpty(UIHandlerAuthComponent.loginPassword.text))
            {
                ShowErrorMessage(true, "Please fill your password");
            }
            else
            {
                loginType = E_LoginType.Username;

                abUser.LoginWithUsername(UIHandlerAuthComponent.loginEmail.text, UIHandlerAuthComponent.loginPassword.text, OnLogin);
                UIElementHandler.ShowLoadingPanel();
            }
        }

        //Attempts to login with launcher
        public void LoginWithLauncher()
        {
            // Check if auth code is available from launcher
            string authCode = Environment.GetEnvironmentVariable(AUTHORIZATION_CODE_ENVIRONMENT_VARIABLE);

            if (authCode != null)
            {
                loginType = E_LoginType.Launcher;

                abUser.LoginWithLauncher(OnLogin);
                UIElementHandler.ShowLoadingPanel();
            }
            else
            {
                Debug.Log("LoginWithLauncher authCode is null");
            }
        }

        //Gets the user's top level account details
        public void GetUserDetails()
        {
            abUser.GetData(OnGetUserData);
        }

        //Logs the user out
        public void Logout()
        {
            AccelBytePlugin.GetLobby().Disconnect();
            abUser.Logout(OnLogout);
        }

        public UserData GetUserData()
        {
            return abUserData;
        }

        private void ShowErrorMessage(bool isEnable, string message = "")
        {
            if (isEnable)
            {
                UIHandlerAuthComponent.errorPanel.SetActive(true);
                UIHandlerAuthComponent.errorMessageText.text = message;
            }
            else
            {
                UIHandlerAuthComponent.errorPanel.SetActive(false);
            }
        }
        #endregion

        #region AccelByte Authentication Callbacks
        //Handles the Registration Response, continues to the Verification Panel on Success
        private void OnRegister(Result<RegisterUserResponse> result)
        {
            if (result.IsError)
            {
                UIHandlerAuthComponent.registerErrorText.text = result.Error.Message;
                Debug.Log("Register failed:" + result.Error.Message);
                Debug.Log("Register Response Code: " + result.Error.Code);
                //Show Error Message
            }
            else
            {
                Debug.Log("Register successful.");
                UIHandlerAuthComponent.loginEmail.text = UIHandlerAuthComponent.registerEmail.text;
                UIHandlerAuthComponent.loginPassword.text = UIHandlerAuthComponent.registerPassword.text;
                Login();
                //Show Verification Panel
                UIElementHandler.ShowExclusivePanel(ExclusivePanelType.REGISTER);
            }
        }

        //Handles Verification Response, continues to the Main Menu Panel on Success
        private void OnVerify(Result result)
        {
            if (result.IsError)
            {
                Debug.Log("Verification failed:" + result.Error.Message);
                Debug.Log("Verification Response Code: " + result.Error.Code);
                //Show Error Message
            }
            else
            {
                Debug.Log("Verification successful.");
                UIElementHandler.ShowExclusivePanel(ExclusivePanelType.MAIN_MENU);
                UIElementHandler.ShowNonExclusivePanel(NonExclusivePanelType.PARENT_OF_OVERLAY_PANELS);

                abUserProfileLogic.Init();
                abLeaderboardLogic.Init();
                abUserStatisticLogic.UpdatePlayerStatisticUI();
                abLobbyLogic.ConnectToLobby();
            }
        }

        //Handles Resend Verification Response
        private void OnResendVerification(Result result)
        {
            if (result.IsError)
            {
                UIElementHandler.HideLoadingPanel();

                Debug.Log("Resend Verification failed:" + result.Error.Message);
                Debug.Log("Resend Verification Response Code: " + result.Error.Code);
                //Show Error Message
            }
            else
            {
                Debug.Log("Resend successful.");
            }
        }

        //Handles User Login, on Success it will get the User's account details
        private void OnLogin(Result result)
        {
            if (result.IsError)
            {
                if (!useSteam)
                {
                    UIElementHandler.HideLoadingPanel();
                }

                Debug.Log("Login failed:" + result.Error.Message);
                Debug.Log("Login Response Code: " + result.Error.Code);
                //Show Error Message
                if (loginType == E_LoginType.Launcher)
                {
                    ShowErrorMessage(true, "Can't login from launcher.");
                }
                else if (loginType == E_LoginType.Username)
                {
                    ShowErrorMessage(true, "Incorrect email address or password.");
                }
                else if (loginType == E_LoginType.Steam)
                {
                    ShowErrorMessage(true, "Can't login from steam");
                }
            }
            else
            {
                Debug.Log("Login successful. Getting User Data");
                //Show Login Successful
                //Show "Getting User Details"
                GetUserDetails();
                ShowErrorMessage(false);
            }
        }

        //Handles Getting the user's data. On Success it prints the information to the console.
        //Also Checks if the user has verified their email address, if not, sends user to verification, else sends to main menu.
        private void OnGetUserData(Result<UserData> result)
        {
            if (result.IsError)
            {
                Debug.Log("GetUserData failed:" + result.Error.Message);
                Debug.Log("GetUserData Response Code: " + result.Error.Code);
            }
            else if (!result.Value.eligible)
            {
                UIElementHandler.HideLoadingPanel();
                gameObject.GetComponent<AccelByteAgreementLogic>().GetUserPolicy();
            }
            else
            {
                abUserData = result.Value;
                UIHandlerAuthComponent.displayName.text = "DisplayName: " + abUserData.displayName;
                UIHandlerAuthComponent.userId.text = "UserId: " + abUserData.userId;
                UIHandlerAuthComponent.sessionId.text = "SessionId: " + abUser.Session.AuthorizationToken;

                if (!abUserData.emailVerified && !useSteam)
                {
                    UIElementHandler.HideNonExclusivePanel(NonExclusivePanelType.LOADING);
                    UIElementHandler.ShowExclusivePanel(ExclusivePanelType.VERIFY);
                }
                else
                {
                    //Progress to Main Menu
                    if (!useSteam)
                    {
                        UIElementHandler.HideLoadingPanel();
                    }
                    UIElementHandler.ShowExclusivePanel(ExclusivePanelType.MAIN_MENU);
                    UIElementHandler.ShowNonExclusivePanel(NonExclusivePanelType.PARENT_OF_OVERLAY_PANELS);
                    
                    abUserProfileLogic.Init();
                    abLeaderboardLogic.Init();
                    abUserStatisticLogic.UpdatePlayerStatisticUI();
                    abLobbyLogic.ConnectToLobby();
                }
            }
        }

        private void OnLogout(Result result)
        {
            if (result.IsError)
            {
                Debug.Log("Login failed:" + result.Error.Message);
                Debug.Log("Login Response Code: " + result.Error.Code);
                //Show Error Message
            }
            else
            {
                UIElementHandler.ShowExclusivePanel(ExclusivePanelType.LOGIN);
                UIElementHandler.HideNonExclusivePanel(NonExclusivePanelType.PARENT_OF_OVERLAY_PANELS);
                UIHandlerAuthComponent.loginEmail.text = string.Empty;
                UIHandlerAuthComponent.loginPassword.text = string.Empty;
            }

            UIElementHandler.HideLoadingPanel();
        }
        #endregion

    }
}