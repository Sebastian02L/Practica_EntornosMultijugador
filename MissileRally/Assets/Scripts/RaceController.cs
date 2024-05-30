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
        List<Player> players;

        public PlayerInfoComparer(List<Player> playersP)
        {
            players = playersP;
        }

        public override int Compare(Player x, Player y)
        {
            if (x.arcLenght < y.arcLenght)
                return 1;
            else return -1;
        }
    }

    public void UpdateRaceProgress()
    {
        // Update car arc-lengths
        float[] arcLengths = new float[_players.Count];

        for (int i = 0; i < _players.Count; ++i)
        {
            _players[i].arcLenght = ComputeCarArcLength(i);
        }
        //Ordenamos la lista de jugadores, segun el valor de su atributo arclength.
        //Esto hace que su orden dentro de ls lista sea el orden de la carrera.
        _players.Sort(new PlayerInfoComparer(_players));  //Esta linea determina el orden de los jugadores
 
        //Mostramos un string con el orden de carrera (para debuggear)
        string myRaceOrder = "";

        for (int i = 0; i < _players.Count; ++i)
        {
            _players[i].actualRacePos = i + 1;

            myRaceOrder += _players[i].Name + " ";
        }

        //Debug.Log("Race order: " + myRaceOrder);
    }

    float ComputeCarArcLength(int id)
    {
        // Compute the projection of the car position to the closest circuit 
        // path segment and accumulate the arc-length along of the car along
        // the circuit.
        Vector3 carPos = Vector3.zero;
        if (this._players[id].car)
        {
            carPos = this._players[id].car.transform.position;  //Inicializa con la posicion del coche
        }

        //Calcula la longitud del arco 
        float minArcL =
            this._circuitController.ComputeClosestPointArcLength(carPos, out _, out var carProj, out _);

        //Actualiza la posicion de la esfera en la linea
        this._debuggingSpheres[id].transform.position = carProj;

        //Ajusta la longitud del arco segun la vuelta en la que se encuentra el coche
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