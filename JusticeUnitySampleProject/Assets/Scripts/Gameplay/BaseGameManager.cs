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
        public event OnAllPlayerConnectedEvent onAllplayerConnected;
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
                AccelByteManager.Instance.ServerLogic.UpdateUserStatItem(player.Value.Character.UserId, isWinner);
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
                AccelByteManager.Instance.ServerLogic.UpdateUserStatItem(player.Value.Character.UserId, isWinner);
            }
            networkObject.SendRpc(RPC_BROADCAST_END_GAME, Receivers.Others, winnerNetId);
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
        /// End the game, making all clients show the Race Over Screen
        /// </summary>
        /// <param></param>
        public override void BroadcastStartGame(RpcArgs args)
        {
            // unlock player pawn controller
            Debug.Log("BaseGameManager BroadcastStartGame!");
            onGameStart?.Invoke();
        }

        /// <summary>
        /// End the game, making all clients show the Race Over Screen
        /// </summary>
        /// <param></param>
        public override void BroadcastEndGame(RpcArgs args)
        {
            var isWinner = networkObject.MyPlayerId == args.GetAt<uint>(0);
            if (!networkObject.IsServer)
            {
                hudMgr_.ShowRaceOverScreen(isWinner);
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
                networkObject.SendRpc(RPC_BROADCAST_START_GAME, Receivers.All);
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
            onAllplayerConnected?.Invoke();
            Debug.Log("GameManager onServerMatched! player count: " + PlayerMatched);
            
            // unsubscribe once get the info
            AccelByteManager.Instance.ServerLogic.onServerGetMatchRequest -= onServerMatched;
        }

        private void OnPlayerDisconnected(NetworkingPlayer player, NetWorker sender)
        {
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
    }
}
