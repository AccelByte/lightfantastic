// Copyright (c) 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AccelByte.Api;
using UnityEngine;
using AccelByte.Models;
using AccelByte.Core;
using UnityEngine.UI;
using WebSocketSharp;

public class AccelByteMatchmakingLogic : MonoBehaviour
{
    private UILobbyLogicComponent UIHandlerLobbyComponent;
    private AccelByteLobbyLogic lobbyLogic;
    private AccelByteManager accelByteManager;

    
    private MatchmakingNotif abMatchmakingNotif;
    private DsNotif abDSNotif;
    private bool connectToLocal;
    private string ipConnectToLocal = "127.0.0.1";
    private string portConnectToLocal = "15937";
    private static LightFantasticConfig.GAME_MODES gameModeEnum = LightFantasticConfig.GAME_MODES.unitytest;
    internal string gameMode = gameModeEnum.ToString();

    private MultiplayerMenu multiplayerConnect;
    
    private Dictionary<string,int> qosLatencies;
    
    public void Init(UILobbyLogicComponent uiLobbyLogicComponent, AccelByteLobbyLogic lobbyLogic)
    {
        UIHandlerLobbyComponent = uiLobbyLogicComponent;
        this.lobbyLogic = lobbyLogic;
        accelByteManager = lobbyLogic.GetComponent<AccelByteManager>();
        multiplayerConnect = lobbyLogic.GetComponent<MultiplayerMenu>();
        RefreshQoS();
    }

    public void AddEventListener()
    {
        UIHandlerLobbyComponent.matchmakingButtonCollection.On1VS1ButtonClicked.AddListener(delegate
        {
            GameplaySetGameMode(LightFantasticConfig.GAME_MODES.unitytest);
            FindMatchButtonClicked();
            UIHandlerLobbyComponent.matchmakingButtonCollection.UnselectAll();
        });
        UIHandlerLobbyComponent.matchmakingButtonCollection.On4FFAButtonClicked.AddListener(delegate
        {
            GameplaySetGameMode(LightFantasticConfig.GAME_MODES.upto4player);
            FindMatchButtonClicked();
            UIHandlerLobbyComponent.matchmakingButtonCollection.UnselectAll();
        });
        UIHandlerLobbyComponent.matchmakingButtonCollection.OnLocalButtonClicked.AddListener(delegate { GameplaySetIsLocal(true); });
        UIHandlerLobbyComponent.matchmakingButtonCollection.OnOnlineButtonClicked.AddListener(delegate { GameplaySetIsLocal(false); });
        
        UIHandlerLobbyComponent.matchmakingFailedPromptPanel.primaryButtonAction.AddListener(delegate { FindMatch(); });
        UIHandlerLobbyComponent.cancelMatchmakingButton.onClick.AddListener(FindMatchCancelClicked);
        // Bind Game Play / matchmaking request configuration
        UIHandlerLobbyComponent.localMatch_IP_inputFields.onValueChanged.AddListener(GameplaySetLocalIP);
        UIHandlerLobbyComponent.localMatch_Port_inputFields.onValueChanged.AddListener(GameplaySetLocalPort);
        UIHandlerLobbyComponent.refreshQoSButton.onClick.AddListener(RefreshQoS);
    }

    public void RemoveListener()
    {
        UIHandlerLobbyComponent.matchmakingButtonCollection.DeregisterAllButton();
        UIHandlerLobbyComponent.cancelMatchmakingButton.onClick.RemoveListener(FindMatchCancelClicked);
        UIHandlerLobbyComponent.localMatch_IP_inputFields.onValueChanged.RemoveListener(GameplaySetLocalIP);
        UIHandlerLobbyComponent.localMatch_Port_inputFields.onValueChanged.RemoveListener(GameplaySetLocalPort);
        UIHandlerLobbyComponent.refreshQoSButton.onClick.RemoveListener(RefreshQoS);
    }
    
    public void SetupMatchmakingCallbacks()
    {
        lobbyLogic.abLobby.MatchmakingCompleted += result => OnFindMatchCompleted(result);
        lobbyLogic.abLobby.DSUpdated += result => OnSuccessMatch(result);
        lobbyLogic.abLobby.RematchmakingNotif += result => OnRematchmaking(result);
        lobbyLogic.abLobby.ReadyForMatchConfirmed += result => OnGetReadyConfirmationStatus(result);
    }

