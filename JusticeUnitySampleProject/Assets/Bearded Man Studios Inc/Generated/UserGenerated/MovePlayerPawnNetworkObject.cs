using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0.15,0.15,0,0]")]
	public partial class MovePlayerPawnNetworkObject : NetworkObject
	{
		public const int IDENTITY = 10;

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
		private Quaternion _Rotation;
		public event FieldEvent<Quaternion> RotationChanged;
		public InterpolateQuaternion RotationInterpolation = new InterpolateQuaternion() { LerpT = 0.15f, Enabled = true };
		public Quaternion Rotation
		{
			get { return _Rotation; }
			set
			{
				// Don't do anything if the value is the same
				if (_Rotation == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_Rotation = value;
				hasDirtyFields = true;
			}
		}

		public void SetRotationDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_Rotation(ulong timestep)
		{
			if (RotationChanged != null) RotationChanged(_Rotation, timestep);
			if (fieldAltered != null) fieldAltered("Rotation", _Rotation, timestep);
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
				_dirtyFields[0] |= 0x4;
				_OwnerNetId = value;
				hasDirtyFields = true;
			}
		}

		public void SetOwnerNetIdDirty()
		{
			_dirtyFields[0] |= 0x4;
			hasDirtyFields = true;
		}

		private void RunChange_OwnerNetId(ulong timestep)
		{
			if (OwnerNetIdChanged != null) OwnerNetIdChanged(_OwnerNetId, timestep);
			if (fieldAltered != null) fieldAltered("OwnerNetId", _OwnerNetId, timestep);
		}
		[ForgeGeneratedField]
		private uint _Frame;
		public event FieldEvent<uint> FrameChanged;
		public Interpolated<uint> FrameInterpolation = new Interpolated<uint>() { LerpT = 0f, Enabled = false };
		public uint Frame
		{
			get { return _Frame; }
			set
			{
				// Don't do anything if the value is the same
				if (_Frame == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x8;
				_Frame = value;
				hasDirtyFields = true;
			}
		}

		public void SetFrameDirty()
		{
			_dirtyFields[0] |= 0x8;
			hasDirtyFields = true;
		}

		private void RunChange_Frame(ulong timestep)
		{
			if (FrameChanged != null) FrameChanged(_Frame, timestep);
			if (fieldAltered != null) fieldAltered("Frame", _Frame, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			PositionInterpolation.current = PositionInterpolation.target;
			RotationInterpolation.current = RotationInterpolation.target;
			OwnerNetIdInterpolation.current = OwnerNetIdInterpolation.target;
			FrameInterpolation.current = FrameInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _Position);
			UnityObjectMapper.Instance.MapBytes(data, _Rotation);
			UnityObjectMapper.Instance.MapBytes(data, _OwnerNetId);
			UnityObjectMapper.Instance.MapBytes(data, _Frame);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_Position = UnityObjectMapper.Instance.Map<Vector3>(payload);
			PositionInterpolation.current = _Position;
			PositionInterpolation.target = _Position;
			RunChange_Position(timestep);
			_Rotation = UnityObjectMapper.Instance.Map<Quaternion>(payload);
			RotationInterpolation.current = _Rotation;
			RotationInterpolation.target = _Rotation;
			RunChange_Rotation(timestep);
			_OwnerNetId = UnityObjectMapper.Instance.Map<uint>(payload);
			OwnerNetIdInterpolation.current = _OwnerNetId;
			OwnerNetIdInterpolation.target = _OwnerNetId;
			RunChange_OwnerNetId(timestep);
			_Frame = UnityObjectMapper.Instance.Map<uint>(payload);
			FrameInterpolation.current = _Frame;
			FrameInterpolation.target = _Frame;
			RunChange_Frame(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _Position);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _Rotation);
			if ((0x4 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _OwnerNetId);
			if ((0x8 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _Frame);

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
				if (RotationInterpolation.Enabled)
				{
					RotationInterpolation.target = UnityObjectMapper.Instance.Map<Quaternion>(data);
					RotationInterpolation.Timestep = timestep;
				}
				else
				{
					_Rotation = UnityObjectMapper.Instance.Map<Quaternion>(data);
					RunChange_Rotation(timestep);
				}
			}
			if ((0x4 & readDirtyFlags[0]) != 0)
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
			if ((0x8 & readDirtyFlags[0]) != 0)
			{
				if (FrameInterpolation.Enabled)
				{
					FrameInterpolation.target = UnityObjectMapper.Instance.Map<uint>(data);
					FrameInterpolation.Timestep = timestep;
				}
				else
				{
					_Frame = UnityObjectMapper.Instance.Map<uint>(data);
					RunChange_Frame(timestep);
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
			if (RotationInterpolation.Enabled && !RotationInterpolation.current.UnityNear(RotationInterpolation.target, 0.0015f))
			{
				_Rotation = (Quaternion)RotationInterpolation.Interpolate();
				//RunChange_Rotation(RotationInterpolation.Timestep);
			}
			if (OwnerNetIdInterpolation.Enabled && !OwnerNetIdInterpolation.current.UnityNear(OwnerNetIdInterpolation.target, 0.0015f))
			{
				_OwnerNetId = (uint)OwnerNetIdInterpolation.Interpolate();
				//RunChange_OwnerNetId(OwnerNetIdInterpolation.Timestep);
			}
			if (FrameInterpolation.Enabled && !FrameInterpolation.current.UnityNear(FrameInterpolation.target, 0.0015f))
			{
				_Frame = (uint)FrameInterpolation.Interpolate();
				//RunChange_Frame(FrameInterpolation.Timestep);
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
