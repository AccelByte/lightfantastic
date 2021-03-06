﻿// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;

namespace Game
{
    public class BaseGameManager : GameManagerBehavior
    {
        public delegate void OnPlayerDisconnectBroadcast(MovePlayerPawnBehavior behavior);
        public event OnPlayerDisconnectBroadcast disconnectBroadcast;
        public delegate void GameManagerInitCompletion();
        public event GameManagerInitCompletion onInitCompleted;
        public delegate void OnAllPlayerConnectedEvent();
        public event OnAllPlayerConnectedEvent onAllplayerReady;
        public delegate void OnGameStartEvent();
        public event OnGameStartEvent onGameStart;
        public delegate void OnGameEndEvent();
        public event OnGameEndEvent onGameEnd;

        public class PlayerData
        {
            public PlayerData(BasePlayerPawn newCharacter, int newStartPosIdx = 0, ulong newFinishedTime = 0)
            {
                character = newCharacter;
                finishedTime = newFinishedTime;
                startPosIdx = newStartPosIdx;
            }
            private readonly BasePlayerPawn character;
            public BasePlayerPawn Character { get { return character; } }
            public ulong finishedTime;
            public int startPosIdx;
        }

        #region Fields and Properties
        [Header("Gameplay Data")]

        private readonly Dictionary<uint, PlayerData> players = new Dictionary<uint, PlayerData>();
        private bool isNetworkReady = false;
        [SerializeField]
        private int mainMenuSceneIdx_ = 0;
        [SerializeField]
        private GameObject achievementPopUp;
        public int MainMenuSceneIdx { get { return mainMenuSceneIdx_; } }
        private InGameHudManager hudMgr_;
        public InGameHudManager HudManager
        {
            get { return hudMgr_; }
            set
            {
                InGameHudManager input = value as InGameHudManager;
                if (input != null)
                {
                    hudMgr_ = input;
                }
                else
                {
                    Debug.LogError("Failed to register HUD Manager, Invalid Type");
                }
            }
        }

        private PawnSpawner pawnSpawner_;
        public PawnSpawner PSpawner
        {
            get { return pawnSpawner_; }
            set { pawnSpawner_ = value; }
        }

        [Header("Virtual Cameras")]
        [SerializeField]
        private Cinemachine.CinemachineVirtualCamera vCam = null;
        private Cinemachine.CinemachineVirtualCamera vCam_ = null;

        public NetWorker InstanceNetworker
        {
            get
            {
                if (isNetworkReady)
                {
                    return networkObject.Networker;
                }
                return null;
            }
        }

        [Header("Parallax")]
        [SerializeField] 
        public ParallaxSetter parallaxSetter;

        public int PlayerConnected = 0;
        public int PlayerMatched = 0;
        private int PlayerReady = 0; // Players that already send RPC_SETUP
        private Coroutine forceStartRaceCountdownCoroutine;

        #endregion

        private void Awake()
        {
            SetupEventHandlers();
            vCam_ = Instantiate(vCam, Camera.main.transform.position, Quaternion.identity);
            MainThreadTaskRunner.CreateGameObject();
            DeveloperConsoleHelper.Instance.Refresh();
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();
            if(networkObject.IsServer)
            {
                NetworkManager.Instance.InstantiateGameTimer(0);
                NetworkManager.Instance.InstantiateGameStartCountDown(0);
                onInitCompleted?.Invoke();
            }
            isNetworkReady = true;
        }

        /// <summary>
        /// Keep track of connected players
        /// </summary>
        /// <param name="playerNetworkId">The player network ID</param>
        /// <param name="pawn">Behaviour class of the pawn</param>
        public void RegisterCharacter(uint playerNetworkId, BasePlayerPawn pawn)
        {
            PlayerData data = new PlayerData(pawn);
            players.Add(playerNetworkId, data);
            if (networkObject.IsServer)
            {
                Debug.Log("BaseGameManager RegisterCharacter player: " + playerNetworkId);
            }
            if (!networkObject.IsServer && pawn.networkObject.IsOwner)
            {
                GameObject tgt = pawn.gameObject;
                vCam_.LookAt = tgt.transform;
                vCam_.Follow = tgt.transform;
            }
        }

