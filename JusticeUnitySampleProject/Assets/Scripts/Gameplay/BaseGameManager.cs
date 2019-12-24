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
        public static BaseGameManager Instance
        {
            get
            {
                return instance;
            }
        }

        [SerializeField]
        private BasePlayerStart[] playerStarts;
        private readonly Dictionary<uint, MovePlayerPawnBehavior> playerObjects = new Dictionary<uint, MovePlayerPawnBehavior>();
        private bool isNetworkReady;

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
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();

            if (NetworkManager.Instance.IsServer)
            {
                NetworkManager.Instance.Networker.playerAccepted += (player, sender) =>
                {
                    MainThreadManager.Run(() =>
                    {
                        int playerIdx = Random.Range(0, playerStarts.Length);
                        MovePlayerPawnBehavior p = NetworkManager.Instance.InstantiateMovePlayerPawn(0, playerStarts[playerIdx].transform.position, playerStarts[playerIdx].transform.rotation);
                        p.networkObject.OwnerNetId = player.NetworkId;
                        p.networkObject.MaxSpeed = ((BasePlayerPawn)p).MaxSpeed;
                        p.networkObject.SetInitialPos(playerStarts[playerIdx].transform.position);
                        p.networkObject.AssignOwnership(player);
                        p.networkObject.SendRpc(player, MovePlayerPawnBehavior.RPC_START_ASSIGN_PLAYER_NUM, (uint)playerIdx + 1);
                        playerObjects.Add(player.NetworkId, p);
                        Debug.Log("Player " + player.NetworkId + " connected.");
                    });
                };

                NetworkManager.Instance.Networker.playerDisconnected += (player, sender) =>
                {
                    // Remove the player from the list of players and destroy it
                    MovePlayerPawnBehavior p = playerObjects[player.NetworkId];
                    Debug.Log("Player " + player.NetworkId + " disconnected.");
                    playerObjects.Remove(player.NetworkId);
                    p.networkObject.Destroy();
                };
            }
            else
            {
                //if client then ready the controller
                NetworkManager.Instance.InstantiateInputListener();
            }

            isNetworkReady = true;
        }
    }
}
