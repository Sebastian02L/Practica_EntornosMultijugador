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

//Se realizó un patrón State para gestionar los cambios de interfaz del juego. Esta clase se trata del contexto del patrón.
public class UIManager : MonoBehaviour, IUI
{
    GameObject _canvas; //Referencia al canvas de la escena para gestionar los cambios de interfaz.
    IState _currentState; //Referencia al estado actual del contexto.

    public IState State
    {
        get
        {
            return _currentState;
        }

        set //Al establecer un nuevo estado se ejecutado el método de salida del estado anterior y el de entrada al estado siguiente.
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

    const int maxConnections = 5; //Número máximo de conexiones de cada cliente sin contarle.

    //Texto por defecto para las áreas de texto en las que introducir el código de sala y el nombre del jugador.
    string joinCode = "Enter room code...";
    public string playerName = "Enter player name...";

    private void Start()
    {
        Canvas = GameObject.Find("Canvas");
        State = new StartingState(this); //La interfaz se inicializa en el estado de inicio.
    }

    private void Update()
    {
        _currentState.Update(); //El contexto ejecuta el método Update de su estado actual.
    }

    //Método encargado de inicializar un runtime como host de una partida del juego.
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

    //Método encargado de inicializar un runtime como cliente de una partida de juego que tiene de código "joinCode".
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

    //Callback de Unity para el redibujado de la interfaz.
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        if (State is StartingState) //Mientras se esté en el estado inicial, el cual corresponde a no haber seleccionado host o cliente.
        {
            StartButtons(); //Se muestran los botones de inicio.
        }
        else //Para el resto de estados de la interfaz se usará el Canvas y no la función OnGUI, no obstante, se mantienen las etiquetas de estado desde este script.
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    //Método que gestiona la funcionalidad de los botones de inicio para comenzar como host o cliente y las áreas de texto del código de sala y el nombre.
    void StartButtons()
    {
        if (GUILayout.Button("Host")) StartHost();
        if (GUILayout.Button("Client")) StartClient(joinCode);
        playerName = GUILayout.TextArea(playerName);
        joinCode = GUILayout.TextArea(joinCode);
    }

    //Método que muestra etiquetas con el estado de red del juego.
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
