using System.Collections.Concurrent;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;

    public ConcurrentDictionary<ulong, PlayerData> players = new ConcurrentDictionary<ulong, PlayerData>();

    public bool gameStarted = false;

    public int numPlayers = 6;

    public int playersFinished = 0;

    public int currentPlayers;

    public int readyPlayers;

    public int mapSelectedId = 0;

    public RaceController currentRace;

    public GameObject circuitManager;

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
