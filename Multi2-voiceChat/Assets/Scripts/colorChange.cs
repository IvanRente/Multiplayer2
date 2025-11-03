using UnityEngine;
using FishNet.Object;
public class ColorChange : NetworkBehaviour
{
    private Renderer render;
    private Color defaultColor;
    private bool isRed = false;

    private void Awake()
    {
        render = GetComponent<Renderer>();
        if (render != null)
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

    [ServerRpc]
    private void ToggleColorServerRpc()
    {
        isRed = !isRed;
        Color newColor = isRed ? Color.red : defaultColor;

        ToggleColorObserversRpc(newColor, isRed);
    }

    [ObserversRpc]
    private void ToggleColorObserversRpc(Color color, bool redState)
    {
        isRed = redState;
        SetColor(color);
    }

    private void SetColor(Color color)
    {
        if (render != null)
            render.material.color = color;
    }
}