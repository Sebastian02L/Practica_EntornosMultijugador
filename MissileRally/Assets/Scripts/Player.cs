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
    public NetworkVariable<int> currentLapNet = new NetworkVariable<int>(1);
    public NetworkVariable<float> gameplayTimer = new NetworkVariable<float>();
    public NetworkVariable<float> finalTime = new NetworkVariable<float>();

    // Player Info
    public string Name { get; set; }
    //Variable donde se almacena la ID del jugador, la cual empieza a contar en 0
    public ulong ID { get; set; }

    // Race Info
    public GameObject car;
    public int CurrentPosition { get; set; }
    public int CurrentLap = 1; //Vuelta en la que está
    

    GameObject _lobby;

    UIManager _ui;
    PlayerInput _playerInput;
    CinemachineVirtualCamera _camera;

    SelectCarColorMenu _selectCarColorMenu;
    PlayerReady _playerReadyComponent;

    public float arcLength;
    public float lastArcLengthWD;
    public float lastArcLength;

    public float wdLimit = 2.5f;
    public float wdCounter;
    public bool wrongDirection;
    public bool lineCrossed;

    public int actualRacePos;

    int countDown = 3;
    float countFrecuency = 1.5f;
    float passedTime = 0f;

    float auxiliarTimer = 0;

    public bool hasFinished = false;

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
        currentLapNet.OnValueChanged += OnCurrentLapChange;
        finalTime.OnValueChanged += OnFinalTimeChange;

        _lobby = GameObject.Find("Lobby");

        _ui = GameObject.Find("@UIManager").GetComponent<UIManager>();
        _camera = FindAnyObjectByType<CinemachineVirtualCamera>(); //Guardamos una referencia de la camara de CineMachine, buscandola en la jerarquia.

        transform.position = new Vector3(40f, 0f, -15f);  //Punto de aparicion del Lobby de la sala.
        _playerInput = GetComponent<PlayerInput>();       //Guardamos la referencia del PlayerInput del prefab del jugador.

        ID = GetComponent<NetworkObject>().OwnerClientId;

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

            GetReadyPlayersServerRpc(ID);

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
            gameplayTimer.OnValueChanged += OnGameplayTimerChange;
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

        if (IsServer && IsOwner)
        {   //Cuando mas de la mitad de los jugadores estan listos y hay mas de 1 jugador, la partida debe comenzar
            //Cuando esto ocurra, el host desde su coche realizara lo siguiente:
            if ((GameManager.Instance.readyPlayers >= ((GameManager.Instance.currentPlayers / 2) + 1)) && !GameManager.Instance.gameStarted && (GameManager.Instance.currentPlayers > 1) && GameManager.Instance.mapSelectedId != 0)
            {
                //Marcar en su runtime que el juego ha comenzado
                GameManager.Instance.gameStarted = true;
                //Desactivar el lobby y encender el escenario seleccionado a traves de la referencia al circuitManager
                _lobby.SetActive(false);
                GameManager.Instance.circuitManager.SetActive(true);
                //Cambiar la interfaz del host
                _ui.State = new GameInterfaceState(_ui);
            }
        }

        //Posteriormente, el servidor ejecutara en cada coche de su runtime lo siguiente:
        if (IsServer)
        {
            if (GameManager.Instance.gameStarted)
            {   //Si la lista del currentRace no posee al coche
                if (!GameManager.Instance.currentRace.ContainsPlayer(this))
                {
                    //Añade al jugador a la lista, lo teletransporta a su posicion correspondiente y ejecuta una llamada rpc
                    //GameManager.Instance.currentRace.AddPlayer(this); 
                    car.transform.position = GameManager.Instance.circuitManager.transform.GetChild(GameManager.Instance.mapSelectedId - 1).Find("StartPos").GetChild((int)ID).transform.position;
                    car.transform.rotation = GameManager.Instance.circuitManager.transform.GetChild(GameManager.Instance.mapSelectedId - 1).Find("StartPos").GetChild((int)ID).transform.rotation;
                    lastArcLengthWD = arcLength;
                    PrepareCircuitClientRpc();
                }
            }
        }

        //Cuando todos los jugadores esten en la partida, antes de que comience la carrera, hacemos que no puedan moverse
        if (IsOwner && GameManager.Instance.currentRace._players.Count == GameManager.Instance.currentPlayers && _playerInput.enabled && countDown == 3 && GameManager.Instance.mapSelectedId != 0)
        {
            _playerInput.enabled = false;
        }

        //Cuando los jugadores no puedan moverse y se haya escogido un mapa, pasamos a la fase de preparacion de la carrera.
        //Sin esta condicion GameManager.Instance.currentRace._players.Count == GameManager.Instance.currentPlayers,
        //al ser IsServer, se ejecuta en el Lobby cuando se une un cliente, cosa que no queremos
        if (IsServer && !_playerInput.enabled && GameManager.Instance.currentRace._players.Count == GameManager.Instance.currentPlayers && countDown >= 0) 
        {
            passedTime += Time.deltaTime;

            //Cada 1.5 segundos
            if(passedTime >= countFrecuency) 
            {
                RaceStartingClientRpc(countDown);
                passedTime = 0f;
            }
        }
        //A partir de esta condicion, la carrera ya ha empezado
        if(IsOwner && _playerInput.enabled && countDown <= 0)
        {
            //El servidor llevara el tiempo de la carrera y en los runtimes se obtendrá ese valor.
            //Una variable auxiliar lleva el tiempo total, luego se redondea y se asigna a la variable de red del coche del host, de manera que en el resto de runtimes se actualizara
            //el timepo. En el Runtime del propio servidor diretamente asignamos el valor al GameManager
            if (IsServer)
            {
                auxiliarTimer += Time.deltaTime;
                gameplayTimer.Value = (float) Math.Round(auxiliarTimer, 2);
                GameManager.Instance.gameplayTimer = gameplayTimer.Value;
            }

            if (arcLength < lastArcLengthWD)
            {
                wrongDirection = true;
            }
            else
            {

                lastArcLengthWD = arcLength;
                wrongDirection = false;
            }

            if (Mathf.Abs(arcLength - lastArcLength) > 20)
            {
                lastArcLengthWD = lastArcLength + 1;
            }

            if (wrongDirection)
            {
                wdCounter -= Time.deltaTime;
                if (wdCounter < 0)
                {
                    GameObject.Find("GameUI").transform.Find("PanelAtras").gameObject.SetActive(true);
                }
            }
            else
            {
                wdCounter = wdLimit;
                GameObject.Find("GameUI").transform.Find("PanelAtras").gameObject.SetActive(false);
            }

            if (GameManager.Instance.currentPlayers == 1)
            {
                car.transform.position = new Vector3(1000, -1000, 1000);
                Rigidbody carRb = car.GetComponent<Rigidbody>();
                carRb.constraints = RigidbodyConstraints.FreezeAll;
                arcLength = float.MaxValue - GameManager.Instance.playersFinished;
                GameManager.Instance.playersFinished += 1;
                hasFinished = true;
                _camera.Follow = GameManager.Instance.circuitManager.transform.GetChild(GameManager.Instance.mapSelectedId - 1).Find("Follow");
                _camera.LookAt = GameManager.Instance.circuitManager.transform.GetChild(GameManager.Instance.mapSelectedId - 1).Find("LookAt");
            }
            
        }
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [ClientRpc]
    void RaceStartingClientRpc(int phase)
    {
        //Buscamos al semaforo del escenario
        Transform LEDS = GameObject.Find("LEDS").transform;
        _camera.LookAt = LEDS.transform;

        switch (phase)
        {
            case 3:
                LEDS.Find("LED1").GetComponent<MeshRenderer>().material.color = new Color(1, 0.8f, 0, 0);
                LEDS.Find("LED1/Point Light1").GetComponent<Light>().color = new Color(1, 0.8f, 0, 0);
                break;

            case 2:
                LEDS.Find("LED2").GetComponent<MeshRenderer>().material.color = new Color(1, 0.8f, 0, 0);
                LEDS.Find("LED2/Point Light2").GetComponent<Light>().color = new Color(1, 0.8f, 0, 0);
                break;

            case 1:
                LEDS.Find("LED3").GetComponent<MeshRenderer>().material.color = new Color(1, 0.8f, 0, 0);
                LEDS.Find("LED3/Point Light3").GetComponent<Light>().color = new Color(1, 0.8f, 0, 0);
                break;

            case 0:

                LEDS.Find("LED1").GetComponent<MeshRenderer>().material.color = new Color(0, 0.7f, 0, 0);
                LEDS.Find("LED1/Point Light1").GetComponent<Light>().color = new Color(0, 0.7f, 0, 0);
                LEDS.Find("LED2").GetComponent<MeshRenderer>().material.color = new Color(0, 0.7f, 0, 0);
                LEDS.Find("LED2/Point Light2").GetComponent<Light>().color = new Color(0, 0.7f, 0, 0);
                LEDS.Find("LED3").GetComponent<MeshRenderer>().material.color = new Color(0, 0.7f, 0, 0);
                LEDS.Find("LED3/Point Light3").GetComponent<Light>().color = new Color(0, 0.7f, 0, 0);

                GameManager.Instance.player._playerInput.enabled = true;
                GameManager.Instance.player._camera.LookAt = (GameManager.Instance.player.car.transform);
                break;
        }
        countDown -= 1;
    }

    //Este metodo se invoca desde cada coche del runtime del host cuando comienza la partida, para ser ejecutado en ese coche del lado del cliente
    [ClientRpc(RequireOwnership = false)]
    void PrepareCircuitClientRpc()
    {
        GameManager.Instance.currentRace.AddPlayer(this);

        if (IsOwner)
        {
            //Marca en el runtime del cliente que el juego comenzo, apaga el lobby y enciende el mapa seleccionado, cambia la interfaz y agrega el jugador a la carrera
            GameManager.Instance.gameStarted = true;
            _lobby.SetActive(false);
            GameManager.Instance.circuitManager.SetActive(true);
            _ui.State = new GameInterfaceState(_ui);
        }
    }

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
        //Elimina el player del diccionario del host
        GameManager.Instance.players.TryRemove(id, out data);

        //Manda a los clientes el id del player que deben eliminar de su diccionario y true si era un jugador que estaba listo
        if (data.status.Equals("Ready"))
        {
            GameManager.Instance.readyPlayers -= 1;
            UpdatePlayersClientRpc(id, true);
        }
        else //Si no era un jugador que estaba listo, simplemente lo elimina del diccionario en los clientes
        {
            UpdatePlayersClientRpc(id, false); 
        }
        
    }

    [ClientRpc]
    void UpdatePlayersClientRpc(ulong id, bool state)
    {
        if (state)
        {
            GameManager.Instance.readyPlayers -= 1;
        }

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
        Name = data.name;
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
        Name = data.name;
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
        if(data.status == "Unready")
        {
            data.status = "Ready";
        }
        else
        {
            data.status = "Unready";
        }
        SendReadyPlayerServerRpc(ID, data);
    }

    [ServerRpc]
    void SendReadyPlayerServerRpc(ulong id, PlayerData data)
    {
        //GameManager.Instance.players[id] = data;
        //this.data = data;
        if(data.status == "Ready")
        {
            GameManager.Instance.readyPlayers += 1;
        }
        else
        {
            GameManager.Instance.readyPlayers -= 1;
        }

        UpdateReadyPlayersClientRpc(id, data, GameManager.Instance.readyPlayers);
    }

    [ClientRpc]
    void UpdateReadyPlayersClientRpc(ulong id, PlayerData data, int readyPlayers)
    {
        GameManager.Instance.players[id] = data;
        this.data = data;
        GameManager.Instance.readyPlayers = readyPlayers;
        

        car.transform.Find("MiniCanvas").transform.Find("Estado").GetComponent<TextMeshProUGUI>().text = GameManager.Instance.players[ID].status;
    }

    //Metodo que consulta el numero de jugadores listos cuando un nuevo jugador se une a la sala.
    [ServerRpc]
    void GetReadyPlayersServerRpc(ulong newPlayerID)
    {
        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { newPlayerID }
            }
        };

        getReadyPlayersClientRpc(GameManager.Instance.readyPlayers, clientRpcParams);
    }

    [ClientRpc]
    void getReadyPlayersClientRpc(int readyPlayers, ClientRpcParams param)
    { 
        GameManager.Instance.readyPlayers = readyPlayers;
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


    [ServerRpc]
    public void UpdateCurrentLapServerRpc()
    {
        currentLapNet.Value += 1;
        CurrentLap += 1;

        if (CurrentLap >= 4)
        {
            car.transform.position = new Vector3(1000, -1000, 1000);
            Rigidbody carRb = car.GetComponent<Rigidbody>();
            carRb.constraints = RigidbodyConstraints.FreezeAll;
            arcLength = float.MaxValue - GameManager.Instance.playersFinished;
            GameManager.Instance.playersFinished += 1;
            hasFinished = true;

            PlayerHasFinishedClientRpc(arcLength, hasFinished);
        }

        SetPartialTimes();
    }

    void OnCurrentLapChange(int previousValue, int newValue)
    {
        if (IsServer) { return; }

        CurrentLap = newValue;
        SetPartialTimes();
    }

    void OnGameplayTimerChange(float previousValue, float newValue)
    {
        if (IsServer) { return; }
        GameManager.Instance.gameplayTimer = newValue;
    }

    //Metodo que inserta en la interfaz el tiempo de cada vuelta cuando el jugador cruza correctamente la vuelta
    void SetPartialTimes()
    {
        if (IsOwner)
        {
            switch (CurrentLap)
            {
                case 2:
                    GameObject.Find("GameUI").transform.Find("PanelTimes").Find("TimeLapOne").GetComponent<TextMeshProUGUI>().text = $"Time Lap 1: {GameManager.Instance.gameplayTimer} s";
                    break;

                case 3:
                    GameObject.Find("GameUI").transform.Find("PanelTimes").Find("TimeLapTwo").GetComponent<TextMeshProUGUI>().text = $"Time Lap 2: {GameManager.Instance.gameplayTimer} s";
                    break;

                case 4:
                    //Si el host completa el circuito, se almacena el tiempo que le ha llevado hacerlo
                    if (IsServer)
                    {
                        finalTime.Value = gameplayTimer.Value;
                    }
                    else //Cada cliente avisa al host que ha completado la carrera, pasandole el tiempo que le ha llevado hacer el circuito
                    {
                        SendFinalTimeServerRpc(GameManager.Instance.gameplayTimer);
                    }
                    GameObject.Find("GameUI").transform.Find("PanelTimes").Find("TimeLapThree").GetComponent<TextMeshProUGUI>().text = $"Time Lap 3: {finalTime.Value} s";
                    break;
                default:
                    break;
            }
        }
    }

    [ServerRpc]
    void SendFinalTimeServerRpc(float totalTime)
    {
        finalTime.Value = totalTime;
    }

    void OnFinalTimeChange(float previousValue, float newValue)
    {
        finalTime.Value = newValue;
    }

    [ClientRpc]
    void PlayerHasFinishedClientRpc(float arcLength, bool hasFinished)
    {
        this.arcLength = arcLength;
        this.hasFinished = hasFinished;
        GameManager.Instance.playersFinished += 1;

        if (IsOwner)
        {
            _camera.Follow = GameManager.Instance.circuitManager.transform.GetChild(GameManager.Instance.mapSelectedId - 1).Find("Follow");
            _camera.LookAt = GameManager.Instance.circuitManager.transform.GetChild(GameManager.Instance.mapSelectedId - 1).Find("LookAt");
        }
    }
}