using FishNet.Managing;
using UnityEngine;

public class ConnectionPanel : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private string defaultAddress = "127.0.0.1";
    [SerializeField] private ushort defaultPort = 7777;

    private string _address;
    private ushort _port;

    void Awake()
    {
        if (!networkManager) networkManager = FindObjectOfType<NetworkManager>();
        _address = PlayerPrefs.GetString("FN_ADDR", defaultAddress);
        _port = (ushort)PlayerPrefs.GetInt("FN_PORT", defaultPort);
    }

    // Wire these to your InputFields (On Value Changed)
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

    // Buttons
    public void StartHost()
    {
        if (!EnsureNM()) return;
        ApplyServerPort(_port);
        networkManager.ServerManager.StartConnection();
        ApplyClientEndpoint(_address, _port);
        networkManager.ClientManager.StartConnection();
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
        networkManager.ClientManager.StartConnection();
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

    // Helpers
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
