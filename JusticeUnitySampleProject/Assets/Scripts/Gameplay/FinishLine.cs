using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class FinishLine : FinishLineBehavior
{
    protected override void NetworkStart()
    {
        base.NetworkStart();
    }

    public override void BroadcastPlayerFinish(RpcArgs args)
    {
        if(!networkObject.IsServer)
        {
            
        }
    }
}
