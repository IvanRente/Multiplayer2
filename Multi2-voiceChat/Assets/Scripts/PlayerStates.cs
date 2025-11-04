using UnityEngine;
using FishNet.Object;
using FishNet.Managing;
using FishNet.Managing.Timing;
using System.Collections.Generic;

public class PlayerStates : NetworkBehaviour
{
    private struct PlayerState
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public PlayerState(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
        }
    }

    private Dictionary<uint, PlayerState> _stateHistory = new Dictionary<uint, PlayerState>();

    private CharacterController _cc;
    private bool _isFrozen;
    private uint _empStartTick;
    private uint _empEndTick;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    public override void OnStartNetwork()
    {
        if (IsServer)
            TimeManager.OnTick += TimeManager_OnTick;

        if (IsClient)
        {
            TimeManager.OnPostTick += TimeManager_OnPostTick;
        }
    }

    public override void OnStopNetwork()
    {
        if (TimeManager == null) return;

        if (IsServer)
            TimeManager.OnTick -= TimeManager_OnTick;

        if (IsClient)
            TimeManager.OnPostTick -= TimeManager_OnPostTick;
    }

    private void TimeManager_OnTick()
    {
        var tick = TimeManager.Tick;

        if (!_stateHistory.ContainsKey(tick))
        {
            _stateHistory.Add(tick, new PlayerState(transform.position, transform.rotation));
        }

        var threshold = tick - 200;
        if (_stateHistory.ContainsKey(threshold))
            _stateHistory.Remove(threshold);

        _isFrozen = tick >= _empStartTick && tick <= _empEndTick;
    }

    private void TimeManager_OnPostTick()
    {
        if (!IsOwner)
        {
            _isFrozen = TimeManager.Tick >= _empStartTick && TimeManager.Tick <= _empEndTick;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerEMPServerRpc(float durationSeconds)
    {
        uint startTick = TimeManager.Tick;
        uint endTick = startTick + (uint)(durationSeconds * TimeManager.TickRate);

        _empStartTick = startTick;
        _empEndTick = endTick;

        SetEmpTicksObserversRpc(startTick, endTick);
    }

    [ObserversRpc]
    private void SetEmpTicksObserversRpc(uint startTick, uint endTick)
    {
        _empStartTick = startTick;
        _empEndTick = endTick;
    }

    public bool IsFrozen()
    {
        return _isFrozen;
    }
}