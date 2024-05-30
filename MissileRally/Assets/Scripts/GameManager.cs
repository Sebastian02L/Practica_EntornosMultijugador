using System.Collections.Concurrent;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;

    public ConcurrentDictionary<ulong, PlayerData> players = new ConcurrentDictionary<ulong, PlayerData>();

    public bool gameStarted = false;

    public int numPlayers = 6;

    public int currentPlayers;

    public int readyPlayers;

    public int mapSelectedId = 0;

    public RaceController currentRace;

    public GameObject circuitManager;

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
