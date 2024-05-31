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

//Struct para almacenar datos que se van a compartir entre todos los jugadores, como por ejemplo el nombre, si esta listo y el color seleccionado por este.
[Serializable]
public struct PlayerData : INetworkSerializable
{
    public string name;
    public float colorRed;
    public float colorGreen;
    public float colorBlue;
    public float colorAlpha;
    public string status;

    //Constructor del Struct
    public PlayerData(string name, float r, float g, float b, float a = 1.0f, string status = "Unready")
    {
        this.name = name;
        this.colorRed = r;
        this.colorGreen = g;
        this.colorBlue = b;
        this.colorAlpha = a;
        this.status = status;
    }
    //Serializador del Struct
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

//Clase Player, que alberga gran parte de la logica del juego junto con la clase GameManager
public class Player : NetworkBehaviour
{
    //Network Data. Variables de red que se sincronizan entre todas las replicas de los objetos "Player"
    public PlayerData data;
    public NetworkVariable<int> mapSelectedId = new NetworkVariable<int>();     //Variable que almacena el id del mapa seleccionado por el Host
    public NetworkVariable<int> currentLapNet = new NetworkVariable<int>(1);    //Variable que almacena la vuelta de la carrera en la que se encuentra el jugador
    public NetworkVariable<float> gameplayTimer = new NetworkVariable<float>(); //Variable que almacena el tiempo de la carrera
    public NetworkVariable<float> finalTime = new NetworkVariable<float>();     //Variable que almacena el tiempo en el que el jugador completó la carrera

    //Player Info
    public string Name { get; set; }

    //Variable donde se almacena la ID del jugador, la cual empieza a contar en 0 ya que utilizamos el OwnerClientID que empieza a contar en dicho numero.
    public ulong ID { get; set; }

    //Race Info
    public GameObject car;
    public int CurrentPosition { get; set; }
    public int CurrentLap = 1; //Vuelta en la que está
    
    
    GameObject _lobby;  //Variable que almacena la referencia al GameObject que representa el Lobby del juego

    UIManager _ui;              //Variable que almacena la referencia del UIManager del juego
    PlayerInput _playerInput;   //Variable que almacena la referencia al PlayerInput del jugador
    CinemachineVirtualCamera _camera;   //Variable que almacena la referencia a la camara del juego

    SelectCarColorMenu _selectCarColorMenu; //Referencia al componente de la interfaz de seleccion de color del coche
    PlayerReady _playerReadyComponent;      //Referencia al componente de la interfaz de "Estoy Listo"

    //Variables relacionadas con la longitud recorrida por el jugador en la carrera
    public float arcLength;         //Longitud de la pista recorrida
    public float lastArcLengthWD;   //Variable que almacena la ultima medida del recorrido en el que el jugador iba en sentido correcto
    public float lastArcLength;     //Variable que almacena la ultima medida del recorrido del jugador

    //Variables relacionadas con la deteccion de si el jugador va en sentido contrario
    public float wdLimit = 2.5f;    //Cuenta atras que realizara el jugador para avisar que esta yendo en sentido contrario, una vez que se detecte esto.
    public float wdCounter;         //Variable que realiza la cuenta atras antes mencionada
    public bool wrongDirection;     //Booleano que se activa cuando el jugador va en sentido contrario
    public bool lineCrossed;        //Booleano que indica que el jugador ha cruzado la meta en sentido contrario

    public int actualRacePos;       //Variable que almacena la posicion del jugador, se utiliza a la hora de mostrar dicho dato en la interfaz

    //Variables relacionadas con la cuenta atras realizada cuando va a comenzar la carrera
    int countDown = 3;  //Numero de veces que se hara la cuenta atras
    float countFrecuency = 1.5f;    //Cada 1.5 segundos se realiza una cuenta atras
    float passedTime = 0f;  //Variable que lleva el tiempo pasado para la cuenta atras

    //Variable relacionada con el contador de tiempo de la carrera. Ella lleva el tiempo total pasado del mismo
    float auxiliarTimer = 0;   

    //Variable relacionada con el fin del juego. Este booleano se pone a true una vez que el jugador haya terminado el circuito completamente
    public bool hasFinished = false;

    public override string ToString()
    {
        return Name;
    }


