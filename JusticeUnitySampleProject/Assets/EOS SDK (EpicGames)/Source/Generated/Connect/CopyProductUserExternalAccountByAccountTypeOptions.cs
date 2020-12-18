// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Connect
{
	/// <summary>
	/// Input parameters for the <see cref="ConnectInterface.CopyProductUserExternalAccountByAccountType" /> function.
	/// </summary>
	public class CopyProductUserExternalAccountByAccountTypeOptions
	{
		/// <summary>
		/// The Product User ID to look for when copying external account info from the cache.
		/// </summary>
		public ProductUserId TargetUserId { get; set; }

		/// <summary>
		/// External auth service account type to look for when copying external account info from the cache.
		/// </summary>
		public ExternalAccountType AccountIdType { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct CopyProductUserExternalAccountByAccountTypeOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_TargetUserId;
		private ExternalAccountType m_AccountIdType;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public ExternalAccountType AccountIdType
		{
			set
			{
				m_AccountIdType = value;
			}
		}

		public void Set(CopyProductUserExternalAccountByAccountTypeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = ConnectInterface.CopyproductuserexternalaccountbyaccounttypeApiLatest;
				TargetUserId = other.TargetUserId;
				AccountIdType = other.AccountIdType;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyProductUserExternalAccountByAccountTypeOptions);
		}

		public void Dispose()
		{
		}
	}
}