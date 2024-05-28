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

    public PlayerData(string name, float r, float g, float b, float a = 1.0f)
    {
        this.name = name;
        this.colorRed = r;
        this.colorGreen = g;
        this.colorBlue = b;
        this.colorAlpha = a;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref colorRed);
        serializer.SerializeValue(ref colorGreen);
        serializer.SerializeValue(ref colorBlue);
        serializer.SerializeValue(ref colorAlpha);
    }

    public override string ToString()
    {
        return name;
    }
}

public class Player : NetworkBehaviour
{
    // Player Info
    PlayerData data;
    public string Name { get; set; }
    public Color color;
    public ulong ID { get; set; }

    // Race Info
    public GameObject car;
    public int CurrentPosition { get; set; }
    public int CurrentLap { get; set; } //Vuelta en la que está

    PlayerInput _playerInput;
    CinemachineVirtualCamera _camera;

    //Transformada de la esfera blanca asociada al jugador. Cuando el jugador se desvuelca, se teletransporta a ella.
    public Transform spherePosition;

    public override string ToString()
    {
        return Name;
    }

    private void Start()
    {
        _camera = FindAnyObjectByType<CinemachineVirtualCamera>(); //Guardamos una referencia de la camara de CineMachine, buscandola en la jerarquia.
        transform.position = new Vector3(40f, 0f, -15f);  //Punto de aparicion del Lobby de la sala.

        GameManager.Instance.currentRace.AddPlayer(this); //Agregamos un jugador nuevo a la carrera.

        _playerInput = GetComponent<PlayerInput>();       //Guardamos la referencia del PlayerInput del prefab del jugador.

        ID = GetComponent<NetworkObject>().NetworkObjectId;

        //Nos interesa que un jugador pueda mover el coche generado por su juego, no el de los demas, por lo tanto, si es propietario del coche:
        if (IsOwner)
        {
            GameManager.Instance.player = this;
            _playerInput.enabled = true;    //Habilitamos su PlayerInput, de manera que pueda controlar su coche.

            playerSetup();                  //Llamada al metodo que se encarga de los preparativos de la cámara cuando el jugador se une a la partida.

            string tempName = GameObject.Find("@UIManager").GetComponent<UIManager>().playerName;

            if (tempName.Equals("Enter player name..."))
            {
                Name = car.transform.Find("MiniCanvas").transform.Find("Nombre").GetComponent<TextMeshProUGUI>().text + " " + ID;
            }
            else
            {
                Name = tempName;
            }

            data = new PlayerData(Name, 1.0f, 0.0f, 0.0f);

            AddPlayer(ID, data);

            car.transform.Find("MiniCanvas").transform.Find("Nombre").GetComponent<TextMeshProUGUI>().text = Name;
        }
        else
        {
            AskForMyInfo(ID);
        }
    }

    //Metodo encargado de asignar el prefab del jugador a la camara de CineMachine
    void playerSetup()
    {
        _camera.Follow = car.transform;                            //Indicamos que la camara debe seguir a la transformada del coche del prefab.
        _camera.LookAt = car.transform;                            //Indicamos que el vector LookAt apunte a la transformada del coche.
    }

    [ServerRpc]
    void AddPlayerServerRpc(ulong id, PlayerData data)
    {
        GameManager.Instance.players.TryAdd(id, data);
        //car.transform.Find("MiniCanvas").transform.Find("Nombre").GetComponent<TextMeshProUGUI>().text = GameManager.Instance.players[ID].name;
    }

    void AddPlayer(ulong id, PlayerData data)
    {
        AddPlayerServerRpc(id, data);
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
        car.transform.Find("MiniCanvas").transform.Find("Nombre").GetComponent<TextMeshProUGUI>().text = GameManager.Instance.players[ID].name;
    }
}