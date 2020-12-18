// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Connect
{
	/// <summary>
	/// Input parameters for the <see cref="ConnectInterface.GetProductUserIdMapping" /> function.
	/// </summary>
	public class GetProductUserIdMappingOptions
	{
		/// <summary>
		/// The Product User ID of the existing, logged-in user that is querying account mappings
		/// </summary>
		public ProductUserId LocalUserId { get; set; }

		/// <summary>
		/// External auth service mapping to retrieve
		/// </summary>
		public ExternalAccountType AccountIdType { get; set; }

		/// <summary>
		/// The Product User ID of the user whose information is being requested
		/// </summary>
		public ProductUserId TargetProductUserId { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct GetProductUserIdMappingOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_LocalUserId;
		private ExternalAccountType m_AccountIdType;
		private System.IntPtr m_TargetProductUserId;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public ExternalAccountType AccountIdType
		{
			set
			{
				m_AccountIdType = value;
			}
		}

		public ProductUserId TargetProductUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetProductUserId, value);
			}
		}

		public void Set(GetProductUserIdMappingOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = ConnectInterface.GetproductuseridmappingApiLatest;
				LocalUserId = other.LocalUserId;
				AccountIdType = other.AccountIdType;
				TargetProductUserId = other.TargetProductUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as GetProductUserIdMappingOptions);
		}

		public void Dispose()
		{
		}
	}
}