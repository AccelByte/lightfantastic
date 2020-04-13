using System.Collections;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UITools;

public class AccelByteUserProfileLogic : MonoBehaviour
{
    private UserProfiles abUserProfiles;
    private UserProfile myProfile;
    private List<UserProfile> userProfilesCache = new List<UserProfile>();

    private GameObject UIHandler;
    private UIUserProfileLogicComponent UIHandlerUserProfileComponent;
    private UIElementHandler UIElementHandler;

    private bool isActionPhaseOver = false;

    void Start()
    {
        abUserProfiles = AccelBytePlugin.GetUserProfiles();
        AccelByteManager.Instance.LobbyLogic.onMatchOver += LobbyLogic_onMatchOver;
    }

    private void LobbyLogic_onMatchOver()
    {
        isActionPhaseOver = true;
    }

    public void Init()
    {
        // Create default user profile
        var defaultUserProfile = new CreateUserProfileRequest{language = LightFantasticConfig.DEFAULT_LANGUAGE};
        abUserProfiles.CreateUserProfile(defaultUserProfile, OnCreateUserProfile);

        // Update player profile info UI
        UIHandlerUserProfileComponent.PlayerProfilePrefab.GetComponent<PlayerStatusPrefab>().UpdatePlayerProfile();
    }

    #region UI Listeners
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshUIHandler();
    }

    public void RefreshUIHandler()
    {
        if (abUserProfiles == null) { abUserProfiles = AccelBytePlugin.GetUserProfiles(); }
        UIHandler = GameObject.FindGameObjectWithTag("UIHandler");
        if (UIHandler == null)
        {
            Debug.Log("ABUserProfile RefreshUIHandler no reference to UI Handler!");
            return;
        }
        UIHandlerUserProfileComponent = UIHandler.GetComponent<UIUserProfileLogicComponent>();
        UIElementHandler = UIHandler.GetComponent<UIElementHandler>();

        if (isActionPhaseOver)
        {
            UpdatePlayerProfileUI();
            isActionPhaseOver = false;
        }
    }
    #endregion // UI Listeners

    #region AccelByte User Profile Functions
    /// <summary>
    /// Get user profile of the current user that logged in
    /// </summary>
    /// <param name="onGetProfile"> Result callback function that has userprofile param </param>
    public void GetMine(ResultCallback<UserProfile> onGetProfile)
    {
        abUserProfiles.GetUserProfile(onGetProfile);
    }

    public void UpdatePlayerProfileUI()
    {
        UIHandlerUserProfileComponent.PlayerProfilePrefab.GetComponent<PlayerStatusPrefab>().UpdatePlayerProfile();
    }

    public UserProfile Get(string userId)
    {
        foreach (var item in userProfilesCache)
        {
            if (item.userId == userId)
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// Update call to user profile service
    /// </summary>
    /// <param name="request"> contains attributes that will need to be updated </param>
    /// <param name="callback"> result callback that will return the updated userprofile data </param>
    public void UpdateMine(UpdateUserProfileRequest request, ResultCallback<UserProfile> callback)
    {
        abUserProfiles.UpdateUserProfile(request, callback);
    }
    #endregion // AccelByte User Profile Callbacks

    #region AccelByte User Profile Callbacks
    /// <summary>
    /// Callback from get user profile
    /// </summary>
    /// <param name="result"></param>
    private void OnGetMyProfile(Result<UserProfile> result)
    {
        if (!result.IsError)
        {
            myProfile = result.Value;
        }
    }

    /// <summary>
    /// Callback on create user profile
    /// If exist already, then use the one that already exist
    /// </summary>
    /// <param name="result"></param>
    private void OnCreateUserProfile(Result<UserProfile> result)
    {
        if (!result.IsError)
        {
            myProfile = result.Value;
        }
        else if (result.Error.Code == ErrorCode.UserProfileConflict)
        {
            abUserProfiles.GetUserProfile(OnGetMyProfile);
        }
    }
    #endregion // AccelByte User Profile Callbacks
}
