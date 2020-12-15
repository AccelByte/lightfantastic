# Justice Unity Sample Project

Unity 3D Sample Game Demo

Using Unity Version 2019.3.9f1

Using Accelbyte Services SDK  Version 2.24.0

## Overview
A sample implementation of our Unity SDK in a game that uses the breadth of the AccelByte features.

## HOW-TOs
### Build Light Fantastic client
* Open the solution using Unity 2019.3.9f1 as the minimum version.
* Ensure **Assets/Resources/AccelByteSDKConfig.json** fields are correct, especially the *ClientId* & *ClientSecret*.
* Modify *GAME_VERSION* (*and DS_TARGET_VERSION*) in the **LightFantasticConfig.cs**. If there's no changes in the gameplay and server version, *DS_TARGET VERSION* doesn't have to be updated.
* In the Unity Editor, open `File` `>` `BuildSettings`. Select *PC, Mac & Linux Standalone* platform. Pick your architecture. Ensure the `Server Build` is **unchecked** and ensure the `Development Build` is **checked**.
* Click `Build`.

### Build Light Fantastic server
* Open the solution using Unity 2019.3.9f1 as the minimum version.
* Ensure **Assets/Resources/AccelByteServerSDKConfig.json** fields are correct, especially the *ClientId* & *ClientSecret*.
* In the Unity Editor, open `File` `>` `BuildSettings`. Select *PC, Mac & Linux Standalone* platform. Pick Linux as the server's architecture. Ensure the `Server Build` is **checked**.
* Click `Build`.
