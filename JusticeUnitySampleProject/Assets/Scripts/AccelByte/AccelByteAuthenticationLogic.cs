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

namespace ABRuntimeLogic
{
    public class AccelByteAuthenticationLogic : MonoBehaviour
    {
        private User abUser;
        private UserData abUserData;
        
        #region Register UI Fields
        [SerializeField]
        private InputField registerEmail;
        [SerializeField]
        private InputField registerDisplayName;
        [SerializeField]
        private InputField registerPassword;
        [SerializeField]
        private InputField registerConfirmPassword;
        [SerializeField]
        private InputField registerDobDay;
        [SerializeField]
        private InputField registerDobMonth;
        [SerializeField]
        private InputField registerDobYear;
        #endregion

        #region Verify UI Fields
        [SerializeField]
        private InputField verificationCode;
        #endregion

        #region Login UI Fields
        [SerializeField]
        private InputField loginEmail;
        [SerializeField]
        private InputField loginPassword;
        #endregion

        #region Debug UI Fields
        [SerializeField]
        private Text displayName;
        [SerializeField]
        private Text userId;
        [SerializeField]
        private Text sessionId;
        #endregion

        [SerializeField]
        private SteamAuth steamAuth;
        public bool useSteam;
        [SerializeField]
        private CommandLineArgs cmdLine;

        private AccelByteLobbyLogic abLobbyLogic;
        [SerializeField]
        private GameObject loginPanel;
        private UIElementHandler uiHandler;


        void Awake()
        {
            abLobbyLogic = GetComponent<AccelByteLobbyLogic>();
            uiHandler = GetComponent<UIElementHandler>();
            //Initialize AccelByte Plugin
            abUser = AccelBytePlugin.GetUser();

            useSteam = cmdLine.ParseCommandLine();
        }

        public void Start()
        {
            if (useSteam)
            {
                loginPanel.gameObject.SetActive(false);
                abUser.LoginWithOtherPlatform(PlatformType.Steam, steamAuth.GetSteamTicket(), OnLogin);
                Debug.Log("USE STEAM");
            }
            else 
            {
                loginPanel.gameObject.SetActive(true);

                Debug.Log("Don't USE STEAM");
            }
        }

        #region AccelByte Authentication Functions
        //Register's a new user with the information from the UIInputs
        //Uses RegionInfo to get the two letter ISO Region Name
        public void Register()
        {
            System.DateTime dob = new System.DateTime(int.Parse(registerDobYear.text), 
                int.Parse(registerDobMonth.text), int.Parse(registerDobDay.text));

            abUser.Register(registerEmail.text, registerPassword.text, registerDisplayName.text, 
                RegionInfo.CurrentRegion.TwoLetterISORegionName, dob, OnRegister);
        }

        //Sends the verification code to the backend server
        public void VerifyRegister()
        {
            abUser.Verify(verificationCode.text, OnVerify);
        }

        //Tells the server to send another verification code to the user's email
        public void ResendVerification()
        {
            abUser.SendVerificationCode(OnResendVerification);
        }

        //Attempts to log the user in
        public void Login()
        {
            abUser.LoginWithUsername(loginEmail.text, loginPassword.text, OnLogin);
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
                abUser.LoginWithUsername(registerEmail.text, registerPassword.text, OnLogin);
                //Show Verification Panel
                uiHandler.FadeRegister();
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
                uiHandler.FadeVerify();
                uiHandler.FadePersistentFriends();
                uiHandler.FadeMenu();
                abLobbyLogic.ConnectToLobby();
            }
        }

        //Handles Resend Verification Response
        private void OnResendVerification(Result result)
        {
            if (result.IsError)
            {
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
            else
            {
                abUserData = result.Value;
                displayName.text = "DisplayName: " + abUserData.displayName;
                userId.text = "UserId: " + abUserData.userId;
                sessionId.text = "SessionId: " + abUser.Session.AuthorizationToken;

                if (!abUserData.emailVerified && !useSteam)
                {
                    uiHandler.FadeVerify();
                }
                else
                {
                    //Progress to Main Menu
                    uiHandler.FadeLogin();
                    uiHandler.FadePersistentFriends();
                    uiHandler.FadeMenu();
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
                uiHandler.FadeCurrent();
                uiHandler.FadeLogin();
                loginEmail.text = string.Empty;
                loginPassword.text = string.Empty;
            }
        }
        #endregion

    }
}