    public void CleanupMatchmakingUI()
    {
        ShowMatchmakingBoard(false);
    }

    public void UnsubscribeAllCallbacks()
    {
        lobbyLogic.abLobby.MatchmakingCompleted -= OnFindMatchCompleted;
        lobbyLogic.abLobby.DSUpdated -= OnSuccessMatch;
        lobbyLogic.abLobby.RematchmakingNotif -= OnRematchmaking;
        lobbyLogic.abLobby.ReadyForMatchConfirmed -= OnGetReadyConfirmationStatus;
    }
    
    #region AccelByte MatchMaking Functions
    /// <summary>
    /// Find match function by calling lobby matchmaking
    /// Get latencies from available game server regions from Quality of Service
    /// Latencies can be used for matchmaking to determine which region has the lowest latency
    /// </summary>
    private void FindMatch()
    {
        if (connectToLocal)
        {
            lobbyLogic.abLobby.StartMatchmaking(gameMode, accelByteManager.LocalDSName, OnFindMatch);
        }
        else
        {
            var regionIndex = UIHandlerLobbyComponent.regionSelectorDropdown.value;
            var selectedLatency = qosLatencies.Values.ToList()[regionIndex];
            var selectedRegion = qosLatencies.Keys.ToList()[regionIndex];
            Dictionary<string, int> latencies = new Dictionary<string, int>(1){{selectedRegion, selectedLatency}};
            if (!selectedRegion.IsNullOrEmpty() && selectedLatency > 0)
            {
                lobbyLogic.abLobby.StartMatchmaking(gameMode, "", LightFantasticConfig.DS_TARGET_VERSION, latencies, OnFindMatch);
            }
            else
            {
                lobbyLogic.abLobby.StartMatchmaking(gameMode, "", LightFantasticConfig.DS_TARGET_VERSION, OnFindMatch);
            }
        }
    }

    /// <summary>
    /// Matchmaking required the player to a party
    /// When the player does not included in a party yet, then create a party first
    /// </summary>
    private void FindMatchButtonClicked()
    {
        if (!lobbyLogic.partyLogic.GetIsLocalPlayerInParty())
        {
            lobbyLogic.abLobby.CreateParty(OnPartyCreatedFindMatch);
        }
        else
        {
            FindMatch();
        }
    }

    /// <summary>
    /// Cancel matchmaking bound on cancel matchmaking button
    /// </summary>
    private void FindMatchCancelClicked()
    {
        lobbyLogic.abLobby.CancelMatchmaking(gameMode, OnFindMatchCanceled);
    }

    /// <summary>
    /// The game is using auto accept then ready consent callback thrown from lobby service
    /// This function can be used for accepting ready consent by bind it to accept popup button
    /// abMatchmakingNotif.MatchID will be filled when the match has been found
    /// </summary>
    public void OnAcceptReadyForMatchClicked()
    {
        if (abMatchmakingNotif != null)
        {
            lobbyLogic.abLobby.ConfirmReadyForMatch(abMatchmakingNotif.matchId, OnReadyForMatchConfirmation);
        }

        UIHandlerLobbyComponent.popupMatchConfirmation.gameObject.SetActive(false);
    }

    /// <summary>
    /// The game is using auto accept then ready consent callback thrown from lobby service
    /// This function can be used for decline ready consent by bind it to decline popup button
    /// </summary>
    public void OnDeclineReadyForMatchClicked()
    {
        lobbyLogic.abLobby.CancelMatchmaking(gameMode, OnFindMatchCanceled);
        UIHandlerLobbyComponent.popupMatchConfirmation.gameObject.SetActive(false);
    }

    /// <summary>
    /// Connect to game server once the game client knows once the game server is ready
    /// </summary>
    private bool allowToConnectServer = true;
    IEnumerator WaitForGameServerReady(string ip, string port)
    {
        if (allowToConnectServer)
        {
            allowToConnectServer = false;
            bool isActive = true;
            while (isActive)
            {
                yield return new WaitForSecondsRealtime(1.0f);
                if (connectToLocal)
                { 
                    ip = ipConnectToLocal;
                    port = portConnectToLocal;
                }
                multiplayerConnect.SetIPAddressPort(ip, port);
                multiplayerConnect.Connect();
                isActive = false;
            }
        }
    }

