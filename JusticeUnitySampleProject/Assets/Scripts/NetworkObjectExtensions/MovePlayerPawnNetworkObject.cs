using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
    public partial class MovePlayerPawnNetworkObject : NetworkObject
    {
        private Vector3 previousPos = Vector3.zero;
        private ulong prevTimeStep = 0;
        private float maxSpeed_ = 0;
        public bool Banable { get; set; }
        public bool Banned { get; set; }
        public float MaxSpeed
        {
            get { return maxSpeed_; }
            set
            {
                if (IsServer)
                {
                    maxSpeed_ = value;
                }
            }
        }

        public void SetInitialPos(Vector3 initial)
        {
            previousPos = initial;
        }

        protected override bool ServerAllowRpc(byte methodId, Receivers receivers, RpcArgs args)
        {
            switch (methodId)
            {
                case MovePlayerPawnBehavior.RPC_UPDATE_POSITION:
                    {
                        if (Banned)
                        {
                            return false;
                        }
                        Vector3 currentPos = args.GetAt<Vector3>(0);
                        Vector3 posDelta = currentPos - previousPos;
                        ulong txDeltaT = args.Info.TimeStep - prevTimeStep;
                        float speed = posDelta.magnitude / txDeltaT;
                        Debug.Log("Speed: " + speed + "/" + maxSpeed_);
                        if (speed < maxSpeed_)
                        {
                            previousPos = currentPos;
                            prevTimeStep = args.Info.TimeStep;
                            return true;
                        }
                        if (Banable)
                        {
                            Banned = true;
                            SendRpc(MovePlayerPawnBehavior.RPC_BAN, Receivers.All);
                        }
                        return false;
                    }
            }
            return true;
        }
    }
}
