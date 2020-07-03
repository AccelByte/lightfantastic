// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
#if !UNITY_EDITOR 
    using System.Linq;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using AccelByte.Server;
using AccelByte.Core;
using AccelByte.Models;

public class AccelByteServerLogic : MonoBehaviour
{
    private static AccelByteServerLogic instance;
    public static AccelByteServerLogic Instance { get { return instance; } }
    public delegate void OnServerRegisteredEvent();
    public event OnServerRegisteredEvent onServerRegistered;

    public delegate void OnServerFailureEvent();
    public event OnServerFailureEvent onServerFailure;

    private DedicatedServer abServer;
    private DedicatedServerManager abServerManager;
    private ServerStatistic abServerStatistic;

    public bool isLocal = true;

    /// <summary>
    /// IP that used in Local gameplay using local game server
    /// </summary>
    [SerializeField]
    private string ipAddress = "127.0.0.1";

    /// <summary>
    /// Port that used in Local gameplay local game server
    /// </summary>
    [SerializeField]
    private string port = "15937";
    public string LocalDSName { get; set; }
    private string mainMenuSceneName;

    public delegate void OnServerGetMatchRequestEvent(int playerCount);
    public event OnServerGetMatchRequestEvent onServerGetMatchRequest;

    private MatchRequest currentMatchmakingRequest = null;
    private int playerCount = 0;
    private int playerStatUpdatedCount = 0;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        abServer = AccelByteServerPlugin.GetDedicatedServer();
        abServerManager = AccelByteServerPlugin.GetDedicatedServerManager();
        abServerStatistic = AccelByteServerPlugin.GetStatistic();

        // Log the game server in to the accelbyte's IAM service
        abServer.LoginWithClientCredentials(OnLogin);
        mainMenuSceneName = SceneManager.GetActiveScene().name;
        SceneManager.sceneUnloaded += OnCurrentSceneUnloaded;

        // Register to get match request info, 
        // this will be triggered when the Dedicated Server Manager (DSM) decide if this game server will be used immediately
        abServerManager.OnMatchRequest += OnMatchRequest;

