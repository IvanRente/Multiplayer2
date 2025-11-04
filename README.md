# Group: Multiplayer 2 - Workshop 2

Welcome everyone, today you are going to learn to make a multiplayer game that supports Proximity VioceChat.

#### You are going to add the following things:

- Setting up game with a FishNet network.
- Consistency via FishNet components.
- Scalability via Lobbies.
- Proximity VoiceChat

## Setting up a game with FishNet.

You will have this already if you followed the first part of the workshop with the other group (Multiplayer 1) and us (
If you didn't follow along you can switch the branches 😄)

If you did follow along, you should have a simple scene with a player that can move around and see other players moving
around.

**Setting up FishNet**  
I installed FishNet through the Git URL and added a NetworkManager and Tugboat transport to the scene. Tugboat allows
the game to send and receive data locally or online.

**Creating the Player prefab**  
I made a simple Cube as the player, then added the required components:

- NetworkObject
- NetworkTransform
- CharacterController
- PlayerMovement

**Registering the prefab**  
I dragged the Player prefab into a Spawnable Prefabs list (inside a Prefab Objects asset) and also assigned it as the
Default Player Prefab in the NetworkManager and PlayerSpawner. This allows FishNet to spawn a player automatically when
someone connects.

**Adding player movement**  
The `PlayerMovement` script used Unity’s input system, but only worked for the local player by checking `IsOwner`. This
way, each player can move independently, and their positions stay synchronized using the `NetworkTransform` component.

**Adding color change with RPCs**  
The `ColorChange` script used a ServerRpc to send a command from the client to the server whenever the player pressed C,
and an ObserversRpc to update everyone’s screens. Pressing C toggles between red and the player’s original color. Both
the Host and Client see the same color at the same time.

## Achieving prediction (CPS: Client side prediction)

To achieve client-side prediction, we will implement a simple prediction system for player movement. This will help
reduce the perceived latency for players.

## Consistency via FishNet components.

We will add simple components to our network manager to ensure consistency across clients.

### Prediction

Using FishNet's built-in PredictionManager, TimeManager, and NetworkObject components, we can achieve a more consistent
experience for players.

#### Configure PredictionManager

Add the PredictionManager component to the NetworkManager GameObject and configure it to handle player movement
prediction.

#### Configure TimeManager

Add the TimeManager component to the NetworkManager GameObject and set it up to manage time synchronization across
clients.

#### Configure NetworkObject

Add the NetworkObject component to the player prefab to ensure that it is properly synchronized across the network.

### Lag Compensation

Add scripts to handle lag compensation for player actions, ensuring that actions are processed correctly even with
network latency.

#### Implementing States

```C#
private void TimeManager_OnTick()
{
    if (base.IsServer && ApplyEMP())
    {
        uint futureTicks = base.TimeManager.TimeToTicks(0.05f, TickRounding.RoundUp);
        ObserversSetEMPTick(base.TimeManager.LocalTick + futureTicks);
    }

}

// RunLocally is used to the server also sets the emp tick.
[ObserversRpc(RunLocally = true)]
private void ObserversSetEMPTick(uint serverTick)
{
    // Converts the server EMP tick to local tick for this client.
    _empStartTick = base.TimeManager.TickToLocalTick(serverTick);
    // Set end tick. In our example the EMP will last 1 second.
    _empEndTick = _empStartTick + base.TimeManager.TimeToTicks(1f, TickRounding.RoundUp);
}
```

If **RPC** is enabled.

```C++
// Tick when to start emp.
private uint _empStartTick = uint.MaxValue;
// Tick when to end emp.
private uint _empEndTick = uint.MaxValue;

/* This is an example of a replicate data excluding the extra
 * parameters, given they have no context in this example. */
[Replicate]
private void Replicate(MotorData md)
{
    uint localTick = base.TimeManager.LocalTick;
    /* If localTick is between EMP range then return.
     * It's important to not reset the emp values because
     * we want these to be the same during replays. If you reset
     * values soon as the condition of exiting emp was satisfied
     * then emp would not be properly set during a replay. 
     *
     * Note that this logic runs on the server and owner,
     * and if using prediction v2 can run on other clients
     * as well. */
    if (localTick >= _empStartTick && localTick <= _empEndTick)
    {
        /* Since under emp exit method early
        * to not process MotorData. In this example
         * we are using an EMP state, therefor the motor
         * will not work. You'll adjust this to whatever
         * your game needs. */
        return;
    }

    // Normal motor logic here. 
}
```

#### Implementing Rollback

The RollbackManager must know how far back in time to place colliders to obtain accurate hit results. When your client
is to fire their weapon you will want to gather the current PreciseTick and include it with your Fire RPC.

```C#
[Client]
private void Fire()
{
    // Use LastPacketTick to get the best tick alignment.
    PreciseTick pt = base.TimeManager.GetPreciseTick(TickType.LastPacketTick);
    // Call fire on the server.
    ServerFire(pt);
}

[ServerRpc]
private void ServerFire(PreciseTick pt)
{
    // Rollback using the precise tick sent in.
    // Using Physics for 3d rollback, Physics3D for 2d rollback.
    // Both physics types can be used at once.
    base.RollbackManager.Rollback(pt, RollbackManager.PhysicsType.Physics, base.IsOwner);
    // Perform your raycast normally.
    RaycastHit hit;
    if (Physics.Raycast(transform.position, transform.forward, out hit)) { }
    // Return the colliders to their proper positions.
    base.RollbackManager.Return();
}
```

#### Synchronizing Projectiles
When firing projectiles, ensure that their positions and states are synchronized across all clients using FishNet's
NetworkObject and NetworkTransform components.

*First the local client, or owning client, fires the projectile. The projectile is spawned locally, then the client tells the server to also fire the projectile. The MAX_PASSED_TIME constant is covered in the next code snippet.*

```C#
/// <summary>
/// Projectile to spawn.
/// </summary>
[Tooltip("Projectile to spawn.")]
[SerializeField]
private PredictedProjectile _projectile;
/// <summary>
/// Maximum amount of passed time a projectile may have.
/// This ensures really laggy players won't be able to disrupt
/// other players by having the projectile speed up beyond
/// reason on their screens.
/// </summary>
private const float MAX_PASSED_TIME = 0.3f;

/// <summary>
/// Local client fires weapon.
/// </summary>
private void ClientFire()
{
    Vector3 position = transform.position;
    Vector3 direction = transform.forward;

    /* Spawn locally with 0f passed time.
     * Since this is the firing client
     * they do not need to accelerate/catch up
     * the projectile. */
    SpawnProjectile(position, direction, 0f);
    // Ask server to also fire passing in current Tick.
    ServerFire(position, direction, base.TimeManager.Tick);
}

/// <summary>
/// Spawns a projectile locally.
/// </summary>
private void SpawnProjectile(Vector3 position, Vector3 direction, float passedTime)
{
    PredictedProjectile pp = Instantiate(_projectile, position, Quaternion.identity);
    pp.Initialize(direction, passedTime);
}#

```

## Scalability via Lobbies.

## Proximity VoiceChat

To implement proximity voice chat, we will use a voice chat system that allows players to hear each other based on their
distance in the game world.

### Setting up Proximity VoiceChat
