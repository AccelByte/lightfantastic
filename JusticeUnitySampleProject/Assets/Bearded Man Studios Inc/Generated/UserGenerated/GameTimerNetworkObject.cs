using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0]")]
	public partial class GameTimerNetworkObject : NetworkObject
	{
		public const int IDENTITY = 7;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		[ForgeGeneratedField]
		private int _sec;
		public event FieldEvent<int> secChanged;
		public Interpolated<int> secInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int sec
		{
			get { return _sec; }
			set
			{
				// Don't do anything if the value is the same
				if (_sec == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_sec = value;
				hasDirtyFields = true;
			}
		}

		public void SetsecDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_sec(ulong timestep)
		{
			if (secChanged != null) secChanged(_sec, timestep);
			if (fieldAltered != null) fieldAltered("sec", _sec, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			secInterpolation.current = secInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _sec);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_sec = UnityObjectMapper.Instance.Map<int>(payload);
			secInterpolation.current = _sec;
			secInterpolation.target = _sec;
			RunChange_sec(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _sec);

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
				if (secInterpolation.Enabled)
				{
					secInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					secInterpolation.Timestep = timestep;
				}
				else
				{
					_sec = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_sec(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (secInterpolation.Enabled && !secInterpolation.current.UnityNear(secInterpolation.target, 0.0015f))
			{
				_sec = (int)secInterpolation.Interpolate();
				//RunChange_sec(secInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public GameTimerNetworkObject() : base() { Initialize(); }
		public GameTimerNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public GameTimerNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
