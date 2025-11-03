using UnityEngine;
using FishNet.Object;
public class ColorChanger : NetworkBehaviour
{
    private Renderer rend;
    private Color defaultColor;
    private bool isRed = false;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            defaultColor = rend.material.color; 
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleColorServerRpc();
        }
    }

    // Called on the server when the local player presses C
    [ServerRpc]
    private void ToggleColorServerRpc()
    {
        isRed = !isRed;
        Color newColor = isRed ? Color.red : defaultColor;

        // Send the color change to all clients
        ToggleColorObserversRpc(newColor, isRed);
    }

    // Called on all clients to update visuals
    [ObserversRpc]
    private void ToggleColorObserversRpc(Color color, bool redState)
    {
        isRed = redState;
        SetColor(color);
    }

    private void SetColor(Color color)
    {
        if (rend != null)
            rend.material.color = color;
    }
}