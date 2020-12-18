// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Stats
{
	/// <summary>
	/// Input parameters for the <see cref="StatsInterface.CopyStatByIndex" /> function.
	/// </summary>
	public class CopyStatByIndexOptions
	{
		/// <summary>
		/// The Product User ID of the user who owns the stat
		/// </summary>
		public ProductUserId TargetUserId { get; set; }

		/// <summary>
		/// Index of the stat to retrieve from the cache
		/// </summary>
		public uint StatIndex { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct CopyStatByIndexOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_TargetUserId;
		private uint m_StatIndex;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public uint StatIndex
		{
			set
			{
				m_StatIndex = value;
			}
		}

		public void Set(CopyStatByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = StatsInterface.CopystatbyindexApiLatest;
				TargetUserId = other.TargetUserId;
				StatIndex = other.StatIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyStatByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}