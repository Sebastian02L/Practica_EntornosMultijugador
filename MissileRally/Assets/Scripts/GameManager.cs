using System.Collections.Concurrent;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player; // Referencia al player que es el owner 

    public ConcurrentDictionary<ulong, PlayerData> players = new ConcurrentDictionary<ulong, PlayerData>(); // Diccionario donde se guardan los jugadores por su clientId y que contienen el data de los jugadores

    public bool gameStarted = false; // Variable que indica que el la partida ha comenzado

    public int numPlayers = 6; // Número máximo de jugadores

    public int playersFinished = 0; // Número de jugadores que han terminado la carrera

    public int currentPlayers; // Número de jugadores conectados

    public int readyPlayers; // Número de jugadores listos

    public int mapSelectedId = 0; // Id del mapa seleccionado: 1-NASCAR 2-OASIS 3-RAINY 4-OWL PLAINS 

    public RaceController currentRace; // Referencia a la carrera

    public GameObject circuitManager; // Referencia al circuit manager

    //Variable donde se almacena el valor de la variable de red que lleva el cronometro de la partida
    //Simplemente se hace una asignacion, no se hace nada mas en este script.
    public float gameplayTimer;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }
    

    

}
