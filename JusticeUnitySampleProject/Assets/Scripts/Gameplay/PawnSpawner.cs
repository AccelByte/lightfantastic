using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;

namespace Game
{
    public class PawnSpawner : PawnSpawnerBehavior
    {
        [SerializeField]
        private BasePlayerStart[] playerStarts_ = null;
        private BaseGameManager gameMgr_;
        private void Awake()
        {
            gameMgr_ = GameObject.FindGameObjectWithTag("GameManager").GetComponent<BaseGameManager>();
            gameMgr_.PSpawner = this;
            gameMgr_.disconnectBroadcast += RemoveFromStartPos;
            networkStarted += OnNetworkStarted;
        }

        private int FindEmptyStartPos()
        {
            for (int i = 0; i < playerStarts_.Length; i++)
            {
                if (!playerStarts_[i].Occupied)
                {
                    return i;
                }
            }
            return -1;
        }

        private void UpdatePlayerStarts(uint idx, bool isOccupied)
        {
            playerStarts_[idx].Occupied = isOccupied;
            networkObject.SendRpc(RPC_UPDATE_START_POS, Receivers.Others, new object[] { idx, isOccupied });
        }
        public void RemoveFromStartPos(MovePlayerPawnBehavior behavior)
        {
            UpdatePlayerStarts((uint)behavior.networkObject.playerNum - 1, false);
        }
        #region RPCs
        public override void RPCSpawnPawn(RpcArgs args)
        {
            uint ownerNetId = args.GetAt<uint>(0);
            int idx = args.GetAt<int>(1);

            if (networkObject.MyPlayerId == ownerNetId)
            {
                MovePlayerPawnBehavior p = NetworkManager.Instance.InstantiateMovePlayerPawn(0, playerStarts_[idx].transform.position, Quaternion.identity);
                UpdatePlayerStarts((uint)idx, true);
                playerStarts_[idx].Occupied = true;
                p.networkObject.SetInitialPos(playerStarts_[idx].transform.position);
                p.networkObject.OwnerNetId = ownerNetId;
                p.networkObject.playerNum = (uint)idx + 1;
            }
        }

        public override void RPCAnnounceReady(RpcArgs args)
        {
            if (networkObject.IsServer)
            {
                int idx = FindEmptyStartPos();
                if (idx == -1)
                {
                    return;
                }
                uint ownerNetId = args.Info.SendingPlayer.NetworkId;
                networkObject.SendRpc(args.Info.SendingPlayer, RPC_SPAWN_PAWN, new object[] { ownerNetId, idx });
            }
        }

        public override void RPCUpdateStartPos(RpcArgs args)
        {
            uint idx = args.GetAt<uint>(0);
            bool isOccupied = args.GetAt<bool>(1);
            playerStarts_[idx].Occupied = isOccupied;
        }
        #endregion

        #region Events
        private void OnNetworkStarted(NetworkBehavior behavior)
        {
            if (!networkObject.IsServer)
            {
                networkObject.SendRpc(RPC_ANNOUNCE_READY, Receivers.Server);
            }
        }
        #endregion
    }
}
