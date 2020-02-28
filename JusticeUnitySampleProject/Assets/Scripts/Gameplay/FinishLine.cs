using UnityEngine;
using UnityEngine.UI;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

namespace Game
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer), typeof(Rigidbody2D))]
    public class FinishLine : FinishLineBehavior
    {
        private BaseGameManager gameMgr;
        private Collider2D col2d;
        private bool gameAlreadyStarted = false;
        private void Awake()
        {
            var originalPosition = gameObject.transform.position;
            MainThreadTaskRunner.Instance.Run(() =>
            {
                gameObject.transform.position = new Vector3(originalPosition.x + LightFantasticConfig.FINISH_LINE_DISTANCE, originalPosition.y, originalPosition.z);
            });
            gameMgr = GameObject.FindGameObjectWithTag("GameManager").GetComponent<BaseGameManager>();
            gameMgr.onGameStart += () => { gameAlreadyStarted = true; };
            col2d = GetComponent<Collider2D>();
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!networkObject.IsServer && gameAlreadyStarted)
            {
                BasePlayerPawn finishedPawn = other.gameObject.GetComponent<Game.BasePlayerPawn>();
                if (finishedPawn != null)
                {
                    Debug.Log("Player " + finishedPawn.networkObject.playerNum + " finished");
                    networkObject.SendRpc(RPC_PLAYER_FINISHED, Receivers.Server, finishedPawn.networkObject.OwnerNetId);
                }
            }
        }

        public override void PlayerFinished(RpcArgs args)
        {
            if (networkObject.IsServer)
            {
                Debug.Log("Player " + args.GetAt<uint>(0) + " finished");
                gameMgr.OnPlayerFinished(args.GetAt<uint>(0), args.Info.TimeStep);
            }
        }
    }
}
