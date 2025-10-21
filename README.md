# Multiplayer 2
Research Question: What is the difference between the voice libraries?

### Explanation (of the Chosen Criteria)

#### 1. How hard it is to use
Gives a quick sense of the learning curve and setup difficulty. Since these tools vary from plug-and-play to very technical, it’s important to know how much work integration will require.

#### 2. Can compress voice data
Shows whether the library includes compression or encoding features. This directly affects performance and network load in multiplayer games.

#### 3. Works in Unity
Shows compatibility. Some libraries (like Concentus) are pure C# and work easily in Unity, while others (like PortAudio) require native plugins.

#### 4. Free to use
This helps determine whether the library can be used without extra costs.

#### 5. Bandwidth
Estimates the amount of data sent per second when streaming voice. Bandwidth directly affects network stability and latency in multiplayer environments.

#### 6. Best use
Suggests use cases (e.g., small games, large-scale projects, technical demos). This helps identify which option fits the project scale.

#### 7. How to integrate with Fishnet
Explains how to connect each voice library to FishNet.

#### 8. Works in Fishnet
Gives a quick finding on whether the integration is realistic and how much effort it takes.

#### 9. Link
Provides the source link for direct access to documentation or downloads

|                                   | **Concentus**                                                                                    | **Unity.WebRTC Audio**                                                            | **NAudio**                                                                                              | **BASS Audio Library**                                                                | **FMOD Core API**                                                                                                          | **PortAudio**                                                                       |
|-----------------------------------|--------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------|
| **How hard it is to use**         | Works right away (No plugins needed)                                                             | Needs setup for peer-to-peer connections                                          | Simple (For Windows only)                                                                               | Needs extra setup and paid license for some uses                                      | Complex to integrate                                                                                                       | Very low-level and technical                                                        |
| **Can compress voice data**       | Yes                                                                                              | Yes                                                                               | No                                                                                                      | Yes                                                                                   | Yes                                                                                                                        | No                                                                                  |
| **Works in Unity**                | Yes                                                                                              | Yes                                                                               | Yes (For Windows only)                                                                                  | Yes (Needs native plugin setup)                                                       | Yes (Via plugin)                                                                                                           | Yes (Needs wrapper plugin)                                                          |
| **Free to use**                   | Free                                                                                             | Free                                                                              | Free                                                                                                    | Free (For non-commercial use)                                                         | Free (For non-commercial use)                                                                                              | Free                                                                                |
| **Best use**                      | Making your own easy voice chat with FishNet                                                     | Real-time calls (Or video between players)                                        | Recording sound                                                                                         | Games that need high-quality sound effects                                            | Games that already use FMOD                                                                                                | Tech demos or custom sound tools                                                    |
| **Bandwidth**                     | 16–24 kbps (based on Opus codec spec: [RFC 6716](https://datatracker.ietf.org/doc/html/rfc6716)) | 30–60 kbps (based on WebRTC’s adaptive Opus mode: [WebRTC](https://webrtc.org/))  | 700–900 kbps (Based on: [PCM](https://en.wikipedia.org/wiki/Pulse-code_modulation))                     | 20–40 kbps (Based on MP3/OGG voice compression: [BASS](https://www.un4seen.com/doc/)) | 20–40 kbps (based on FMOD’s Vorbis/Opus voice compression range: [FMOD](https://www.fmod.com/resources/documentation-api)) | 700–900 kbps (Based on: [PCM](https://en.wikipedia.org/wiki/Pulse-code_modulation)) |
| **How to integrate with FishNet** | Encode mic data, send bytes via FishNet’s channel, decode on the other end.                      | Uses its own WebRTC transport; would require bridging between WebRTC and FishNet. | Capture voice with NAudio, then use Concentus or another codec to compress before sending over FishNet. | Needs a native plugin bridge and manual data streaming through FishNet.               | Requires FMOD C# bindings and custom network streaming code.                                                               | Must capture, format and send raw audio manually through FishNet.                   |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
| **Works in FishNet**              | Just send voice data                                                                             | Uses a different network system                                                   | Needs extra coding for compression | Needs custom setup and plugin | Complex to integrate | Manual setup, very technical |
| **Link**                          | https://github.com/lostromb/concentus                                                            | https://github.com/Unity-Technologies/com.unity.webrtc                            | https://github.com/naudio/NAudio                                                                        | https://www.un4seen.com/                                                              | https://www.fmod.com/resources/documentation-api                                                                           | http://www.portaudio.com/                                                           |
