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

# FishNet Tutorial

In this tutorial, I set up a small multiplayer project in Unity using FishNet to understand how network consistency (**RPC**) works between clients and the server. The goal was to make players move and change color in a way that stays synchronized for everyone.

---

## Tutorial:

1. **Setting up FishNet**  
   I installed FishNet through the Git URL and added a NetworkManager and Tugboat transport to the scene. Tugboat allows the game to send and receive data locally or online.


2. **Creating the Player prefab**  
   I made a simple Cube as the player, then added the required components:
    - NetworkObject
    - NetworkTransform
    - CharacterController
    - PlayerMovement


3. **Registering the prefab**  
   I dragged the Player prefab into a Spawnable Prefabs list (inside a Prefab Objects asset) and also assigned it as the Default Player Prefab in the NetworkManager or PlayerSpawner. This allows FishNet to spawn a player automatically when someone connects.


4. **Adding player movement**  
   The `PlayerMovement` script used Unity’s input system, but only worked for the local player by checking `IsOwner`. This way, each player can move independently, and their positions stay synchronized using the `NetworkTransform` component.


5. **Adding color change with RPCs**  
   The `ColorChanger` script used a ServerRpc to send a command from the client to the server whenever the player pressed C, and an ObserversRpc to update everyone’s screens. Pressing C toggles between red and the player’s original color. Both the Host and Client see the same color at the same time.

## Conclusion

This demo showed how FishNet maintains consistency in a multiplayer environment. Using RPCs (Remote Procedure Calls) made it possible to keep both player movement and color changes synchronized between all clients. The ServerRpc ensured that input was processed by the server, while the ObserversRpc broadcasted updates to everyone.

Together with NetworkTransform and ownership checks, these systems allowed me to create a working, consistent multiplayer interaction where both movement and visual changes behave identically for every connected player. This helped me understand how networked actions, data, and visuals stay aligned across different machines in real time.
# Group: Multiplayer 2 - Workshop 2

Welcome everyone, today you are going to learn to make a multiplayer game that supports Proximity VioceChat.

#### You are going to add the following things:
- Setting up game with a FishNet network.
- Consistency via FishNet components.
- Scalability via Lobbies.
- Proximity VoiceChat

## Setting up a game with FishNet.
You will have this already if you followed the first part of the workshop with the other group (Multiplayer 1) and us (If you didn't follow along you can switch the branches 😄)

If you did follow along, you should have a simple scene with a player that can move around and see other players moving around.

## Achieving prediction (CPS: Client side prediction)
To achieve client-side prediction, we will implement a simple prediction system for player movement. This will help reduce the perceived latency for players.


## Consistency via FishNet components.
We will add simple components to our network manager to ensure consistency across clients.

### 


## Scalability via Lobbies.


## Proximity VoiceChat
To implement proximity voice chat, we will use a voice chat system that allows players to hear each other based on their distance in the game world.
### Setting up Proximity VoiceChat