    //////////////////////////
    ////CALLBACKS DE UNITY////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void Start()
    {
        //Suscripcion a las variables de red. Concretamente "vuelta actual" y "tiempo final" respectivamente. De esta manera la replica del jugador de cada runtime tendra esta info sincronizada
        currentLapNet.OnValueChanged += OnCurrentLapChange;
        finalTime.OnValueChanged += OnFinalTimeChange;
        
        //Buscamos y guardamos referencias a los GO del Lobby, el UIManager y la camara.
        _lobby = GameObject.Find("Lobby");
        _ui = GameObject.Find("@UIManager").GetComponent<UIManager>();
        _camera = FindAnyObjectByType<CinemachineVirtualCamera>(); //Guardamos una referencia de la camara de CineMachine, buscandola en la jerarquia.

        //Punto de aparicion de los jugadores en el Lobby de la sala.
        transform.position = new Vector3(40f, 0f, -15f);
        //Guardamos la referencia del PlayerInput del prefab del jugador.
        _playerInput = GetComponent<PlayerInput>();       
        //Guardamos la ID del cliente. Este ID es unico y identifica al jugador en todos los runtime
        ID = GetComponent<NetworkObject>().OwnerClientId;

        //Nos interesa que un jugador pueda mover el coche generado por su juego, no el de los demas.
        //Si el jugador es propietario de esta instancia de Player:
        if (IsOwner)
        {
            //Agregamos la instancia del coche a la variable "player" del GameManager
            GameManager.Instance.player = this;
            //Habilitamos su PlayerInput, de manera que pueda controlar su coche.
            _playerInput.enabled = true;
            //Llamada al metodo que se encarga de los preparativos de la cámara cuando el jugador se une a la partida.
            playerSetup();                  
            //Variable donde se almacena el nombre introducido por el jugador en la pantalla de inicio del juego.
            string tempName = _ui.playerName;

            //Si no inserto un nombre, se le asigna uno por defecto. En caso contrario, se le asigna el nombre que escribio.
            if (tempName.Equals("Enter player name..."))
            {
                Name = car.transform.Find("MiniCanvas").transform.Find("Nombre").GetComponent<TextMeshProUGUI>().text + " " + ID;
            }
            else
            {
                Name = tempName;
            }

            //Guardamos el color inicial del coche accediendo mediante la variable "car".
            Color initialCarColor = car.transform.Find("body").gameObject.GetComponent<MeshRenderer>().materials[1].color;
            //Con la informacion disponible hasta el momento, se crea un Struct con el nombre y el color del coche. Este Struct se utiliza
            //para sincronizar estos datos del jugador entre todos los clientes.
            data = new PlayerData(Name, initialCarColor.r, initialCarColor.g, initialCarColor.b);

            //Metodo que añade el struct a una lista de la clase GameManager
            AddPlayer(ID, data);

            //Metodo con el cual el cliente solicita el numero de jugadores "Listos" en la sala. De esta manera puede saber cuantos jugadores
            //estaban listos antes de que este jugador se uniera a la sala
            GetReadyPlayersServerRpc(ID);

            //Si esta instancia de Player es el servidor, suscribe dicha instancia al evento "OnClientDisconnectCallback"
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            }
        }

        //Metodo que solicita la informacion de la instancia a traves de su ID. De esta manera se consigue el Struct de datos de todos los
        //jugadores.
        AskForMyInfo(ID);

        //Si la instancia es el servidor (cuyo ID sera siempre 0). Se suscribira dicha instancia a los cambios de las variables "mapSelectedID" y "gameplayTimer".
        //Esto permitira que esta instancia sea la encargada de sincronizar los valores de dichas variables entre todos los runtime.
        if (ID == 0)
        {
            GameManager.Instance.mapSelectedId = mapSelectedId.Value;
            mapSelectedId.OnValueChanged += OnMapSelected;
            gameplayTimer.OnValueChanged += OnGameplayTimerChange;
        }
    }

