# Welcome to Stadia Platform Support

Stadia Platform Support provides the following:
* Access to the Stadia API through a p/invoke layer
* Build and Run capabilities for Stadia in the Unity Editor

# Technical details
## Requirements

This version of Stadia Platform Support is compatible with the following versions of the Unity Editor:

* 2019.4 and later (recommended)

## Package contents

The following table indicates the folder structure of the Stadia Platform Support package:

|Location|Description|
|---|---|
|`<Runtime>`|Root folder containing the source for the Stadia p/invoke runtime, helper extension methods, and StreamConnect PulseAudio MonoBehaviours. This is the source that is available in the Player. |
|`<Editor>`|Root folder containing the source for the Stadia Build Settings window in the Unity Editor.|
