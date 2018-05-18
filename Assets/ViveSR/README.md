ViveSR plugin for Unity version 0.6.0.0
Copyright(c) 2017 HTC Corporation, All rights reserved.


Requirements:
1. This version ONLY supports the Vive HMD of dual camera version.
2. This version ONLY supports NVIDIA graphics cards.
3. This version ONLY works in the 64-bit window system.
4. MUST import SteamVR plugin which you can download in Unity AssetStore.
5. Enable Camera by steps [SteamVR]> [Settings]> [Camera]> [Enable Camera]

Quick Start:
1. Ensure requirements are met.
2. Drag [ViveSR].Prefab into the Hierarchy window.
5. Run it.

Warning:
1. The culling Mask of Camera of DualCamera (left) and its children must set same layer (layer 30 occupied by default).
2. The culling Mask of Camera of DualCamera (right) and its children must set same layer (layer 31 occupied by default).
3. The culling Mask of Camera of Camera (eye) must set the layers without above layers (without 30 and 31 by default).

Known issues:
1. Not support the direct mode on GTX 1060/1070