        #if !UNITY_EDITOR 
        isLocal = Environment.GetCommandLineArgs().Contains(LightFantasticConfig.DS_LOCALMODE_CMD_ARG);
        #endif
    }

    #region AccelByte Server Functions
    /// <summary>
    /// This environment variable will be set by DSM service,
    /// during the creation of game server
    /// </summary>
    /// <returns></returns>
    private string GetPodName()
    {
        return Environment.GetEnvironmentVariable("POD_NAME");
    }

    /// <summary>
    /// Callback from login to IAM service
    /// Decide what todo if the server is failed  or success to register
    /// </summary>
    /// <param name="result"></param>
    private void OnLogin(Result result)
    {
        Debug.Log("[AccelByteServerLogic] OnRegistered");
        if (result.IsError)
        {
            Debug.Log("[AccelByteServerLogic] OnRegistered failed:" + result.Error.Message);
            Debug.Log("[AccelByteServerLogic] OnRegistered Response Code: " + result.Error.Code);
            onServerFailure?.Invoke();
        }
        else
        {
            Debug.Log("[AccelByteServerLogic] OnRegistered Success! Is Local DS: " + isLocal);

            // Configure heartbeat to keep the DSM service know that the game server is alive
            abServerManager.ConfigureHeartBeat();
            if (isLocal)
            {
                // if the game server is running locally then notify the DSM service that this is local server
                Debug.Log("[AccelByteServerLogic] Local DS IP: " + ipAddress + " Port: " + port);
                abServerManager.RegisterLocalServer(ipAddress, ushort.Parse(port), LocalDSName, OnRegistered);
            }
            else
            {
                // otherwise, user register server method to notify the DSM servic that this is local server
                abServerManager.RegisterServer(ushort.Parse(port), OnRegistered);
            }
        }
    }

    /// <summary>
    /// Callback from register to DSM service
    /// from here the game server can be monitored from Admin portal
    /// </summary>
    /// <param name="result"></param>
    private void OnRegistered(Result result)
    {
        Debug.Log("[AccelByteServerLogic] OnRegisterServer");
        if (result.IsError)
        {
            Debug.Log("[AccelByteServerLogic] OnRegisterServer failed:" + result.Error.Message);
            Debug.Log("[AccelByteServerLogic] OnRegisterServer Response Code: " + result.Error.Code);
            onServerFailure?.Invoke();
        }
        else
        {
            Debug.Log("[AccelByteServerLogic] OnRegisterServer success, start Hosting");
            onServerRegistered?.Invoke();
        }
    }

    private void OnCurrentSceneUnloaded(Scene scene)
    {
        Debug.Log("Unloading Scene: " + scene.name);
        if (scene.name != mainMenuSceneName)
        {
            // Turned the server of when unloaded gameplay scene
            DeregisterServer();
        }
    }

    private void DestroySelf()
    {
        instance = null;
        Destroy(gameObject);
        Application.Quit(0);
    }

    /// <summary>
    /// Call on DeregisterLocalServer to deactivate local game server
    /// Call on ShutdownServer to deactivate online game server
    /// </summary>
    private void DeregisterServer()
    {
        if (isLocal)
        {
            abServerManager.DeregisterLocalServer(OnDeregister);
        }
        else
        {
            abServerManager.ShutdownServer(true, OnDeregister);
        }
    }

    private void OnApplicationQuit()
    {
        DeregisterServer();
    }

    /// <summary>
    /// Callback from deactivation request to DSM service
    /// Shutdown the game server on the end
    /// </summary>
    /// <param name="result"></param>
    private void OnDeregister(Result result)
    {
        Debug.Log("[AccelByteServerLogic] OnDeregister");
        if (result.IsError)
        {
            Debug.Log("[AccelByteServerLogic] OnDeregister failed:" + result.Error.Message);
            Debug.Log("[AccelByteServerLogic] OnDeregister Response Code: " + result.Error.Code);
        }
        else
        {
            Debug.Log("[AccelByteServerLogic] OnDeregister Success! Shutting down");
        }
        DestroySelf();
    }

    public int GetPlayerCount()
    {
        return playerCount;
    }

    public MatchRequest GetMatchRequest()
    {
        return currentMatchmakingRequest;
    }

    /// <summary>
    /// Update each player's user statistics to statistics service
    /// </summary>
    /// <param name="distance"> the player's distance traveled on the current match </param>
    /// <param name="userId"> the user id of the player </param>
    /// <param name="isWinner"> the winner in the current match </param>
    public void UpdateUserStatItem(float distance, string userId, bool isWinner)
    {
        if (playerStatUpdatedCount < playerCount)
        {
            if (distance > LightFantasticConfig.FINISH_LINE_DISTANCE)
            {
                distance = LightFantasticConfig.FINISH_LINE_DISTANCE;
            }

            var createStatItemRequest = new CreateStatItemRequest[4];
            createStatItemRequest[0] = new CreateStatItemRequest() { statCode = LightFantasticConfig.StatisticCode.total };
            createStatItemRequest[1] = new CreateStatItemRequest() { statCode = LightFantasticConfig.StatisticCode.win };
            createStatItemRequest[2] = new CreateStatItemRequest() { statCode = LightFantasticConfig.StatisticCode.lose };
            createStatItemRequest[3] = new CreateStatItemRequest() { statCode = LightFantasticConfig.StatisticCode.distance };

            abServerStatistic.CreateUserStatItems(userId, createStatItemRequest, OnCreateUserStatItems);

            var statItemOperationResult = new StatItemIncrement[3];
            //Update total-match
            statItemOperationResult[0] = new StatItemIncrement()
            {
                statCode = createStatItemRequest[0].statCode,
                inc = 1
            };
            //Update total-lose or total-win
            if (isWinner)
            {
                statItemOperationResult[1] = new StatItemIncrement()
                {
                    statCode = createStatItemRequest[1].statCode,
                    inc = 1
                };
            }
            else
            {
                statItemOperationResult[1] = new StatItemIncrement()
                {
                    statCode = createStatItemRequest[2].statCode,
                    inc = 1
                };
            }
            //Update total-distance
            statItemOperationResult[2] = new StatItemIncrement()
            {
                statCode = createStatItemRequest[3].statCode,
                inc = distance
            };

            abServerStatistic.IncrementUserStatItems(userId, statItemOperationResult, OnIncrementUserStatItems);
            playerStatUpdatedCount += 1;
        }
    }
    #endregion //AccelByte Server Functions

    #region AccelByte Server Callbacks

    /// <summary>
    /// Callback that registered from DSM match request
    /// </summary>
    /// <param name="result"> Callback result matchrequest that contains sessionid gamemode and userids</param>
    private void OnMatchRequest(MatchRequest result)
    {
        Debug.Log("[AccelByteServerLogic] OnMatchRequest");
        if (result == null)
        {
            Debug.Log("[AccelByteServerLogic] OnMatchRequest is null");
        }
        else
        {
            Debug.Log("[AccelByteServerLogic] OnMatchRequest Success! Game Mode: " + result.game_mode);
            currentMatchmakingRequest = result;

            // Do head count of players for gameplay setup
            if (playerCount == 0 && currentMatchmakingRequest.matching_allies.Length > 0)
            {
                foreach (MatchingAlly ally in currentMatchmakingRequest.matching_allies)
                {
                    if (ally.matching_parties.Length > 0)
                    {
                        Debug.Log("[AccelByteServerLogic] OnMatchRequest ally.partyMember length: " + ally.matching_parties.Length);
                        foreach (MatchParty member in ally.matching_parties)
                        {
                            playerCount++;
                        }
                    }
                    else
                    {
                        Debug.Log("[AccelByteServerLogic] OnMatchRequest ally.partyMember length is 0: " + ally.matching_parties.Length);
                    }
                }
            }

            onServerGetMatchRequest?.Invoke(playerCount);
            Debug.Log("[AccelByteServerLogic] OnMatchRequest head count: " + playerCount);
        }
    }

    /// <summary>
    /// Callback of create user stat items when the player does not yet has user statistic
    /// </summary>
    /// <param name="result"> Callback result that contains the statcode </param>
    private void OnCreateUserStatItems(Result<StatItemOperationResult[]> result)
    {
        if (result.IsError)
        {
            Debug.Log("[AccelByteServerLogic] Create User statistic item failed:" + result.Error.Message);
            Debug.Log("[AccelByteServerLogic] Create User statistic item  Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("[AccelByteServerLogic] Create User statistic item successful");
        }
    }

    /// <summary>
    /// Callback of increment user stat items when the user already has user statistic created
    /// </summary>
    /// <param name="result"> Callback result that contains the statcode </param>
    private void OnIncrementUserStatItems(Result<StatItemOperationResult[]> result)
    {
        if (result.IsError)
        {
            Debug.Log("[AccelByteServerLogic] Increment User statistic item failed:" + result.Error.Message);
            Debug.Log("[AccelByteServerLogic] Increment User statistic item  Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            foreach (var data in result.Value)
            {
                Debug.Log("[AccelByteServerLogic] Increment User statistic item successful, stat code is: " + data.statCode);
            }
        }
    }
    #endregion // AccelByte Server Callbacks
}