    /// <summary>
    /// Show matchmaking board once the callback from find match triggered
    /// </summary>
    /// <param name="show"> switch either to show and hide UI </param>
    /// <param name="gameFound"> show the state of the match has been found </param>
    public void ShowMatchmakingBoard(bool show, bool gameFound = false)
    {
        // If there's matchmaking board shown, disable the [ONLINE] and [LOCAL] button
        UIHandlerLobbyComponent.matchmakingButtonCollection.SetInteractable(PlayMatchButtonsScript.ButtonList.OnlineButton, !show);
        UIHandlerLobbyComponent.matchmakingButtonCollection.SetInteractable(PlayMatchButtonsScript.ButtonList.LocalButton, !show);
        
        UIHandlerLobbyComponent.matchmakingBoard.waitingTimerLayout.gameObject.SetActive(false);
        UIHandlerLobbyComponent.matchmakingBoard.gameObject.SetActive(show);
        if (!show)
        {
            UIHandlerLobbyComponent.matchmakingBoard.TerminateCountdown();
        }

        if (gameFound)
        {
            UIHandlerLobbyComponent.matchmakingBoard.gameObject.SetActive(true);
        }
        else
        {
            UIHandlerLobbyComponent.matchmakingBoard.waitingTimerLayout.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Game server using this function to host a game match
    /// multiplayerConnect referenced multiplayer menu from Forge Networking
    /// </summary>
    public void HostingGameServer()
    {
        multiplayerConnect.Host();
    }

    /// <summary>
    /// Game client function to connect to game server
    /// multiplayerConnect referenced multiplayer menu from Forge Networking
    /// </summary>
    public void ConnecttoGameServer()
    {
        multiplayerConnect.Connect();
    }
    
    /// <summary>
    /// Show prompt panel to choose "RETRY" or "CANCEL" matchmaking
    /// </summary>
    private void OnFailedMatchmaking(string reason)
    {
        lobbyLogic.abLobby.CancelMatchmaking(gameMode, OnFindMatchCanceled);
        UIHandlerLobbyComponent.matchmakingFailedPromptPanel.SetText("MATCHMAKING FAILED", reason);
        UIHandlerLobbyComponent.matchmakingFailedPromptPanel.Show();
    }
    #endregion

    #region AccelByte MatchMaking Callbacks
    /// <summary>
    /// Callback from start matchmaking 
    /// </summary>
    /// <param name="result"></param>
    private void OnFindMatch(Result<MatchmakingCode> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnFindMatch failed:" + result.Error.Message);
            Debug.Log("OnFindMatch Response Code::" + result.Error.Code);
            OnFailedMatchmaking("Couldn't do a matchmaking");
        }
        else
        {
            allowToConnectServer = true;
            
            Debug.Log("OnFindMatch Finding matchmaking with gameMode: " + gameMode + " . . .");

            // show matchmaking board and start count down on finding match
            ShowMatchmakingBoard(true);
            UIHandlerLobbyComponent.matchmakingBoard.StartCountdown(MatchmakingWaitingPhase.FindMatch,
                delegate
                {
                    OnFailedMatchmaking("Timeout to finding match");
                });

            // setup current game mode based on player's selected game mode
            UIHandlerLobbyComponent.matchmakingBoard.SetGameMode(gameModeEnum);
        }
    }

    /// <summary>
    /// Callback from cancel find match
    /// </summary>
    /// <param name="result"></param>
    private void OnFindMatchCanceled(Result<MatchmakingCode> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnFindMatchCanceled failed:" + result.Error.Message);
            Debug.Log("OnFindMatchCanceled Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnFindMatchCanceled The Match is canceled");
            ShowMatchmakingBoard(false);
            lobbyLogic.WriteInDebugBox(" Match Canceled");
        }
    }

    /// <summary>
    /// Callback from ready consent match accepted
    /// </summary>
    /// <param name="result"></param>
    private void OnReadyForMatchConfirmation(Result result)
    {
        abMatchmakingNotif = null;
        if (result.IsError)
        {
            Debug.Log("OnReadyForMatchConfirmation failed:" + result.Error.Message);
            Debug.Log("OnReadyForMatchConfirmation Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnReadyForMatchConfirmation Waiting for other player . . .");
            lobbyLogic.WriteInDebugBox("Waiting for other players . . .");
        }
    }
    #endregion

