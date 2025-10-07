using FishNet.Object;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Camera cameraPrefab;
    [SerializeField] private Transform cameraHolder;

    public override void OnStartClient()
    {
        if (IsOwner)
            Instantiate(cameraPrefab, cameraHolder.position, cameraHolder.rotation, cameraHolder);
    }
}