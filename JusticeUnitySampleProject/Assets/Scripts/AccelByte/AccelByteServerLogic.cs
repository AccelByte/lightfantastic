using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking.Lobby;

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

    [Header("Local Dedicated Server Settings")]
    [SerializeField]
    private bool isLocal = true;
    [SerializeField]
    private string ipAddress = "127.0.0.1";
    [SerializeField]
    private string port = "15937";
    public string LocalDSName { get; set; }
    private string mainMenuSceneName;

    public delegate void OnServerGetMatchRequestEvent(int playerCount);
    public event OnServerGetMatchRequestEvent onServerGetMatchRequest;

    private MatchRequest currentMatchmakingRequest = null;
    private int playerCount = 0;

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
        abServer = AccelByte.Server.AccelByteServerPlugin.GetDedicatedServer();
        abServerManager = AccelByte.Server.AccelByteServerPlugin.GetDedicatedServerManager();
        abServer.LoginWithClientCredentials(OnLogin);
        mainMenuSceneName = SceneManager.GetActiveScene().name;
        SceneManager.sceneUnloaded += OnCurrentSceneUnloaded;
        abServerManager.OnMatchRequest += OnMatchRequest;
    }

    private string GetPodName()
    {
        return Environment.GetEnvironmentVariable("POD_NAME");
    }

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
            abServerManager.ConfigureHeartBeat();
            if (isLocal)
            {
                Debug.Log("[AccelByteServerLogic] Local DS IP: " + ipAddress + " Port: " + port);
                abServerManager.RegisterLocalServer(ipAddress, ushort.Parse(port), LocalDSName, OnRegistered);
            }
            else
            {
                abServerManager.RegisterServer(GetPodName(), ushort.Parse(port), OnRegistered);
            }
        }
    }

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
            DeregisterServer();
        }
    }

    private void DestroySelf()
    {
        instance = null;
        Destroy(gameObject);
        Application.Quit(0);
    }

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

            // do head count
            if (playerCount == 0 && currentMatchmakingRequest.matching_allies.Length > 0)
            {
                Debug.Log("[AccelByteServerLogic] OnMatchRequest player count 0");

                foreach (MatchingAlly ally in currentMatchmakingRequest.matching_allies)
                {
                    Debug.Log("[AccelByteServerLogic] OnMatchRequest matching_allies");

                    Debug.Log("[AccelByteServerLogic] OnMatchRequest matching_allies length: " + currentMatchmakingRequest.matching_allies.Length);

                    //for (int i = 0; i < ally.matching_parties.Length; i++)
                    //{
                    //    Debug.Log("[AccelByteServerLogic] OnMatchRequest partyMember: " + i + " " + ally.matching_parties[i]);
                    //}

                    if (ally.matching_parties.Length > 0)
                    {
                        Debug.Log("[AccelByteServerLogic] OnMatchRequest ally.partyMember length: " + ally.matching_parties.Length);
                        foreach (MatchParty member in ally.matching_parties)
                        {
                            Debug.Log("[AccelByteServerLogic] OnMatchRequest partyMember");
                            playerCount++;
                            Debug.Log("[AccelByteServerLogic] OnMatchRequest count: " + playerCount);
                        }
                    }
                    else
                    {
                        Debug.Log("[AccelByteServerLogic] OnMatchRequest ally.partyMember length is 0: " + ally.matching_parties.Length);
                    }
                    //playerCount++;
                    Debug.Log("[AccelByteServerLogic] OnMatchRequest partyMember = 0");
                }
            }

            onServerGetMatchRequest?.Invoke(playerCount);
            Debug.Log("[AccelByteServerLogic] OnMatchRequest head count: " + playerCount);
        }
    }
    public int GetPlayerCount()
    {
        return playerCount;
    }

    public MatchRequest GetMatchRequest()
    {
        return currentMatchmakingRequest;
    }
}
