using UnityEngine;

public class WalkieTalkieToggle : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.T;
    public bool UseRadio { get; private set; } = false;
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            UseRadio = !UseRadio;
            Debug.Log($"[RADIO] UseRadio = {UseRadio}");
        }
    }
}
