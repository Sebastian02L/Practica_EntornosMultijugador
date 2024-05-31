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

//Se realiz� un patr�n State para gestionar los cambios de interfaz del juego. Esta clase se trata del contexto del patr�n.
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

        set //Al establecer un nuevo estado se ejecutado el m�todo de salida del estado anterior y el de entrada al estado siguiente.
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

    const int maxConnections = 5; //N�mero m�ximo de conexiones de cada cliente sin contarle.

    //Texto por defecto para las �reas de texto en las que introducir el c�digo de sala y el nombre del jugador.
    string joinCode = "Enter room code...";
    public string playerName = "Enter player name...";

    private void Start()
    {
        Canvas = GameObject.Find("Canvas");
        State = new StartingState(this); //La interfaz se inicializa en el estado de inicio.
    }

    private void Update()
    {
        _currentState.Update(); //El contexto ejecuta el m�todo Update de su estado actual.
    }

    //M�todo encargado de inicializar un runtime como host de una partida del juego.
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

    //M�todo encargado de inicializar un runtime como cliente de una partida de juego que tiene de c�digo "joinCode".
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

        if (State is StartingState) //Mientras se est� en el estado inicial, el cual corresponde a no haber seleccionado host o cliente.
        {
            StartButtons(); //Se muestran los botones de inicio.
        }
        else //Para el resto de estados de la interfaz se usar� el Canvas y no la funci�n OnGUI, no obstante, se mantienen las etiquetas de estado desde este script.
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    //M�todo que gestiona la funcionalidad de los botones de inicio para comenzar como host o cliente y las �reas de texto del c�digo de sala y el nombre.
    void StartButtons()
    {
        if (GUILayout.Button("Host")) StartHost();
        if (GUILayout.Button("Client")) StartClient(joinCode);
        playerName = GUILayout.TextArea(playerName);
        joinCode = GUILayout.TextArea(joinCode);
    }

    //M�todo que muestra etiquetas con el estado de red del juego.
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
