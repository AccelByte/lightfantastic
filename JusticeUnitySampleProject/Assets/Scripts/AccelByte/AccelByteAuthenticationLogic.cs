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
#if UNITY_STANDALONE && !DISABLESTEAMWORKS
using Steamworks;
#endif
#if UNITY_STADIA
using UnityEngine.Stadia;
using Unity.StadiaWrapper;
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

#if UNITY_STANDALONE && !DISABLESTEAMWORKS
        [SerializeField]
        private SteamAuth steamAuth;
#endif
        public bool useSteam;
        public bool isUsingOtherPlatform;
        [SerializeField]
        private CommandLineArgs cmdLine;
        [SerializeField]
        private EosSDKAuth eosSdk;

        private AccelByteLobbyLogic abLobbyLogic;
        private AccelByteUserProfileLogic abUserProfileLogic;
        private AccelByteStatisticLogic abUserStatisticLogic;
        private AccelByteLeaderboardLogic abLeaderboardLogic;
        private AccelByteCloudSaveLogic abCloudSaveLogic;

        private E_LoginType loginType;

        private const string AUTHORIZATION_CODE_ENVIRONMENT_VARIABLE = "JUSTICE_AUTHORIZATION_CODE";

        void Awake()
        {
            abLobbyLogic = GetComponent<AccelByteLobbyLogic>();
            abUserProfileLogic = GetComponent<AccelByteUserProfileLogic>();
            abUserStatisticLogic = GetComponent<AccelByteStatisticLogic>();
            abLeaderboardLogic = GetComponent<AccelByteLeaderboardLogic>();
            abCloudSaveLogic = GetComponent<AccelByteCloudSaveLogic>();

            //Initialize AccelByte Plugin
            abUser = AccelBytePlugin.GetUser();

            useSteam = cmdLine.ParseCommandLine();
            eosSdk.OnSuccessLogin = LoginWithEpicGames;
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
            ControlOtherPlatformButton();
            AddEventListeners();
        }

        void AddEventListeners()
        {
            // Bind Buttons
            UIHandlerAuthComponent.loginButton.onClick.AddListener(Login);

            // Bind other platform Buttons
            Button[] buttons = UIHandlerAuthComponent.otherPlatformLoginButton;
            // 0 index = EpicGames Login
            buttons[0].onClick.AddListener(delegate {
                UIElementHandler.ShowLoadingPanel();
                eosSdk.LoginEpic();
            });
            
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

        void ControlOtherPlatformButton()
        {
            Button[] otherPlatformBtns = UIHandlerAuthComponent.otherPlatformLoginButton;
#if UNITY_EDITOR || UNITY_STANDALONE
            otherPlatformBtns[0].gameObject.SetActive(true);
#else
            otherPlatformBtns[0].gameObject.SetActive(false);
#endif
            bool hasActiveBtn = false;
            for (int i = 0; i < otherPlatformBtns.Length; i++)
            {
                if (otherPlatformBtns[i].gameObject.activeInHierarchy)
                {
                    hasActiveBtn = true;
                    break;
                }
            }
            UIHandlerAuthComponent.otherPlatformLoginText.gameObject.SetActive(hasActiveBtn);
        }
        #endregion // UI Listeners

        public void Start()
        {
            if (useSteam)
            {
#if UNITY_STANDALONE && !DISABLESTEAMWORKS
                isUsingOtherPlatform = true;
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
                Debug.Log("Don't USE STEAM");
#if UNITY_STANDALONE
                UIHandlerAuthComponent.gameObject.SetActive(true);
                // Try to login with launcher
                LoginWithLauncher();
#elif UNITY_STADIA
                UIHandlerAuthComponent.loginPanel.gameObject.SetActive(false);
                // Try to login with stadia
                Debug.Log("[LF] Login with stadia");
                LoginWithStadia();
                UIHandlerAuthComponent.mainMenuLogoutButton.gameObject.SetActive(false);
#endif
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
            isUsingOtherPlatform = true;

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

        //Attemps to login with Epic Games Account Portal
#if UNITY_EDITOR || UNITY_STANDALONE
        public void LoginWithEpicGames()
        {
            string accessToken = eosSdk.Token.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                Debug.Log("Epic Games token is null");
            }
            else
            {
                loginType = E_LoginType.EpicGames;
                abUser.LoginWithOtherPlatform(PlatformType.EpicGames, accessToken, OnLogin);
            }
        }
#endif

#if UNITY_STADIA
        //Attempts to login with stadia
        public void LoginWithStadia()
        {
            Debug.Log("[LF] LoginWithStadia Start");
            isUsingOtherPlatform = true;
            GgpPlayerId playerId = StadiaNativeApis.GgpGetPrimaryPlayerId();
            float startTime = Time.realtimeSinceStartup;
            UIElementHandler.ShowLoadingPanel();
            while (playerId.Value == (int)GgpIdConstants.kGgpInvalidId && Time.realtimeSinceStartup - startTime < 10f)
            {
                new WaitForSeconds(0.5f);
                playerId = StadiaNativeApis.GgpGetPrimaryPlayerId();
            }

            if (playerId.Value == (int)GgpIdConstants.kGgpInvalidId)
            {
                Debug.Log("[STADIA] Can't retrieve playerId!");
            }

            GgpStatus reqStatus;
            GgpPlayerJwt playerJwt = StadiaNativeApis.GgpGetJwtForPlayer(playerId, 1000, new GgpJwtFields((ulong)GgpJwtFieldValues.kGgpJwtField_None)).GetResultBlocking<GgpPlayerJwt>(out reqStatus);

            var user = AccelBytePlugin.GetUser();
            loginType = E_LoginType.Stadia;

            abUser.LoginWithOtherPlatform(AccelByte.Models.PlatformType.Stadia, playerJwt.jwt, OnLogin);
        }
#endif

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
                Debug.Log("[LF] OnLogin Failed");

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
                else if(loginType == E_LoginType.Stadia)
                {
                    ShowErrorMessage(true, "Can't login from stadia");
                }
                else if (loginType == E_LoginType.EpicGames)
                {
                    ShowErrorMessage(true, "Can't login from epic games.");
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
            Debug.Log("[LF] OnGetUserData Start");

            if (result.IsError)
            {
                Debug.Log("GetUserData failed:" + result.Error.Message);
                Debug.Log("GetUserData Response Code: " + result.Error.Code);
            }
            else if (!result.Value.eligible)
            {
                Debug.Log("[LF] OnGetUserData Not eligible proceeding to EULA");
                UIElementHandler.HideLoadingPanel();
                gameObject.GetComponent<AccelByteAgreementLogic>().GetUserPolicy();
            }
            else
            {
                Debug.Log("[LF] OnGetUserData success!");

                abUserData = result.Value;
                UIHandlerAuthComponent.displayName.text = "DisplayName: " + abUserData.displayName;
                UIHandlerAuthComponent.userId.text = "UserId: " + abUserData.userId;
                UIHandlerAuthComponent.sessionId.text = "SessionId: " + abUser.Session.AuthorizationToken;

                if (!abUserData.emailVerified && !isUsingOtherPlatform)
                {
                    Debug.Log("[LF] OnGetUserData emailVerified is false verify the email!");

                    UIElementHandler.HideNonExclusivePanel(NonExclusivePanelType.LOADING);
                    UIElementHandler.ShowExclusivePanel(ExclusivePanelType.VERIFY);
                }
                else
                {
                    //Progress to Main Menu
                    Debug.Log("[LF] OnGetUserData proceed to main menu");
                    if (!useSteam)
                    {
                        Debug.Log("[LF] OnGetUserData proceed to main menu");
                        UIElementHandler.HideLoadingPanel();
                    }
                    UIElementHandler.ShowExclusivePanel(ExclusivePanelType.MAIN_MENU);
                    UIElementHandler.ShowNonExclusivePanel(NonExclusivePanelType.PARENT_OF_OVERLAY_PANELS);
                    
                    abUserProfileLogic.Init();
                    abLeaderboardLogic.Init();
                    abCloudSaveLogic.GetUserAudioSettingRecord();
                    abUserStatisticLogic.UpdatePlayerStatisticUI();
                    abLobbyLogic.ConnectToLobby();
                }
            }
        }

        private void OnLogout(Result result)
        {
            if (result.IsError)
            {
                Debug.Log("Logout failed:" + result.Error.Message);
                Debug.Log("Logout Response Code: " + result.Error.Code);
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