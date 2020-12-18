// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Logging
{
	/// <summary>
	/// Logging Categories
	/// </summary>
	public enum LogCategory : int
	{
		/// <summary>
		/// Low level logs unrelated to specific services
		/// </summary>
		Core = 0,
		/// <summary>
		/// Logs related to the Auth service
		/// </summary>
		Auth = 1,
		/// <summary>
		/// Logs related to the Friends service
		/// </summary>
		Friends = 2,
		/// <summary>
		/// Logs related to the Presence service
		/// </summary>
		Presence = 3,
		/// <summary>
		/// Logs related to the UserInfo service
		/// </summary>
		UserInfo = 4,
		/// <summary>
		/// Logs related to HTTP serialization
		/// </summary>
		HttpSerialization = 5,
		/// <summary>
		/// Logs related to the Ecommerce service
		/// </summary>
		Ecom = 6,
		/// <summary>
		/// Logs related to the P2P service
		/// </summary>
		P2P = 7,
		/// <summary>
		/// Logs related to the Sessions service
		/// </summary>
		Sessions = 8,
		/// <summary>
		/// Logs related to rate limiting
		/// </summary>
		RateLimiter = 9,
		/// <summary>
		/// Logs related to the PlayerDataStorage service
		/// </summary>
		PlayerDataStorage = 10,
		/// <summary>
		/// Logs related to sdk analytics
		/// </summary>
		Analytics = 11,
		/// <summary>
		/// Logs related to the messaging service
		/// </summary>
		Messaging = 12,
		/// <summary>
		/// Logs related to the Connect service
		/// </summary>
		Connect = 13,
		/// <summary>
		/// Logs related to the Overlay
		/// </summary>
		Overlay = 14,
		/// <summary>
		/// Logs related to the Achievements service
		/// </summary>
		Achievements = 15,
		/// <summary>
		/// Logs related to the Stats service
		/// </summary>
		Stats = 16,
		/// <summary>
		/// Logs related to the UI service
		/// </summary>
		Ui = 17,
		/// <summary>
		/// Logs related to the lobby service
		/// </summary>
		Lobby = 18,
		/// <summary>
		/// Logs related to the Leaderboards service
		/// </summary>
		Leaderboards = 19,
		/// <summary>
		/// Logs related to an internal Keychain feature that the authentication interfaces use
		/// </summary>
		Keychain = 20,
		/// <summary>
		/// Logs related to external identity providers
		/// </summary>
		IdentityProvider = 21,
		/// <summary>
		/// Logs related to Title Storage
		/// </summary>
		TitleStorage = 22,
		/// <summary>
		/// Logs related to the Mods service
		/// </summary>
		Mods = 23,
		/// <summary>
		/// Not a real log category. Used by <see cref="LoggingInterface.SetLogLevel" /> to set the log level for all categories at the same time
		/// </summary>
		AllCategories = 0x7fffffff
	}
}