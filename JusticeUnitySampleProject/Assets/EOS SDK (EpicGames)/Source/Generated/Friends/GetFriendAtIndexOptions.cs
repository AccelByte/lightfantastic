// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Friends
{
	/// <summary>
	/// Input parameters for the <see cref="FriendsInterface.GetFriendAtIndex" /> function.
	/// </summary>
	public class GetFriendAtIndexOptions
	{
		/// <summary>
		/// The Epic Online Services Account ID of the user whose friend list is being queried
		/// </summary>
		public EpicAccountId LocalUserId { get; set; }

		/// <summary>
		/// Index into the friend list. This value must be between 0 and <see cref="FriendsInterface.GetFriendsCount" />-1 inclusively.
		/// </summary>
		public int Index { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct GetFriendAtIndexOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_LocalUserId;
		private int m_Index;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public int Index
		{
			set
			{
				m_Index = value;
			}
		}

		public void Set(GetFriendAtIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = FriendsInterface.GetfriendatindexApiLatest;
				LocalUserId = other.LocalUserId;
				Index = other.Index;
			}
		}

		public void Set(object other)
		{
			Set(other as GetFriendAtIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}