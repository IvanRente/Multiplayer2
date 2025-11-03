using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

public class NetworkUIHider : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private GameObject[] uiRoots;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private bool hideOnClientOnly = true;

    LocalConnectionState _client = LocalConnectionState.Stopped;

    void Awake()
    {
        if (!networkManager)
#if UNITY_2023_1_OR_NEWER
            networkManager = Object.FindFirstObjectByType<NetworkManager>();
#else
            networkManager = FindObjectOfType<NetworkManager>();
#endif
        if (!networkManager) { Debug.LogError("NetworkManager not found"); enabled = false; return; }

        foreach (var go in uiRoots)
            if (go && !go.TryGetComponent<CanvasGroup>(out _))
                go.AddComponent<CanvasGroup>();

        networkManager.ClientManager.OnClientConnectionState += OnClientState;

        SetUIVisible(true);
    }

    void OnDestroy()
    {
        if (networkManager)
            networkManager.ClientManager.OnClientConnectionState -= OnClientState;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetUIVisible(!IsVisible());
    }

    void OnClientState(ClientConnectionStateArgs a)
    {
        _client = a.ConnectionState;
        Refresh();
    }

    void Refresh()
    {
        bool clientRunning = (_client == LocalConnectionState.Started);
        if (hideOnClientOnly)
            SetUIVisible(!clientRunning);
        else
            SetUIVisible(!(clientRunning || Application.isBatchMode));
    }

    void SetUIVisible(bool visible)
    {
        foreach (var go in uiRoots)
        {
            if (!go) continue;
            if (!go.TryGetComponent(out CanvasGroup cg))
                cg = go.AddComponent<CanvasGroup>();

            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }
    }

    bool IsVisible()
    {
        foreach (var go in uiRoots)
            if (go && go.TryGetComponent(out CanvasGroup cg) && cg.alpha > 0.5f)
                return true;
        return false;
    }
}
