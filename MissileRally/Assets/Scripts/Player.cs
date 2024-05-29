using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Unity.Collections;
using Unity.Netcode;
using Cinemachine;
using TMPro;
using System.Collections.Concurrent;
using System;
using System.Globalization;

[Serializable]
public struct PlayerData : INetworkSerializable
{
    public string name;
    public float colorRed;
    public float colorGreen;
    public float colorBlue;
    public float colorAlpha;
    public string status;

    public PlayerData(string name, float r, float g, float b, float a = 1.0f, string status = "Unready")
    {
        this.name = name;
        this.colorRed = r;
        this.colorGreen = g;
        this.colorBlue = b;
        this.colorAlpha = a;
        this.status = status;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref colorRed);
        serializer.SerializeValue(ref colorGreen);
        serializer.SerializeValue(ref colorBlue);
        serializer.SerializeValue(ref colorAlpha);
        serializer.SerializeValue(ref status);
    }

    public override string ToString()
    {
        return name;
    }
}

public class Player : NetworkBehaviour
{
    // Network Data
    public PlayerData data;
    public NetworkVariable<int> mapSelectedId = new NetworkVariable<int>();

    // Player Info
    public string Name { get; set; }
    public ulong ID { get; set; }

    // Race Info
    public GameObject car;
    public int CurrentPosition { get; set; }
    public int CurrentLap { get; set; } //Vuelta en la que está

    UIManager _ui;
    PlayerInput _playerInput;
    CinemachineVirtualCamera _camera;

    SelectCarColorMenu _selectCarColorMenu;
    PlayerReady _playerReadyComponent;

    //Transformada de la esfera blanca asociada al jugador. Cuando el jugador se desvuelca, se teletransporta a ella.
    public Transform spherePosition;

    public override string ToString()
    {
        return Name;
    }


    //////////////////////////
    ////CALLBACKS DE UNITY////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void Start()
    {
        _ui = GameObject.Find("@UIManager").GetComponent<UIManager>();
        _camera = FindAnyObjectByType<CinemachineVirtualCamera>(); //Guardamos una referencia de la camara de CineMachine, buscandola en la jerarquia.

        transform.position = new Vector3(40f, 0f, -15f);  //Punto de aparicion del Lobby de la sala.
        _playerInput = GetComponent<PlayerInput>();       //Guardamos la referencia del PlayerInput del prefab del jugador.

        ID = GetComponent<NetworkObject>().NetworkObjectId - 1;

        GameManager.Instance.currentRace.AddPlayer(this); //Agregamos un jugador nuevo a la carrera.

        //Nos interesa que un jugador pueda mover el coche generado por su juego, no el de los demas, por lo tanto, si es propietario del coche:
        if (IsOwner)
        {
            GameManager.Instance.player = this;
            _playerInput.enabled = true;    //Habilitamos su PlayerInput, de manera que pueda controlar su coche.

            playerSetup();                  //Llamada al metodo que se encarga de los preparativos de la cámara cuando el jugador se une a la partida.

            string tempName = _ui.playerName;

            if (tempName.Equals("Enter player name..."))
            {
                Name = car.transform.Find("MiniCanvas").transform.Find("Nombre").GetComponent<TextMeshProUGUI>().text + " " + ID;
            }
            else
            {
                Name = tempName;
            }

            Color initialCarColor = car.transform.Find("body").gameObject.GetComponent<MeshRenderer>().materials[1].color;
            data = new PlayerData(Name, initialCarColor.r, initialCarColor.g, initialCarColor.b);

            AddPlayer(ID, data);

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            }
        }

        AskForMyInfo(ID);

