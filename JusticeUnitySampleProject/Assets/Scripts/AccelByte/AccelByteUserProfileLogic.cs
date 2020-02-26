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
        var defaultUserProfile = new CreateUserProfileRequest{language = LightFantasticConfig.DEFAULT_LANGUAGE};
        abUserProfiles.CreateUserProfile(defaultUserProfile, OnCreateUserProfile);

        // Update player profile info
        UIHandlerUserProfileComponent.PlayerProfilePrefab.GetComponent<PlayerStatusPrefab>().UpdatePlayerProfile();
    }
    
    public void GetMine(ResultCallback<UserProfile> onGetProfile)
    {
        abUserProfiles.GetUserProfile(onGetProfile);
    }

    public void UpdatePlayerProfileUI()
    {
        UIHandlerUserProfileComponent.PlayerProfilePrefab.GetComponent<PlayerStatusPrefab>().UpdatePlayerProfile();
    }

    #region UI Listeners
    void OnEnable()
    {
        Debug.Log("ABUserProfile OnEnable called!");

        // Register to onsceneloaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        Debug.Log("ABUserProfile OnDisable called!");

        // Register to onsceneloaded
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (UIHandler != null)
        {
            RemoveListeners();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("ABUserProfile OnSceneLoaded level loaded!");

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

        AddEventListeners();

        if (isActionPhaseOver)
        {
            UpdatePlayerProfileUI();
            isActionPhaseOver = false;
        }
    }

    void AddEventListeners()
    {
        Debug.Log("ABUserProfile AddEventListeners!");
        // Bind Buttons
    }

    void RemoveListeners()
    {
        Debug.Log("ABUserProfile RemoveListeners!");
    }
    #endregion // UI Listeners

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
        //TODO: Get it from backend and then store it in the userProfilesCache
        //abUserProfiles.GetUserProfileById(userId, result =>
        //{
        //    
        //});
    }

    public void UpdateMine(UpdateUserProfileRequest request, ResultCallback<UserProfile> callback)
    {
        abUserProfiles.UpdateUserProfile(request, callback);
    }

    private void OnGetMyProfile(Result<UserProfile> result)
    {
        if (!result.IsError)
        {
            myProfile = result.Value;
        }
    }

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

    private void UpdatePlayerProfile()
    {
        //TODO: update player profile status
    }
}
