using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0.15,0,0]")]
	public partial class MovePlayerPawnNetworkObject : NetworkObject
	{
		public const int IDENTITY = 11;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		[ForgeGeneratedField]
		private Vector3 _Position;
		public event FieldEvent<Vector3> PositionChanged;
		public InterpolateVector3 PositionInterpolation = new InterpolateVector3() { LerpT = 0.15f, Enabled = true };
		public Vector3 Position
		{
			get { return _Position; }
			set
			{
				// Don't do anything if the value is the same
				if (_Position == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_Position = value;
				hasDirtyFields = true;
			}
		}

		public void SetPositionDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_Position(ulong timestep)
		{
			if (PositionChanged != null) PositionChanged(_Position, timestep);
			if (fieldAltered != null) fieldAltered("Position", _Position, timestep);
		}
		[ForgeGeneratedField]
		private uint _OwnerNetId;
		public event FieldEvent<uint> OwnerNetIdChanged;
		public Interpolated<uint> OwnerNetIdInterpolation = new Interpolated<uint>() { LerpT = 0f, Enabled = false };
		public uint OwnerNetId
		{
			get { return _OwnerNetId; }
			set
			{
				// Don't do anything if the value is the same
				if (_OwnerNetId == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_OwnerNetId = value;
				hasDirtyFields = true;
			}
		}

		public void SetOwnerNetIdDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_OwnerNetId(ulong timestep)
		{
			if (OwnerNetIdChanged != null) OwnerNetIdChanged(_OwnerNetId, timestep);
			if (fieldAltered != null) fieldAltered("OwnerNetId", _OwnerNetId, timestep);
		}
		[ForgeGeneratedField]
		private uint _playerNum;
		public event FieldEvent<uint> playerNumChanged;
		public Interpolated<uint> playerNumInterpolation = new Interpolated<uint>() { LerpT = 0f, Enabled = false };
		public uint playerNum
		{
			get { return _playerNum; }
			set
			{
				// Don't do anything if the value is the same
				if (_playerNum == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x4;
				_playerNum = value;
				hasDirtyFields = true;
			}
		}

		public void SetplayerNumDirty()
		{
			_dirtyFields[0] |= 0x4;
			hasDirtyFields = true;
		}

		private void RunChange_playerNum(ulong timestep)
		{
			if (playerNumChanged != null) playerNumChanged(_playerNum, timestep);
			if (fieldAltered != null) fieldAltered("playerNum", _playerNum, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			PositionInterpolation.current = PositionInterpolation.target;
			OwnerNetIdInterpolation.current = OwnerNetIdInterpolation.target;
			playerNumInterpolation.current = playerNumInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _Position);
			UnityObjectMapper.Instance.MapBytes(data, _OwnerNetId);
			UnityObjectMapper.Instance.MapBytes(data, _playerNum);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_Position = UnityObjectMapper.Instance.Map<Vector3>(payload);
			PositionInterpolation.current = _Position;
			PositionInterpolation.target = _Position;
			RunChange_Position(timestep);
			_OwnerNetId = UnityObjectMapper.Instance.Map<uint>(payload);
			OwnerNetIdInterpolation.current = _OwnerNetId;
			OwnerNetIdInterpolation.target = _OwnerNetId;
			RunChange_OwnerNetId(timestep);
			_playerNum = UnityObjectMapper.Instance.Map<uint>(payload);
			playerNumInterpolation.current = _playerNum;
			playerNumInterpolation.target = _playerNum;
			RunChange_playerNum(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _Position);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _OwnerNetId);
			if ((0x4 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _playerNum);

			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (PositionInterpolation.Enabled)
				{
					PositionInterpolation.target = UnityObjectMapper.Instance.Map<Vector3>(data);
					PositionInterpolation.Timestep = timestep;
				}
				else
				{
					_Position = UnityObjectMapper.Instance.Map<Vector3>(data);
					RunChange_Position(timestep);
				}
			}
			if ((0x2 & readDirtyFlags[0]) != 0)
			{
				if (OwnerNetIdInterpolation.Enabled)
				{
					OwnerNetIdInterpolation.target = UnityObjectMapper.Instance.Map<uint>(data);
					OwnerNetIdInterpolation.Timestep = timestep;
				}
				else
				{
					_OwnerNetId = UnityObjectMapper.Instance.Map<uint>(data);
					RunChange_OwnerNetId(timestep);
				}
			}
			if ((0x4 & readDirtyFlags[0]) != 0)
			{
				if (playerNumInterpolation.Enabled)
				{
					playerNumInterpolation.target = UnityObjectMapper.Instance.Map<uint>(data);
					playerNumInterpolation.Timestep = timestep;
				}
				else
				{
					_playerNum = UnityObjectMapper.Instance.Map<uint>(data);
					RunChange_playerNum(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (PositionInterpolation.Enabled && !PositionInterpolation.current.UnityNear(PositionInterpolation.target, 0.0015f))
			{
				_Position = (Vector3)PositionInterpolation.Interpolate();
				//RunChange_Position(PositionInterpolation.Timestep);
			}
			if (OwnerNetIdInterpolation.Enabled && !OwnerNetIdInterpolation.current.UnityNear(OwnerNetIdInterpolation.target, 0.0015f))
			{
				_OwnerNetId = (uint)OwnerNetIdInterpolation.Interpolate();
				//RunChange_OwnerNetId(OwnerNetIdInterpolation.Timestep);
			}
			if (playerNumInterpolation.Enabled && !playerNumInterpolation.current.UnityNear(playerNumInterpolation.target, 0.0015f))
			{
				_playerNum = (uint)playerNumInterpolation.Interpolate();
				//RunChange_playerNum(playerNumInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public MovePlayerPawnNetworkObject() : base() { Initialize(); }
		public MovePlayerPawnNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public MovePlayerPawnNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
