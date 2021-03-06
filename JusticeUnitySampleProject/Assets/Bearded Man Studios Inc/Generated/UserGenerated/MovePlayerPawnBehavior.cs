using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"Vector3\"][\"uint\", \"uint\", \"Vector3\"][][\"string\"][\"string\", \"string\"][\"float\"][\"string\", \"uint\", \"uint\"][]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"newPos\"][\"newPlayerNum\", \"newOwnerId\", \"initialPos\"][][\"newUserId\"][\"newHatTitle\", \"newEffectTitle\"][\"CurrentSpeed\"][\"displayName\", \"platform\", \"playerIndex\"][]]")]
	public abstract partial class MovePlayerPawnBehavior : NetworkBehavior
	{
		public const byte RPC_UPDATE_POSITION = 0 + 5;
		public const byte RPC_SETUP = 1 + 5;
		public const byte RPC_BAN = 2 + 5;
		public const byte RPC_SET_USER_ID = 3 + 5;
		public const byte RPC_SET_ACTIVE_EQUIPMENT = 4 + 5;
		public const byte RPC_SET_CURRENT_SPEED = 5 + 5;
		public const byte RPC_SET_DISPLAY_NAME_AND_PLATFORM = 6 + 5;
		public const byte RPC_SET_READY = 7 + 5;
		
		public MovePlayerPawnNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (MovePlayerPawnNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("UpdatePosition", UpdatePosition, typeof(Vector3));
			networkObject.RegisterRpc("RPCSetup", RPCSetup, typeof(uint), typeof(uint), typeof(Vector3));
			networkObject.RegisterRpc("Ban", Ban);
			networkObject.RegisterRpc("RPCSetUserId", RPCSetUserId, typeof(string));
			networkObject.RegisterRpc("RPCSetActiveEquipment", RPCSetActiveEquipment, typeof(string), typeof(string));
			networkObject.RegisterRpc("RPCSetCurrentSpeed", RPCSetCurrentSpeed, typeof(float));
			networkObject.RegisterRpc("RPCSetDisplayNameAndPlatform", RPCSetDisplayNameAndPlatform, typeof(string), typeof(uint), typeof(uint));
			networkObject.RegisterRpc("RPCSetReady", RPCSetReady);

			networkObject.onDestroy += DestroyGameObject;

			if (!obj.IsOwner)
			{
				if (!skipAttachIds.ContainsKey(obj.NetworkId)){
					uint newId = obj.NetworkId + 1;
					ProcessOthers(gameObject.transform, ref newId);
				}
				else
					skipAttachIds.Remove(obj.NetworkId);
			}

			if (obj.Metadata != null)
			{
				byte transformFlags = obj.Metadata[0];

				if (transformFlags != 0)
				{
					BMSByte metadataTransform = new BMSByte();
					metadataTransform.Clone(obj.Metadata);
					metadataTransform.MoveStartIndex(1);

					if ((transformFlags & 0x01) != 0 && (transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() =>
						{
							transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform);
							transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform);
						});
					}
					else if ((transformFlags & 0x01) != 0)
					{
						MainThreadManager.Run(() => { transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform); });
					}
					else if ((transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() => { transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform); });
					}
				}
			}

			MainThreadManager.Run(() =>
			{
				NetworkStart();
				networkObject.Networker.FlushCreateActions(networkObject);
			});
		}

		protected override void CompleteRegistration()
		{
			base.CompleteRegistration();
			networkObject.ReleaseCreateBuffer();
		}

		public override void Initialize(NetWorker networker, byte[] metadata = null)
		{
			Initialize(new MovePlayerPawnNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new MovePlayerPawnNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// Vector3 newPos
		/// </summary>
		public abstract void UpdatePosition(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// uint newPlayerNum
		/// uint newOwnerId
		/// Vector3 initialPos
		/// </summary>
		public abstract void RPCSetup(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void Ban(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void RPCSetUserId(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void RPCSetActiveEquipment(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void RPCSetCurrentSpeed(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void RPCSetDisplayNameAndPlatform(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void RPCSetReady(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}