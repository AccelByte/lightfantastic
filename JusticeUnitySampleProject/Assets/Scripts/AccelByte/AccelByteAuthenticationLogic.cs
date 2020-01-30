//Disables the warning messages generated from private [SerializeField]
#pragma warning disable 0649
using System.Globalization;
using UnityEngine;
using AccelByte.Api;
using AccelByte.Models;
using AccelByte.Core;
using UnityEngine.UI;
using UITools;
using Steamworks;
using System;
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

        [SerializeField]
        private SteamAuth steamAuth;
        public bool useSteam;
        [SerializeField]
        private CommandLineArgs cmdLine;

        private AccelByteLobbyLogic abLobbyLogic;
        private AccelByteGameProfileLogic abGameProfileLogic;
        private AccelByteUserProfileLogic abUserProfileLogic;

        private const string AUTHORIZATION_CODE_ENVIRONMENT_VARIABLE = "JUSTICE_AUTHORIZATION_CODE";

        void Awake()
        {
            abLobbyLogic = GetComponent<AccelByteLobbyLogic>();
            abGameProfileLogic = GetComponent<AccelByteGameProfileLogic>();
            abUserProfileLogic = GetComponent<AccelByteUserProfileLogic>();

            //Initialize AccelByte Plugin
            abUser = AccelBytePlugin.GetUser();

            useSteam = cmdLine.ParseCommandLine();
        }

        #region UI Listeners
        void OnEnable()
        {
            Debug.Log("ABAuth OnEnable called!");

            // Register to onsceneloaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            Debug.Log("ABAuth OnDisable called!");

            // Register to onsceneloaded
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (UIHandler != null)
            {
                RemoveListeners();
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("ABAuth OnSceneLoaded level loaded!");

            RefreshUIHandler();
        }

        public void RefreshUIHandler()
        {
            UIHandler = GameObject.FindGameObjectWithTag("UIHandler");
            if (UIHandler == null)
            {
                Debug.Log("ABAuth RefreshUIHandler no reference to UI Handler!");
                return;
            }
            UIHandlerAuthComponent = UIHandler.GetComponent<UIAuthLogicComponent>();
            UIElementHandler = UIHandler.GetComponent<UIElementHandler>();

            AddEventListeners();
        }

        void AddEventListeners()
        {
            Debug.Log("ABAuth AddEventListeners!");
            // Bind Buttons
            UIHandlerAuthComponent.loginButton.onClick.AddListener(Login);
            UIHandlerAuthComponent.registerButton.onClick.AddListener(Register);
            UIHandlerAuthComponent.verifyButton.onClick.AddListener(VerifyRegister);
            UIHandlerAuthComponent.resendVerificationButton.onClick.AddListener(ResendVerification);
            UIHandlerAuthComponent.logoutButton.onClick.AddListener(Logout);
        }

        void RemoveListeners()
        {
            Debug.Log("ABAuth RemoveListeners!");
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
                UIHandlerAuthComponent.loginPanel.gameObject.SetActive(false);
                abUser.LoginWithOtherPlatform(PlatformType.Steam, steamAuth.GetSteamTicket(), OnLogin);
                Debug.Log("USE STEAM");
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
        //Register's a new user with the information from the UIInputs
        //Uses RegionInfo to get the two letter ISO Region Name
        public void Register()
        {
            System.DateTime dob = new System.DateTime(int.Parse(UIHandlerAuthComponent.registerDobYear.text), 
                int.Parse(UIHandlerAuthComponent.registerDobMonth.text), int.Parse(UIHandlerAuthComponent.registerDobDay.text));

            abUser.Register(UIHandlerAuthComponent.registerEmail.text, UIHandlerAuthComponent.registerPassword.text, UIHandlerAuthComponent.registerDisplayName.text, 
                RegionInfo.CurrentRegion.TwoLetterISORegionName, dob, OnRegister);
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
            abUser.LoginWithUsername(UIHandlerAuthComponent.loginEmail.text, UIHandlerAuthComponent.loginPassword.text, OnLogin);

            UIElementHandler.FadeLoading();
        }

        //Attempts to login with launcher
        public void LoginWithLauncher()
        {
            // Check if auth code is available from launcher
            string authCode = Environment.GetEnvironmentVariable(AUTHORIZATION_CODE_ENVIRONMENT_VARIABLE);

            if (authCode != null)
            {
                abUser.LoginWithLauncher(OnLogin);
                UIElementHandler.FadeLoading();
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
            abUser.Logout(OnLogout);
        }

        public UserData GetUserData()
        {
            return abUserData;
        }
        #endregion

        #region AccelByte Authentication Callbacks
        //Handles the Registration Response, continues to the Verification Panel on Success
        private void OnRegister(Result<RegisterUserResponse> result)
        {
            if (result.IsError)
            {
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
                UIElementHandler.FadeRegister();
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
                UIElementHandler.FadeVerify();
                UIElementHandler.FadePersistentFriends();
                UIElementHandler.FadeMenu();
                abUserProfileLogic.Init();
                abLobbyLogic.ConnectToLobby();
            }
        }

        //Handles Resend Verification Response
        private void OnResendVerification(Result result)
        {
            if (result.IsError)
            {
                UIElementHandler.FadeLoading();

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
                    UIElementHandler.FadeLoading();
                }

                Debug.Log("Login failed:" + result.Error.Message);
                Debug.Log("Login Response Code: " + result.Error.Code);
                //Show Error Message
            }
            else
            {
                Debug.Log("Login successful. Getting User Data");
                //Show Login Successful
                //Show "Getting User Details"
                GetUserDetails();
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
                UIElementHandler.FadeLoading();// hide
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
                    UIElementHandler.FadeLoading();// hide
                    UIElementHandler.FadeVerify();
                }
                else
                {
                    //Progress to Main Menu
                    if (!useSteam)
                    {
                        UIElementHandler.FadeLoading();
                    }
                    UIElementHandler.FadeLogin();
                    UIElementHandler.FadePersistentFriends();
                    UIElementHandler.FadeMenu();
                    abUserProfileLogic.Init();
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
                UIElementHandler.FadeCurrent();
                UIElementHandler.FadeLogin();
                UIElementHandler.FadePersistentFriends();
                UIHandlerAuthComponent.loginEmail.text = string.Empty;
                UIHandlerAuthComponent.loginPassword.text = string.Empty;
            }
        }
        #endregion

    }
}