        if (ID == 0)
        {
            GameManager.Instance.mapSelectedId = mapSelectedId.Value;
            mapSelectedId.OnValueChanged += OnMapSelected;
        }
    }

    private void Update()
    {
        if (_selectCarColorMenu == null && IsOwner && FindAnyObjectByType<SelectCarColorMenu>() != null)
        {
            _selectCarColorMenu = FindAnyObjectByType<SelectCarColorMenu>();
            _selectCarColorMenu.colorSelected += OnColorSelected;
        }

        if (_playerReadyComponent == null && IsOwner && FindAnyObjectByType<PlayerReady>() != null)
        {
            _playerReadyComponent = FindAnyObjectByType<PlayerReady>();
            _playerReadyComponent.playerReady += OnPlayerReady;
        }

        // Apartado encargado de comprobar cuantos jugadores hay conectados
        if (GameManager.Instance.currentPlayers != GameManager.Instance.players.Count) // Si el valor de currentPlayers es distinto del numero de players del diccionario
        {
            GameManager.Instance.currentPlayers = GameManager.Instance.players.Count; // Lo actualiza
        }
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    //////////////////////////////////
    ////INICIALIZACIÓN DEL JUGADOR////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    void playerSetup() //Metodo encargado de asignar el prefab del jugador a la camara de CineMachine
    {
        _camera.Follow = car.transform;                            //Indicamos que la camara debe seguir a la transformada del coche del prefab.
        _camera.LookAt = car.transform;                            //Indicamos que el vector LookAt apunte a la transformada del coche.
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    /////////////////////////////////
    ////DESCONEXIÓN DE UN CLIENTE////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void OnClientDisconnect(ulong id) // Este método se llama en el servidor cuando un cliente se desconecta
    {
            // Elimina el player del diccionario
            GameManager.Instance.players.TryRemove(id, out _); // out _ descarta el playerData
            UpdatePlayersClientRpc(id); // Manda a los clientes el id del player que deben eliminar de su diccionario
    }

    [ClientRpc]
    void UpdatePlayersClientRpc(ulong id)
    {
        GameManager.Instance.players.TryRemove(id, out _); // out _ descarta el playerData
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    //////////////////////////////
    ////CONEXIÓN DE UN CLIENTE////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    void AddPlayer(ulong id, PlayerData data)
    {
        GameManager.Instance.players.TryAdd(id, data);
        AddPlayerServerRpc(id, data);
    }

    [ServerRpc]
    void AddPlayerServerRpc(ulong id, PlayerData data)
    {
        GameManager.Instance.players.TryAdd(id, data);
        car.transform.Find("MiniCanvas").transform.Find("Nombre").GetComponent<TextMeshProUGUI>().text = GameManager.Instance.players[ID].name;
    }

    void AskForMyInfo(ulong id)
    {
        AskForMyInfoServerRpc(id);
    }

    [ServerRpc(RequireOwnership = false)]
    void AskForMyInfoServerRpc(ulong id)
    {
        GetMyInfoClientRpc(id, GameManager.Instance.players[id]);
    }

    [ClientRpc]
    void GetMyInfoClientRpc(ulong id, PlayerData data)
    {
        GameManager.Instance.players.TryAdd(id, data);
        this.data = data;

        car.transform.Find("MiniCanvas").transform.Find("Nombre").GetComponent<TextMeshProUGUI>().text = GameManager.Instance.players[ID].name;
        car.transform.Find("body").gameObject.GetComponent<MeshRenderer>().materials[1].color = new Color(GameManager.Instance.players[ID].colorRed, GameManager.Instance.players[ID].colorGreen, GameManager.Instance.players[ID].colorBlue, GameManager.Instance.players[ID].colorAlpha);
        car.transform.Find("MiniCanvas").transform.Find("Estado").GetComponent<TextMeshProUGUI>().text = GameManager.Instance.players[ID].status;
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    ///////////////////////
    ////CAMBIO DE COLOR////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    void OnColorSelected()
    {
        SendDataServerRpc(ID, data);

        if (IsServer)
        {
            _ui.State = new MapSelectionState(_ui);
        }
        else
        {
            _ui.State = new LobbyState(_ui);
        }
    }

    [ServerRpc]
    void SendDataServerRpc(ulong id, PlayerData data)
    {
        GameManager.Instance.players[id] = data;
        this.data = data;

        GetMyColorClientRpc(id, data);
    }

    [ClientRpc]
    void GetMyColorClientRpc(ulong id, PlayerData data)
    {
        GameManager.Instance.players[id] = data;
        this.data = data;

        car.transform.Find("body").gameObject.GetComponent<MeshRenderer>().materials[1].color = new Color(GameManager.Instance.players[ID].colorRed, GameManager.Instance.players[ID].colorGreen, GameManager.Instance.players[ID].colorBlue, GameManager.Instance.players[ID].colorAlpha);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    /////////////////////////
    ////JUGADOR PREPARADO////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    void OnPlayerReady()
    {
        data.status = "Ready";
        SendReadyPlayerServerRpc(ID, data);
    }

    [ServerRpc]
    void SendReadyPlayerServerRpc(ulong id, PlayerData data)
    {
        GameManager.Instance.players[id] = data;
        this.data = data;
        UpdateReadyPlayersClientRpc(id, data);
    }

    [ClientRpc]
    void UpdateReadyPlayersClientRpc(ulong id, PlayerData data)
    {
        GameManager.Instance.players[id] = data;
        this.data = data;

        car.transform.Find("MiniCanvas").transform.Find("Estado").GetComponent<TextMeshProUGUI>().text = GameManager.Instance.players[ID].status;
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    /////////////////////////
    ////MAPA SELECCIONADO////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    void OnMapSelected(int previousValue, int newValue)
    {
        if (IsServer) { return; }
        
        GameManager.Instance.mapSelectedId = newValue;
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}