    #region AccelByte MatchMaking Notification Callbacks
    /// <summary>
    /// Callback from MatchmakingCompleted event of Lobby service
    /// This will be triggered if the player is in a party and the party leader do matchmaking
    /// </summary>
    /// <param name="result"> callback result that consist of a match id and matchmaking status </param>
    private void OnFindMatchCompleted(Result<MatchmakingNotif> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnFindMatchCompleted failed:" + result.Error.Message);
            Debug.Log("OnFindMatchCompleted Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnFindMatchCompleted Finding matchmaking Completed");
            Debug.Log("OnFindMatchCompleted Match Found: " + result.Value.matchId);
            Debug.Log(" Match status: " + result.Value.status);
            Debug.Log(" Expected match status: " + MatchmakingNotifStatus.done.ToString());

            abMatchmakingNotif = result.Value;
            lobbyLogic.abLobby.ConfirmReadyForMatch(abMatchmakingNotif.matchId, OnReadyForMatchConfirmation);
            UIHandlerLobbyComponent.matchmakingBoard.StartCountdown(MatchmakingWaitingPhase.ConfirmingMatch,
            delegate
            {
                OnFailedMatchmaking("Timeout to confirm matchmaking");
            });
            // if the player is in a party and the match is complete
            if (result.Value.status == MatchmakingNotifStatus.done.ToString())
            {
                lobbyLogic.WriteInDebugBox(" Match Found: " + result.Value.matchId);
            }
            // if the player is in a party and the party leader start a matchmaking
            else if (result.Value.status == MatchmakingNotifStatus.start.ToString())
            {
                MainThreadTaskRunner.Instance.Run(delegate 
                {
                    ShowMatchmakingBoard(true);
                });

                UIHandlerLobbyComponent.matchmakingBoard.StartCountdown(MatchmakingWaitingPhase.FindMatch,
                delegate
                {
                    OnFailedMatchmaking("Timeout to finding match");
                });
                UIHandlerLobbyComponent.matchmakingBoard.SetGameMode(gameModeEnum);
            }
            // if the player is in a party and the party leader cancel the current matchmaking
            else if (result.Value.status == MatchmakingNotifStatus.cancel.ToString())
            {
                MainThreadTaskRunner.Instance.Run(delegate
                {
                    ShowMatchmakingBoard(false);
                });
            }
        }
    }

    /// <summary>
    /// Callback event from DSUpdated of Lobby service
    /// The data are retrieved from DSM containing the current status of game server that being spawned
    /// Will be triggered multiple times each time will be udpated with the most current status
    /// CREATING : The DS is being spawned by DSM
    /// READY : The DS is ready to use, the game client can start to jump in to the game server
    /// BUSY : The DS is currently being used
    /// </summary>
    /// <param name="result"> Callback result of the current DS status </param>
    private void OnSuccessMatch(Result<DsNotif> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnSuccessMatch failed:" + result.Error.Message);
            Debug.Log("OnSuccessMatch Response Code::" + result.Error.Code);
            MainThreadTaskRunner.Instance.Run(delegate
            {
                OnFailedMatchmaking("An error occurs in the dedicated server manager");
            });
        }
        else
        {
            if (result.Value.isOK == "false" && !connectToLocal)
            {
                MainThreadTaskRunner.Instance.Run(delegate
                {
                    OnFailedMatchmaking("Failed to create a dedicated server");
                });
                return;
            }
            
            UIHandlerLobbyComponent.matchmakingBoard.StartCountdown(MatchmakingWaitingPhase.WaitingDSM, 
                delegate { OnFailedMatchmaking("Spawning a dedicated server timed out"); });
            Debug.Log("OnSuccessMatch success match completed");
            // DSM on process creating DS
            if (result.Value.status == DSNotifStatus.CREATING.ToString())
            {
                Debug.Log("Waiting for the game server!");
            }
            // DS is ready
            else if (result.Value.status == DSNotifStatus.READY.ToString())
            {
                // Set IP and port to persistent and connect to the game
                Debug.Log("Entering the game!");

                Debug.Log("Lobby OnSuccessMatch Connect");
                MainThreadTaskRunner.Instance.Run(() => { StartCoroutine(WaitForGameServerReady(result.Value.ip, result.Value.port.ToString())); });
            }
            else if (result.Value.status == DSNotifStatus.BUSY.ToString())
            {
                Debug.Log("Entering the game!");

                Debug.Log("Lobby OnSuccessMatch Connect");
                Debug.Log("ip: " + result.Value.ip + "port: " + result.Value.port);
                MainThreadTaskRunner.Instance.Run(() => { StartCoroutine(WaitForGameServerReady(result.Value.ip, result.Value.port.ToString())); });
            }

            Debug.Log("OnSuccessMatch ip: " + result.Value.ip + "port: " + result.Value.port);
            lobbyLogic.WriteInDebugBox("Match Success status " + result.Value?.status + " isOK " + result.Value.isOK + " pod: " + result.Value.podName);
            lobbyLogic.WriteInDebugBox("Match Success IP: " + result.Value.ip + " Port: " + result.Value.port);
            ShowMatchmakingBoard(true, true);
            abDSNotif = result.Value;
        }
    }

    /// <summary>
    /// Callback from Lobby service event when the player is banned
    /// Because the player does not accept ready consent within the time limit
    /// and continue to rematchmaking after banned
    /// </summary>
    /// <param name="result"></param>
    private void OnRematchmaking(Result<RematchmakingNotification> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnRematchmaking failed:" + result.Error.Message);
            Debug.Log("OnRematchmaking Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log(string.Format("OnRematchmaking OnProgress. Banned for {0} seconds", result.Value.banDuration));
            lobbyLogic.WriteInDebugBox(string.Format("OnRematchmaking... Banned for {0} seconds", result.Value.banDuration));
        }
    }

    /// <summary>
    /// Callback from lobby event when the other user that matched together have accepted the ready consent
    /// </summary>
    /// <param name="result"> callback result showing off match id and user id of the user confirming the ready consent </param>
    private void OnGetReadyConfirmationStatus(Result<ReadyForMatchConfirmation> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnGetReadyConfirmationStatus failed:" + result.Error.Message);
            Debug.Log("OnGetReadyConfirmationStatus Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnGetReadyConfirmationStatus Ready confirmation completed");
            Debug.Log("OnGetReadyConfirmationStatus: " + result.Value.userId + " is ready");
            lobbyLogic.WriteInDebugBox("Player " + result.Value.userId + " is ready");
        }
    }
    #endregion

    #region Gameplay Configuration Setter

    public void GameplaySetGameMode(LightFantasticConfig.GAME_MODES gameMode_)
    {
        gameModeEnum = gameMode_;
        gameMode = gameMode_.ToString();
    }
    
    public void GameplaySetIsLocal(bool isLocal)
    {
        connectToLocal = isLocal;
    }

    public void GameplaySetLocalIP(string ip)
    {
        ipConnectToLocal = ip;
    }

    public void GameplaySetLocalPort(string port)
    {
        portConnectToLocal = port;
    }
    #endregion

    #region Misc.

    /// <summary>
    /// Callback on find match button if the player is not in a party yet
    /// </summary>
    /// <param name="result"> callback result that contains various party info </param>
    private void OnPartyCreatedFindMatch(Result<PartyInfo> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnPartyCreated failed:" + result.Error.Message);
            Debug.Log("OnPartyCreated Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnPartyCreated Party successfully created with party ID: " + result.Value.partyID);
            lobbyLogic.partyLogic.SetAbPartyInfo(result.Value);
            lobbyLogic.partyLogic.SetIsLocalPlayerInParty(true);
            FindMatch();
        }
    }

    /// <summary>
    /// Get various latencies from available server regions
    /// </summary>
    private void RefreshQoS()
    {
        AccelBytePlugin.GetQos().GetServerLatencies(result =>
        {
            if (result.IsError || result.Value.Count == 0)
            {
                UIHandlerLobbyComponent.regionSelectorDropdown.options.Clear();
                UIHandlerLobbyComponent.regionSelectorDropdown.RefreshShownValue();
                UIHandlerLobbyComponent.regionSelectorDropdown.interactable = false;
            }
            else
            {
                UIHandlerLobbyComponent.regionSelectorDropdown.interactable = true;
                qosLatencies = result.Value;
                UIHandlerLobbyComponent.regionSelectorDropdown.options.Clear();
                foreach (var latency in qosLatencies)
                {
                    string text = $"{latency.Key} - {latency.Value}ms";
                    UIHandlerLobbyComponent.regionSelectorDropdown.options.Add(new Dropdown.OptionData(text));
                }
                UIHandlerLobbyComponent.regionSelectorDropdown.RefreshShownValue();
                UIHandlerLobbyComponent.regionSelectorDropdown.SetValueWithoutNotify(0);
            }
        });
    }

    #endregion
}
