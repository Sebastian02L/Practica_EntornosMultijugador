using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceController : MonoBehaviour
{
    public int numPlayers;

    private readonly List<Player> _players = new(6);
    private CircuitController _circuitController;
    private GameObject[] _debuggingSpheres;

    private void Start()
    {
        if (_circuitController == null) _circuitController = GetComponent<CircuitController>();

        _debuggingSpheres = new GameObject[GameManager.Instance.numPlayers];
        for (int i = 0; i < GameManager.Instance.numPlayers; ++i)
        {
            _debuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _debuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        }

        foreach(var player in _players)
        {
            SpherePlayer(player);
        }
    }

    private void Update()
    {
        if (_players.Count == 0)
            return;

        UpdateRaceProgress();
    }

    public void AddPlayer(Player player)
    {
        Debug.Log("Añadiendo al jugador: " + player.ID);
        _players.Add(player);
    }

    public void SpherePlayer(Player player)
    {
        player.spherePosition = _debuggingSpheres[player.ID].transform; //Asociamos la esfera correspondiente al jugador
        //player.gameObject.AddComponent<RecoverComponent>();
    }

    public bool ContainsPlayer(Player player)
    {
        return _players.Contains(player);
    }

    private class PlayerInfoComparer : Comparer<Player>
    {
        readonly float[] _arcLengths;

        public PlayerInfoComparer(float[] arcLengths)
        {
            _arcLengths = arcLengths;
        }

        public override int Compare(Player x, Player y)
        {
            if (_arcLengths[x.ID] < _arcLengths[y.ID])
                return 1;
            else return -1;
        }
    }

    public void UpdateRaceProgress()
    {
        Debug.Log("Numero de jugadores en la lista: " + _players.Count);
        // Update car arc-lengths
        float[] arcLengths = new float[_players.Count];

        for (int i = 0; i < _players.Count; ++i)
        {
            arcLengths[i] = ComputeCarArcLength(i);
        }

        //_players.OrderBy(x => arcLengths[x.ID]);//Prueba de orden de los jugadores

        //_players.Sort(new PlayerInfoComparer(arcLengths));  //Esta linea determina el orden de los jugadores
        _players.Sort((x, y) =>
        {
            if (arcLengths[x.ID] < arcLengths[y.ID])
                return 1;
            else return -1;
        });

        //print(_players[0].ID + " " + _players[1].ID);

        string myRaceOrder = "";
        foreach (Player player in _players)
        {
            Debug.Log("Jugador iterado: " + player.ID + "y nombre" + player.Name);
            myRaceOrder += player.Name + " ";
            Debug.Log("Valor post aumento" + myRaceOrder);
        }

        Debug.Log("Race order: " + myRaceOrder);
    }

    float ComputeCarArcLength(int id)
    {
        // Compute the projection of the car position to the closest circuit 
        // path segment and accumulate the arc-length along of the car along
        // the circuit.
        Vector3 carPos = Vector3.zero;
        if (this._players[id].car)
        {
            carPos = this._players[id].car.transform.position;
        }

        float minArcL =
            this._circuitController.ComputeClosestPointArcLength(carPos, out _, out var carProj, out _);

        this._debuggingSpheres[id].transform.position = carProj;

        if (this._players[id].CurrentLap == 0)
        {
            minArcL -= _circuitController.CircuitLength;
        }
        else
        {
            minArcL += _circuitController.CircuitLength *
                       (_players[id].CurrentLap - 1);
        }

        return minArcL;
    }
}