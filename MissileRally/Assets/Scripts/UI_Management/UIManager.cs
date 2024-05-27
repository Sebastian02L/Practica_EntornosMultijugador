using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Core;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IUI
{
    GameObject _canvas;
    IState _currentState;

    public IState State
    {
        get
        {
            return _currentState;
        }

        set
        {
            if (_currentState != null)
            {
                _currentState.Exit();
            }

            _currentState = value;

            _currentState.Enter();
        }
    }

    public GameObject Canvas 
    {
        get
        {
            return _canvas;
        }

        set
        {
            _canvas = value;
        }
    }

    const int maxConnections = 50;
    string joinCode = "Enter room code...";
    public string playerName = "Enter player name...";

    private void Start()
    {
        Canvas = GameObject.Find("Canvas");
        State = new StartingState(this);
    }

    private void Update()
    {
        _currentState.Update();
    }

    async void StartHost()
    {

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        NetworkManager.Singleton.StartHost();

    }

    async void StartClient(string joinCode)
    {

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        NetworkManager.Singleton.StartClient();

    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        if (State is StartingState)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host")) StartHost();
        if (GUILayout.Button("Client")) StartClient(joinCode);
        playerName = GUILayout.TextArea(playerName);
        joinCode = GUILayout.TextArea(joinCode);
    }

    void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
        GUILayout.Label("Room: " + joinCode);
    }
}
