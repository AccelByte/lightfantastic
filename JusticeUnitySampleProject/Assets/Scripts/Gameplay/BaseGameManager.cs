using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;

namespace Game
{
    public class BaseGameManager : GameManagerBehavior
    {
        // Singleton instance
        private static BaseGameManager instance;
        public static BaseGameManager Instance { get { return instance; } }

        [Header("Gameplay Data")]
        [SerializeField]
        private BasePlayerStart[] playerStarts = null;
        private readonly Dictionary<uint, MovePlayerPawnBehavior> playerObjects = new Dictionary<uint, MovePlayerPawnBehavior>();
        private bool isNetworkReady = false;
        [SerializeField]
        private string mainMenuSceneName_ = null;
        public string MainMenuSceneName { get { return mainMenuSceneName_; } }

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
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(instance);
            SetupEventHandlers();
            vCam_ = Instantiate(vCam, Camera.main.transform.position, Quaternion.identity);
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();
            isNetworkReady = true;
        }

        public void RegisterCharacter(uint playerNetworkId, MovePlayerPawnBehavior character)
        {
            Debug.Log("Registering New Character, Id: " + playerNetworkId);
            playerObjects.Add(playerNetworkId, character);
            if (!networkObject.IsServer)
            {
                GameObject tgt = character.gameObject;
                vCam_.LookAt = tgt.transform;
                vCam_.Follow = tgt.transform;
            }
        }

        public void RemoveFromCharacterList(uint playerNetworkId)
        {
            Debug.Log("Deleting A Character, Id: " + playerNetworkId);
            playerObjects.Remove(playerNetworkId);
        }

        private void SetupEventHandlers()
        {
            if (NetworkManager.Instance.IsServer)
            {
                NetworkManager.Instance.Networker.playerAccepted += (player, sender) =>
                {
                    MainThreadManager.Run(() =>
                    {
                        int playerIdx = Random.Range(0, playerStarts.Length);
                        MovePlayerPawnBehavior p = NetworkManager.Instance.InstantiateMovePlayerPawn(0, playerStarts[playerIdx].transform.position, playerStarts[playerIdx].transform.rotation);
                        p.networkObject.AssignOwnership(player);
                        p.networkObject.SetInitialPos(playerStarts[playerIdx].transform.position);
                        p.networkObject.OwnerNetId = player.NetworkId;
                        p.networkObject.playerNum = (uint)playerIdx + 1;
                        p.networkStarted += OnPlayerPawnNetworkStarted;
                        Debug.Log("A Player Just Connected, Id: " + player.NetworkId);
                    });
                };
                NetworkManager.Instance.Networker.playerDisconnected += (player, sender) =>
                {
                    // Remove the player from the list of players and destroy it
                    MovePlayerPawnBehavior p = playerObjects[player.NetworkId];
                    p.networkObject.Destroy();
                    RemoveFromCharacterList(player.NetworkId);
                    Debug.Log("A Player Just Disconnected, Id: " + player.NetworkId);
                };
            }
        }

        #region Events
        private void OnPlayerPawnNetworkStarted(NetworkBehavior behavior)
        {
            Debug.Log("OnPlayerPawnNetworkStarted");
            MovePlayerPawnBehavior p = behavior as MovePlayerPawnBehavior;
            if (p)
            {
                Debug.Log("Sending Initialization RPCs");
                p.networkObject.SendRpc(MovePlayerPawnBehavior.RPC_ASSIGN_PLAYER_NUM, Receivers.Owner, p.networkObject.playerNum);
                p.networkObject.SendRpc(MovePlayerPawnBehavior.RPC_ASSIGN_OWNER_ID, Receivers.Owner, p.networkObject.OwnerNetId);
            }
        }
        #endregion //Events
    }
}
