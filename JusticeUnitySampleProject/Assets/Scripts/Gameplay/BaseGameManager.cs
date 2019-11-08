using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;

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
                          int randomPlayerStart = Random.Range(0, playerStarts.Length);
                          MovePlayerPawnBehavior p = NetworkManager.Instance.InstantiateMovePlayerPawn(0,playerStarts[randomPlayerStart].transform.position, playerStarts[randomPlayerStart].transform.rotation);
                          p.networkObject.OwnerNetId = player.NetworkId;
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

        private void FixedUpdate()
        {
            if (!isNetworkReady && networkObject != null)
            {
                NetworkStart();
            }
        }
    }
}
