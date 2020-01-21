using System.Collections;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class AccelByteUserProfileLogic : MonoBehaviour
{
    private UserProfiles abUserProfiles;
    private UserProfile myProfile;
    private List<UserProfile> userProfilesCache = new List<UserProfile>();
    [SerializeField]
    private Transform PlayerProfilePrefab;
    void Start()
    {
        abUserProfiles = AccelBytePlugin.GetUserProfiles();
    }
    
    public void Init()
    {
        var defaultUserProfile = new CreateUserProfileRequest{language = LightFantasticConfig.DEFAULT_LANGUAGE};
        abUserProfiles.CreateUserProfile(defaultUserProfile, OnCreateUserProfile);

        // Update player profile info
        PlayerProfilePrefab.GetComponent<PlayerStatusPrefab>().UpdatePlayerProfile();
    }
    
    public void GetMine(ResultCallback<UserProfile> onGetProfile)
    {
        abUserProfiles.GetUserProfile(onGetProfile);
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
