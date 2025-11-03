using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

public class ConnectionPanel : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private string defaultAddress = "127.0.0.1";
    [SerializeField] private ushort defaultPort = 7777;

    [Header("UI Panels")] [SerializeField] private GameObject lobbyUIPanel; // Assign your LobbyUI Canvas or panel
    [SerializeField] private GameObject connectionUIPanel; // The current panel with host/client buttons

    private string _address;
    private ushort _port;

    void Awake()
    {
        if (!networkManager) networkManager = FindObjectOfType<NetworkManager>();
        _address = PlayerPrefs.GetString("FN_ADDR", defaultAddress);
        _port = (ushort)PlayerPrefs.GetInt("FN_PORT", defaultPort);

        networkManager.ServerManager.OnServerConnectionState += OnServerStateChanged;
        networkManager.ClientManager.OnClientConnectionState += OnClientStateChanged;
        
        // connectionUIPanel.SetActive(true);
        // lobbyUIPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (networkManager == null) return;
        networkManager.ServerManager.OnServerConnectionState -= OnServerStateChanged;
        networkManager.ClientManager.OnClientConnectionState -= OnClientStateChanged;
    }

    private void OnServerStateChanged(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
            Debug.Log("Server started");
    }

    private void OnClientStateChanged(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("Client connected");
            connectionUIPanel.SetActive(false);
            lobbyUIPanel.SetActive(true);
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            connectionUIPanel.SetActive(true);
            lobbyUIPanel.SetActive(false);
        }
    }

    public void OnAddressChanged(string text)
    {
        _address = string.IsNullOrWhiteSpace(text) ? defaultAddress : text.Trim();
        PlayerPrefs.SetString("FN_ADDR", _address);
    }

    public void OnPortChanged(string text)
    {
        _port = ushort.TryParse(text, out var p) ? p : defaultPort;
        PlayerPrefs.SetInt("FN_PORT", _port);
    }

    public void StartHost()
    {
        if (!EnsureNM()) return;
        ApplyServerPort(_port);
        networkManager.ServerManager.StartConnection();
        ApplyClientEndpoint(_address, _port);
        networkManager.ClientManager.StartConnection();
        if (LobbyManager.Instance == null)
            return;
        
        Debug.Log("Server started. Also joined as a host.");
    }

    public void StartServer()
    {
        if (!EnsureNM()) return;
        ApplyServerPort(_port);
        networkManager.ServerManager.StartConnection();
    }

    public void StartClient()
    {
        if (!EnsureNM()) return;
        ApplyClientEndpoint(_address, _port);
    }

    public void StopServer()
    {
        if (!EnsureNM()) return;
        networkManager.ServerManager.StopConnection(true);
    }

    public void StopClient()
    {
        if (!EnsureNM()) return;
        networkManager.ClientManager.StopConnection();
    }

    bool EnsureNM()
    {
        if (networkManager) return true;
        Debug.LogError("NetworkManager not found.");
        return false;
    }

    void ApplyClientEndpoint(string addr, ushort port)
    {
        var t = networkManager.TransportManager.Transport;
        t.SetClientAddress(addr);
        t.SetPort(port);
    }

    void ApplyServerPort(ushort port)
    {
        var t = networkManager.TransportManager.Transport;
        t.SetPort(port);
    }
}