    private void Update()
    {
        //Al entrar al Lobby, buscamos la interfaz de seleccion de color y suscribimos un metodo al evento de Cambio de color del coche
        if (_selectCarColorMenu == null && IsOwner && FindAnyObjectByType<SelectCarColorMenu>() != null)
        {
            _selectCarColorMenu = FindAnyObjectByType<SelectCarColorMenu>();
            _selectCarColorMenu.colorSelected += OnColorSelected;
        }

        //De forma similar, buscamos la interfaz de "jugador listo" y nos suscribimos a dicho evento
        if (_playerReadyComponent == null && IsOwner && FindAnyObjectByType<PlayerReady>() != null)
        {
            _playerReadyComponent = FindAnyObjectByType<PlayerReady>();
            _playerReadyComponent.playerReady += OnPlayerReady;
        }

        //Apartado encargado de comprobar continuamente cuantos jugadores hay conectados

        //Si el valor de currentPlayers (numero de jugadores conectados) es distinto del numero de players del diccionario
        if (GameManager.Instance.currentPlayers != GameManager.Instance.players.Count) 
        {
            //Actualiza el numero de jugadores conectados
            GameManager.Instance.currentPlayers = GameManager.Instance.players.Count; 
        }

        //Apartado encargado de comprobar el numero de jugadores listos

        if (IsServer && IsOwner)
            //El host comenzara la partida Cuando mas de la mitad de los jugadores estan listos y haya mas de 1 jugador en la partida.
        {   //Cuando esto ocurra, el host desde su coche realizara lo siguiente:
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
            //Si la carrera ha empezado
            if (GameManager.Instance.gameStarted)
            {   //Si la lista del currentRace no posee al coche, quiere decir que no esta agregado a la lista de la carrera
                if (!GameManager.Instance.currentRace.ContainsPlayer(this))
                {
                    //Reduce la velocidad del coche a 0, lo teletransporta a su posicion correspondiente y ejecuta una llamada rpc 
                    car.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
                    car.transform.position = GameManager.Instance.circuitManager.transform.GetChild(GameManager.Instance.mapSelectedId - 1).Find("StartPos").GetChild((int)ID).transform.position;
                    car.transform.rotation = GameManager.Instance.circuitManager.transform.GetChild(GameManager.Instance.mapSelectedId - 1).Find("StartPos").GetChild((int)ID).transform.rotation;
                    lastArcLengthWD = arcLength;
                    PrepareCircuitClientRpc();
                }
            }
        }

        //Cuando todos los jugadores esten en la partida, antes de que comience la carrera, hacemos que estos no puedan moverse
        if (IsOwner && GameManager.Instance.currentRace._players.Count == GameManager.Instance.currentPlayers && _playerInput.enabled && countDown == 3 && GameManager.Instance.mapSelectedId != 0)
        {
            _playerInput.enabled = false;
        }

        //Cuando los jugadores no puedan moverse y se haya escogido un mapa, pasamos a la fase de preparacion de la carrera.

        //Sin esta condicion GameManager.Instance.currentRace._players.Count == GameManager.Instance.currentPlayers,
        //al ser IsServer, se ejecuta en el Lobby cuando se une un cliente, cosa que no queremos que ocurra
        if (IsServer && !_playerInput.enabled && GameManager.Instance.currentRace._players.Count == GameManager.Instance.currentPlayers && countDown >= 0) 
        {
            //Acumulamos el tiempo pasado para la cuenta atras de inicio de carrera
            passedTime += Time.deltaTime;

            //Cada 1.5 segundos, hacemos una llamada rpc
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
            //auxiliarTimer lleva el tiempo total, luego se redondea y se asigna a la variable de red del coche del host, de manera que en el resto de runtimes se actualizara
            //el tiempo. En el Runtime del propio servidor diretamente asignamos el valor al GameManager
            if (IsServer)
            {
                auxiliarTimer += Time.deltaTime;
                gameplayTimer.Value = (float) Math.Round(auxiliarTimer, 2);
                GameManager.Instance.gameplayTimer = gameplayTimer.Value;
            }

            //Si la distancia recorrida en ese momento es ms pequeña que la anterior y la diferencia entre estos supera un umbral
            //quiere decir que el jugador va en sentido contrario
            if (arcLength < lastArcLengthWD && Mathf.Abs(arcLength - lastArcLengthWD) > 0.01f)
            {
                wrongDirection = true;
            }
            else
            {
                lastArcLengthWD = arcLength;
                wrongDirection = false;
            }

            //Si la diferencia es mayor a 20, quiere decir que ha cruzado la meta en sentido contrario
            if (Mathf.Abs(arcLength - lastArcLength) > 20)
            {
                lastArcLengthWD = lastArcLength + 1;
            }

            //Si va en sentido contrario, activamos la cuenta atras para avisar al jugador mediante un cartel en la interfaz
            if (wrongDirection)
            {
                wdCounter -= Time.deltaTime;
                if (wdCounter < 0)
                {
                    GameObject.Find("GameUI").transform.Find("PanelAtras").gameObject.SetActive(true);
                }
            }
            else //Si no va en sentido contario, reiniciamos la cuenta atras para tenerla preparada si wriongDirection vale true
            {
                wdCounter = wdLimit;
                GameObject.Find("GameUI").transform.Find("PanelAtras").gameObject.SetActive(false);
            }

            //En el caso de que solo quede un jugador en la partida (es decir, solo queda el host), se da una condicion de victoria
            //Se realizan las gestiones necesarias para mostrar la interfaz de resultados
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

    //Metodo que hace la "animacion" del semaforo de la carrera
    [ClientRpc]
    void RaceStartingClientRpc(int phase)
    {
        //Buscamos al semaforo del escenario y hacemos que el jugador mire a este
        Transform LEDS = GameObject.Find("LEDS").transform;
        _camera.LookAt = LEDS.transform;

        //Dependiendo de la cuenta atras, se encenderas las bombillas del escenario
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

                //En la ultima cuenta atras, se habilita el control del jugador propietario a traves del GameManager
                GameManager.Instance.player._playerInput.enabled = true;
                //Reestablecemos el objetivo de la camara
                GameManager.Instance.player._camera.LookAt = (GameManager.Instance.player.car.transform);
                break;
        }
        countDown -= 1;
    }

    //Este metodo se invoca desde cada coche del runtime del host cuando comienza la partida, para ser ejecutado en ese coche del lado del cliente
    [ClientRpc]
    void PrepareCircuitClientRpc()
    {
        //Unicamente si la instancia es propietaria del coche
        if (IsOwner)
        {
            //Marca en el runtime del cliente que el juego comenzo, apaga el lobby y enciende el mapa seleccionado, cambia la interfaz y agrega el jugador a la carrera
            GameManager.Instance.gameStarted = true;
            _lobby.SetActive(false);
            GameManager.Instance.circuitManager.SetActive(true);
            _ui.State = new GameInterfaceState(_ui);
        }
        //Apagamos los carteles de Ready de los jugadores, para que en la carrera solo veamos sus nombres
        car.transform.Find("MiniCanvas/Estado").gameObject.SetActive(false);
        car.transform.Find("MiniCanvas/Estado_Panel").gameObject.SetActive(false);

        GameManager.Instance.currentRace.AddPlayer(this);
    }

    //////////////////////////////////
    ////INICIALIZACIÓN DEL JUGADOR////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    void playerSetup() //Metodo encargado de asignar el prefab del jugador a la camara de CineMachine
    {
        _camera.Follow = car.transform;            //Indicamos que la camara debe seguir a la transformada del coche del prefab.
        _camera.LookAt = car.transform;            //Indicamos que el vector LookAt apunte a la transformada del coche.
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

    //Cuando se desconecta un jugador, se decrementa el numero de jugadores conectados
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
        //Añade el struct a la lista y envia dicho Struct al servidor
        GameManager.Instance.players.TryAdd(id, data);
        AddPlayerServerRpc(id, data);
    }

    [ServerRpc]
    void AddPlayerServerRpc(ulong id, PlayerData data)
    {
        //Añade el struct a la lista de GameManager del servidor y muestra el nombre y estado del jugador en su runtime
        GameManager.Instance.players.TryAdd(id, data);
        Name = data.name;
        car.transform.Find("MiniCanvas").transform.Find("Nombre").GetComponent<TextMeshProUGUI>().text = GameManager.Instance.players[ID].name;
    }

    //Metodo que solicita al servidor la informacion del jugador identificado por su id
    void AskForMyInfo(ulong id)
    {
        AskForMyInfoServerRpc(id);
    }

    //Metodo que recibe un id y envia el struct asociado a ese jugador
    [ServerRpc(RequireOwnership = false)]
    void AskForMyInfoServerRpc(ulong id)
    {
        GetMyInfoClientRpc(id, GameManager.Instance.players[id]);
    }

    //Metodo que se ejecuta en los clientes con la informacion solicitada de un jugador especifico
    [ClientRpc]
    void GetMyInfoClientRpc(ulong id, PlayerData data)
    {
        GameManager.Instance.players.TryAdd(id, data);
        this.data = data;

        //Mostramos el nombre, color y estado del jugador
        car.transform.Find("MiniCanvas").transform.Find("Nombre").GetComponent<TextMeshProUGUI>().text = GameManager.Instance.players[ID].name;
        car.transform.Find("body").gameObject.GetComponent<MeshRenderer>().materials[1].color = new Color(GameManager.Instance.players[ID].colorRed, GameManager.Instance.players[ID].colorGreen, GameManager.Instance.players[ID].colorBlue, GameManager.Instance.players[ID].colorAlpha);
        car.transform.Find("MiniCanvas").transform.Find("Estado").GetComponent<TextMeshProUGUI>().text = GameManager.Instance.players[ID].status;
        Name = data.name;
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    ///////////////////////
    ////CAMBIO DE COLOR////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
   
    //Metodo que se activa con el evento de cambio de color de la interfaz
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

    //Metodo que envia al servidor el struct del jugador especificado con el cambio de color
    [ServerRpc]
    void SendDataServerRpc(ulong id, PlayerData data)
    {
        GameManager.Instance.players[id] = data;
        this.data = data;

        GetMyColorClientRpc(id, data);
    }

    //Metodo que se ejecuta en los clientes con los datos actualizados de color
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
   
    //Metodo que se llamada cuando un jugador pulsa el boton de "Ready"
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

    //Metodo que envia al servidor el cambio de estado de los jugadores cuando pulsan el boton "Ready"
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

    //Metodo que recibe los jugadores "Listos"
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
        //El servidor solo mandara la respuesta al cliente que invoco este metodo en el servidor
        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { newPlayerID }
            }
        };
        //Invoca al metodo en el cliente, pasando el numero de jugadores listos
        getReadyPlayersClientRpc(GameManager.Instance.readyPlayers, clientRpcParams);
    }

    //Metodo que envia el numero de jugadores listos
    [ClientRpc]
    void getReadyPlayersClientRpc(int readyPlayers, ClientRpcParams param)
    { 
        GameManager.Instance.readyPlayers = readyPlayers;
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    /////////////////////////
    ////MAPA SELECCIONADO////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    //Cuando se selecciona un mapa en el Host, las istancias suscritas ejecutan este metodo con el cambio de la variable de red 
    void OnMapSelected(int previousValue, int newValue)
    {
        if (IsServer) { return; }

        GameManager.Instance.mapSelectedId = newValue;
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //Metodo que se ejecuta en el servidor cada vez que el jugador pasa la meta correctamente
    [ServerRpc]
    public void UpdateCurrentLapServerRpc()
    {
        currentLapNet.Value += 1;
        CurrentLap += 1;

        //Si completa el circuito, se realizan las cambios necesarios para mostar la interfaz de resultados
        if (CurrentLap >= 4)
        {
            car.transform.position = new Vector3(1000, -1000, 1000);
            Rigidbody carRb = car.GetComponent<Rigidbody>();
            carRb.constraints = RigidbodyConstraints.FreezeAll;
            arcLength = float.MaxValue - GameManager.Instance.playersFinished;
            hasFinished = true;

            //Envia a los clientes que el juagdor ha termiando la carrera y su medida de arcLength
            PlayerHasFinishedClientRpc(arcLength, hasFinished);
        }
        //Metodo que actualiza la interfaz con los tiempos de cada vuelta en la carrera
        SetPartialTimes();
    }

    //Cuando se suma una vuelta, las istancias suscritas ejecutan este metodo con el cambio de la variable de red 
    void OnCurrentLapChange(int previousValue, int newValue)
    {
        if (IsServer) { return; }

        CurrentLap = newValue;
        SetPartialTimes();
    }

    //Metodo que se llama con la sincronizacion del tiempo de la partida, que es una variable de red
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

    //Metodo que se llama cuando el jugador termina la carrera, pasandole al servidor la medida del tiempo cuando esto ocurrio
    [ServerRpc]
    void SendFinalTimeServerRpc(float totalTime)
    {
        finalTime.Value = totalTime;
    }

    void OnFinalTimeChange(float previousValue, float newValue)
    {
        finalTime.Value = newValue;
    }

    //Metodo que se llama cuando un jugador termina la carrera.
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