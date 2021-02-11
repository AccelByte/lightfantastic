// Copyright Epic Games, Inc. All Rights Reserved.

//#define PLATFORM_32BITS
//#define PLATFORM_64BITS

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX) && UNITY_64
	#define PLATFORM_64BITS
#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX)
	#define PLATFORM_32BITS
#endif

namespace Epic.OnlineServices
{
	internal static class Config
	{
		public const string BinaryName =
		#if (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
			"libEOSSDK-Mac-Shipping"
		#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX) && PLATFORM_64BITS
			"EOSSDK-Win64-Shipping"
		#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX) && PLATFORM_32BITS
			"EOSSDK-Win32-Shipping"
		#elif PLATFORM_64BITS
			"EOSSDK-Win64-Shipping.dll"
		#elif PLATFORM_32BITS
			"EOSSDK-Win32-Shipping.dll"
		#else
			#error Unable to determine EOSSDK binary name. Ensure compilation symbols PLATFORM_32BITS and PLATFORM_64BITS have been set accordingly.
			"EOSSDK-UnknownPlatform-Shipping"
		#endif
		;
	}
}