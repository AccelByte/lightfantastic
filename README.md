# Justice Unity Sample Project

Customer Facing Unity 3D Sample Game

Using Unity Version 2019.2.17f1

Using Justice Version 2.6.0

## Overview
A sample implementation of our Unity SDK in a game that uses the breadth of the AccelByte features.

## Resources

[Project JIRA](https://accelbyte.atlassian.net/jira/software/projects/JUSP/boards/87/backlog)

[Unity UI Style Guidelines](https://accelbyte.atlassian.net/wiki/spaces/JUSP/pages/270336058/Unity+UI+Style+Creation+Guidelines)

[General Unity C# Guidelines](https://accelbyte.atlassian.net/wiki/spaces/JUSP/pages/270336058/Unity+UI+Style+Creation+Guidelines)

[Sample Game Concept](https://docs.google.com/document/d/1sPn66uOjR1__uV0AQKoVQ3zEXrYZgkInv2nX9LlJOYU/edit?usp=sharing)

## HOW-TOs
### Build Light Fantastic client
* Open the solution using Unity 2019.2.17f1 as the minimum version.
* Ensure **Assets/Resources/AccelByteSDKConfig.json** fields are correct, especially the *ClientId* & *ClientSecret*.
* Modify *GAME_VERSION* (*and DS_TARGET_VERSION*) in the **LightFantasticConfig.cs**. If there's no changes in the gameplay and server version, *DS_TARGET VERSION* doesn't have to be updated.
* In the Unity Editor, open `File` `>` `BuildSettings`. Select *PC, Mac & Linux Standalone* platform. Pick your architecture. Ensure the `Server Build` is **unchecked** and ensure the `Development Build` is **checked**.
* Click `Build`.

### Build Light Fantastic server
* Open the solution using Unity 2019.2.17f1 as the minimum version.
* Ensure **Assets/Resources/AccelByteServerSDKConfig.json** fields are correct, especially the *ClientId* & *ClientSecret*.
* In the Unity Editor, open `File` `>` `BuildSettings`. Select *PC, Mac & Linux Standalone* platform. Pick Linux as the server's architecture. Ensure the `Server Build` is **checked**.
* Click `Build`.
