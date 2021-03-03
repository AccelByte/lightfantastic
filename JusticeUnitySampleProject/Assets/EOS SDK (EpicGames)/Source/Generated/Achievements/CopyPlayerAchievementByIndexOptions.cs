// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Achievements
{
	/// <summary>
	/// Input parameters for the <see cref="AchievementsInterface.CopyPlayerAchievementByIndex" /> function.
	/// </summary>
	public class CopyPlayerAchievementByIndexOptions
	{
		/// <summary>
		/// The Product User ID for the user who is copying the achievement.
		/// </summary>
		public ProductUserId UserId { get; set; }

		/// <summary>
		/// The index of the player achievement data to retrieve from the cache.
		/// </summary>
		public uint AchievementIndex { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct CopyPlayerAchievementByIndexOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_UserId;
		private uint m_AchievementIndex;

		public ProductUserId UserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_UserId, value);
			}
		}

		public uint AchievementIndex
		{
			set
			{
				m_AchievementIndex = value;
			}
		}

		public void Set(CopyPlayerAchievementByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = AchievementsInterface.CopyplayerachievementbyindexApiLatest;
				UserId = other.UserId;
				AchievementIndex = other.AchievementIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyPlayerAchievementByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}