        /// <summary>
        /// Remove disconnecting player
        /// </summary>
        /// <param name="playerNetworkId">The player network ID</param>
        public void RemoveFromCharacterList(uint playerNetworkId)
        {
            players.Remove(playerNetworkId);
        }

        /// <summary>
        /// Initialize required event handlers, only in server
        /// </summary>
        /// <param></param>
        private void SetupEventHandlers()
        {
            if (NetworkManager.Instance.IsServer)
            {
                Debug.Log("[BaseGameManager] Setting Up Event Handler in Server");
                NetworkManager.Instance.Networker.playerConnected += OnPlayerConnected;
                NetworkManager.Instance.Networker.playerAccepted += OnPlayerAccepted;
                NetworkManager.Instance.Networker.playerDisconnected += OnPlayerDisconnected;
                AccelByteManager.Instance.ServerLogic.onServerGetMatchRequest += onServerMatched;
            }
        }


        /// <summary>
        /// Check whether all player has reached the finish line
        /// </summary>
        /// <param></param>
        private bool HasAllPlayerFinished()
        {
            foreach (var player in players)
            {
                if (player.Value.finishedTime == 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check wheter aany player has reached the finish line
        /// </summary>
        /// <returns></returns>
        private bool HasAnyPlayerFinished()
        {
            foreach (var player in players)
            {
                if (player.Value.finishedTime > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determine the winner by finish time (The finish time is using the server timestep)
        /// </summary>
        /// <param></param>
        private uint DecideWinner()
        {
            uint winnerNetId = uint.MaxValue;
            ulong fastestTime = ulong.MaxValue;
            foreach (var player in players)
            {
                if (player.Value.finishedTime < fastestTime)
                {
                    winnerNetId = player.Key;
                    fastestTime = player.Value.finishedTime;
                }
            }
            return winnerNetId;
        }

        /// <summary>
        /// End the game, making all clients showing the Race Over Screen
        /// </summary>
        /// <param></param>
        private void EndTheGame()
        {
            uint winnerNetId = DecideWinner();
            foreach (var player in players)
            {
                var isWinner = player.Key == winnerNetId;
                AccelByteManager.Instance.ServerLogic.UpdateUserStatItem(player.Value.Character.transform.position.x, player.Value.Character.UserId, isWinner, OnStatisticUpdated_Server);
            }
            onGameEnd?.Invoke();
            networkObject.SendRpc(RPC_BROADCAST_END_GAME, Receivers.Others, winnerNetId);
        }

        private void EndTheGameTimeout(uint winnerId)
        {
            uint winnerNetId = winnerId;
            foreach (var player in players)
            {
                var isWinner = player.Key == winnerNetId;
                AccelByteManager.Instance.ServerLogic.UpdateUserStatItem(player.Value.Character.transform.position.x, player.Value.Character.UserId, isWinner, OnStatisticUpdated_Server);
            }
            onGameEnd?.Invoke();
            networkObject.SendRpc(RPC_BROADCAST_END_GAME, Receivers.Others, winnerNetId);
        }

        private void OnStatisticUpdated_Server(string userId)
        {
            networkObject.SendRpc(RPC_ON_STATISTIC_UPDATED, Receivers.Others, userId);
        }

        /// <summary>
        /// Called when the game timer is up
        /// </summary>
        public void GameTimeOver()
        {
            if (networkObject.IsServer)
            {
                // check if any player has reached finish line
                if (HasAnyPlayerFinished())
                {
                    Debug.Log("[GameManager] GameTimeOver is over, ONE player has finished!");
                    uint theWinner = 0;
                    foreach (var player in players)
                    {
                        if (player.Value.finishedTime > 0)
                        {
                            theWinner = player.Key;
                        }
                    }
                    Debug.Log("[GameManager] GameTimeOver The winner is : " + theWinner);
                    EndTheGameTimeout(theWinner);
                }
                else
                {
                    // calculate distance from player start to player position
                    // add new var to player data, and fill it from player pawn to game manager
                    Debug.Log("[GameManager] GameTimeOver is over, NO player has finished!");

                    uint theWinner = 0;
                    float longestDistance = float.MinValue;
                    foreach (var player in players)
                    {
                        var positionX = player.Value.Character.transform.position.x;
                        Debug.Log("[GameManager] GameTimeOver player " + player.Key + " position X : " + positionX);

                        if (positionX > longestDistance)
                        {
                            longestDistance = positionX;
                            theWinner = player.Key;
                        }
                    }
                    Debug.Log("[GameManager] GameTimeOver The winner is : " + theWinner);
                    EndTheGameTimeout(theWinner);
                }
            }
        }

        /// <summary>
        /// Start the game.
        /// args.GetAt<int>(0) is an Integer that will retrieve PlayerMatched count from server perspective.
        /// </summary>
        /// <param name="args">args.GetAt<int>(0) is PlayerMatched count</int></param>
        public override void BroadcastStartGame(RpcArgs args)
        {
            // unlock player pawn controller
            Debug.Log("BaseGameManager BroadcastStartGame!");
            if (!networkObject.IsServer)
            {
                // Set Local PlayerMatched variable, then MainHUD (local UI) will retrieve PlayerMatched value for Init Non-Incremental Achievement.
                PlayerMatched = args.GetAt<int>(0);
                AccelByteManager.Instance.AchievementLogic.RefreshAchievement();
            }
            onGameStart?.Invoke();
        }

        /// <summary>
        /// End the game, making all clients show the Race Over Screen
        /// </summary>
        /// <param name="args">args.GetAt<uint>(0) is playerID</uint></param>
        public override void BroadcastEndGame(RpcArgs args)
        {
            var isWinner = networkObject.MyPlayerId == args.GetAt<uint>(0);
            if (!networkObject.IsServer)
            {
                onGameEnd?.Invoke();
                hudMgr_.ShowRaceOverScreen(isWinner);
                if (isWinner)
                {
                    // if Win, then Check "win-first-match" Non-Incremental Achievement
                    AccelByteManager.Instance.AchievementLogic.UnlockNonIncrementalAchievement("win-first-time");
                }
            }
        }

        /// <summary>
        /// When Statistic updated, then check Achievement
        /// </summary>
        /// <param name="args">args<string>(0) is userId, to check if ID was correctly owned by client</param>
        public override void OnStatisticUpdated(RpcArgs args)
        {
            if (!networkObject.IsServer)
            {
                AccelByteManager.Instance.AchievementLogic.UnlockIncrementalAchievement(args.GetAt<string>(0));
            }
        }

        /// <summary>
        /// Disconnect this instance from the server, deletes the NetworkManager
        /// </summary>
        /// <param></param>
        public void DisconnectPlayer()
        {
            NetworkManager.Instance.Disconnect();
            SceneManager.LoadScene(mainMenuSceneIdx_);
        }

        public void StartGame()
        {
            if (networkObject.IsServer)
            {
                networkObject.SendRpc(RPC_BROADCAST_START_GAME, Receivers.All, PlayerMatched);
            }
        }

        /// <summary>
        /// this function call when any players connected
        /// this run on server. so only server that know this information
        /// this is our BeardedManStudios Package Manager that handle Server Interaction
        /// with clients or players
        /// </summary>
        /// <param name="player"> playerNetwork that join to server</param>
        /// <param name="sender"> Networker that handle every client to join a server</param>
        private void OnPlayerConnected(NetworkingPlayer player, NetWorker sender)
        {
            Debug.Log("[BaseGameManager] OnPlayerConnected(), " + player.Name + " is Connected");
            
            if(NetworkManager.Instance.IsServer)
            {
                if(PlayerConnected == 0)
                {
                    AccelByteManager.Instance.ServerLogic.OnPlayerFirstJoin();
                }
            }
        }

        #region Events
        public void OnPlayerFinished(uint finishedPlayerNetId, ulong timestep)
        {
            if (networkObject.IsServer)
            {
                if (players.ContainsKey(finishedPlayerNetId))
                {
                    players[finishedPlayerNetId].finishedTime = timestep;
                }
                if (HasAllPlayerFinished())
                {
                    EndTheGame();
                }
            }
        }

        private void OnPlayerAccepted(NetworkingPlayer player, NetWorker sender)
        {
            Debug.Log("GameManager OnPlayerAccepted!");

            if (NetworkManager.Instance.IsServer)
            {
                PlayerConnected++;
                Debug.Log("GameManager OnPlayerAccepted player number : " + player.NetworkId);
                Debug.Log("GameManager OnPlayerAccepted player connected: " + PlayerConnected);
            }
        }

        private void onServerMatched(int playerCount)
        {
            PlayerMatched = playerCount;
            CheckPlayerReady();
            ForceStartRaceCountdown(LightFantasticConfig.DEADLINE_TO_FORCE_START_MATCH_COUNTDOWN);
            Debug.Log("GameManager onServerMatched! player count: " + PlayerMatched);
                
            // unsubscribe once get the info
            //AccelByteManager.Instance.ServerLogic.onServerGetMatchRequest -= onServerMatched;
        }

        private void OnPlayerDisconnected(NetworkingPlayer player, NetWorker sender)
        {
            //TODO: check if any player disconnected when the game is not ended yet
            MainThreadManager.Run(() =>
            {
                MovePlayerPawnBehavior p = players[player.NetworkId].Character;
                RemoveFromCharacterList(player.NetworkId);
                disconnectBroadcast?.Invoke(p);
                List<NetworkObject> toDelete = new List<NetworkObject>();
                foreach (var no in sender.NetworkObjectList)
                {
                    if (no.Owner == player)
                    {
                        toDelete.Add(no);
                    }
                }
                for (int i = 0; i < toDelete.Count; i++)
                {
                    sender.NetworkObjectList.Remove(toDelete[i]);
                    toDelete[i].Destroy();
                }
                if (players.Count == 0)
                {
                    Debug.Log("All Players has exited, return to main menu");
                    DisconnectPlayer();
                }
            });
        }

        private void OnDestroy()
        {
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsServer)
            {
                NetworkManager.Instance.Networker.playerAccepted -= OnPlayerAccepted;
                NetworkManager.Instance.Networker.playerDisconnected -= OnPlayerDisconnected;
            }
        }
        #endregion //Events

        public void PlayerSetReady()
        {
            PlayerReady++;
            CheckPlayerReady();
        }

        private void CheckPlayerReady()
        {
            if (PlayerMatched == PlayerReady)
            {
                ForceStartRaceCountdown(0);
            }
        }
        
        /// <summary>
        /// If player ready count is not same with the connected player. Force to start.
        /// We won't wait all player to be ready for eternity.
        /// Just in case a player is disconnected in the process of joining server.
        /// </summary>
        /// <param name="second">Duration to force start match countdown 3,2,1</param>
        /// <returns></returns>
        private void ForceStartRaceCountdown(uint second)
        {
            IEnumerator ienumerator()
            {
                yield return new WaitForSeconds(second);
                OnAllplayerConnectedInvoke();
            }

            forceStartRaceCountdownCoroutine = StartCoroutine(ienumerator());
        }

        private void OnAllplayerConnectedInvoke()
        {
            onAllplayerReady?.Invoke();
            StopCoroutine(forceStartRaceCountdownCoroutine);
        }
    }
}
