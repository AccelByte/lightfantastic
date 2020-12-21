# Changelog

## com.unity.stadia Changelog

## [0.2.8] - 2020-11-05
• Update APIs to reflect Stadia 1.54 SDK
• Fix bug with GgpGameStateMetadata

## [0.2.7] - 2020-09-24
• Update APIs to reflect Stadia 1.53 SDK

## [0.2.6] - 2020-09-24
• Update APIs to reflect Stadia 1.52 SDK

## [0.2.5] - 2020-09-01
• Update APIs to reflect Stadia 1.51 SDK

## [0.2.4] - 2020-08-06
• Update APIs to reflect Stadia 1.50 SDK

## [0.2.3] - 2020-07-22
• Update APIs to reflect Stadia 1.49 SDK

## [0.2.2] - 2020-06-26
• Initial release of Stadia Platform Support package

• Update APIs to reflect Stadia 1.48 SDK

## [0.2.1] - 2020-06-05
• Added StreamSourceAudioListener and changed StreamSubscriptionAudioSource

## [0.2.0] - 2020-05-06
• Switch to raw cs files instead of dll

• Update APIs to reflect Stadia 1.46 SDK

• Enable Preview APIs with ENABLE_GGP_PREVIEW_APIS

## [0.1.9] - 2020-04-24
• Update APIs to reflect Stadia 1.45 SDK

• Remove GgpStreamSourceCreateFromPrimaryStream as it shouldn't be used.

## [0.1.8] - 2020-04-07
• Update APIs to reflect Stadia 1.44 SDK

• Add StreamSubscriptionAudioSource MonoBehaviour

## [0.1.7] - 2020-02-07
• GgpFutureExtensionMethods.GetMultipleResultBlocking() is now marked public 

## [0.1.6] - 2020-02-04
• Update APIs to reflect Stadia 1.42 SDK

## [0.1.5] - 2019-12-04
• Update APIs to reflect Stadia 1.37 SDK

• Update GgpPlayerSaveSetValue and GgpPlayerSaveGetValue to use a byte buffer instead of string. These APIs support any kind of serialized data.

• Add a Stadia futures wrapper library to make dealing with futures asynchronously easier. With the wrappers, it is possible to use use C# await and task syntax with any Stadia API returning a future.

## [0.1.4] - 2019-11-04

• Update APIS to reflect Stadia 1.36 SDK

• Update the GgpGameEventAttributeValue struct to use an IntPtr as the char* value. The developer must pin the memory before using the string field of this struct. See the [README.md](./README.md) file.

• Add correct array parameter handling for GgpPollCreateMultipleChoicePoll(), GgpGetStartupGameState() and GgpSetActiveGameState()

## [0.1.3] - 2019-10-10

• Update APIS to reflect Stadia 1.36 SDK

• Rename the symbols from libunitystadia.debug to libunitystadia.so.debug for Stadia certification requirements

## [0.1.2] - 2019-09-26

• Fixed struct definitions that contain 2D arrays;

• Fixed struct definitions that are unions in Stadia headers;

• Several APIs that previously took IntPtrs now take arrays;

• Rename 'Methods' class to 'StadiaNativeApis' to make written code using this library more readable;

• Unity.StadiaWrapper.dll has been upgraded to expose Stadia SDK 1.35 APIs (it previously exposed 1.33 APIs).

## [0.1.1] - 2019-08-27

• Renamed stadiawrapper.dll to Unity.StadiaWrapper.dll;

• Renamed wrapper namespace from UnityEngine.Stadia to Unity.StadiaWrapper;

• Wrapped input APIs.

## [0.1.0] - 2019-06-12

• Initial release of Stadia P/Invoke Layer package

This release inludes the libunitystadia.so native shared object and stadiawrapper.dll managed library.
