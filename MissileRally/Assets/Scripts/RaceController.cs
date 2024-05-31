using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceController : MonoBehaviour
{
    public int numPlayers;

    public readonly List<Player> _players = new(6);
    private CircuitController _circuitController;
    private GameObject[] _debuggingSpheres;

    public void Start()
    {
        if (_circuitController == null) _circuitController = GetComponent<CircuitController>();
        if (_debuggingSpheres == null)
        {
            _debuggingSpheres = new GameObject[GameManager.Instance.numPlayers];
            for (int i = 0; i < GameManager.Instance.numPlayers; ++i) //Se instancian la esferas de debug (Solo para cuestiones de debugging)
            {
                _debuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _debuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
                _debuggingSpheres[i].GetComponent<MeshRenderer>().enabled = false;
            }
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

    public bool ContainsPlayer(Player player)
    {
        return _players.Contains(player);
    }
    
    public void UpdateRaceProgress()
    {
        for (int i = 0; i < _players.Count; ++i)
        {
            if (!_players[i].hasFinished) //El cálculo solo se hace si el jugador no ha terminado la carrera
            {
                _players[i].arcLength = ComputeCarArcLength(i);
                if (Mathf.Abs(_players[i].arcLength - _players[i].lastArcLength) > 20) 
                {
                    // Si se produce un cambio muy brusco en el arclenth del jugador quiere decir que ha cruzado la linea de meta pero no ha sumado una vuelta.
                    // Esto significa que la ha cruzado en el sentido contrario
                    _players[i].arcLength = _players[i].lastArcLength; // Al ocurrirse el caso, no se actualiza su arcLength y por tanto no seproduce ningun error al calcular las posiciones de los jugadores en la carrera
                    _players[i].lineCrossed = true;
                }
                else
                {
                    _players[i].lastArcLength = _players[i].arcLength;  // lastArcLength lo utilizamos para guardar el valor de arcLength del frame anterior, así se puede comprovar la diferencia producida entre el arcLength nuevo y el antiguo
                    _players[i].lineCrossed = false;
                }
            }
        }
    
        //Ordenamos la lista de jugadores, segun el valor de su atributo arclength.
        //Esto hace que su orden dentro de ls lista sea el orden de la carrera.
        _players.Sort((x, y) =>
        {
            if (x.arcLength < y.arcLength)
                return 1;
            else return -1;
        }); //Esta linea determina el orden de los jugadores

        for (int i = 0; i < _players.Count; ++i)
        {
            _players[i].actualRacePos = i + 1;
        }
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
            minArcL += _circuitController.CircuitLength * (_players[id].CurrentLap - 1);
        }

        return minArcL;
    }
}