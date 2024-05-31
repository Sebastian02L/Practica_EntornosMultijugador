using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceController : MonoBehaviour
{
    public int numPlayers;

    public readonly List<Player> _players = new(6); // Lista con las referencias de los players para la carrera
    private CircuitController _circuitController; // Referencia al componente circuit controller
    private GameObject[] _debuggingSpheres; 

    public void Start()
    {
        if (_circuitController == null) _circuitController = GetComponent<CircuitController>(); // Si no tiene la referencia al circuit controller, se la guarda
        if (_debuggingSpheres == null)
        {
            _debuggingSpheres = new GameObject[GameManager.Instance.numPlayers];
            for (int i = 0; i < GameManager.Instance.numPlayers; ++i) //Se instancian la esferas de debug (Solo para cuestiones de debugging, por lo que se oculta su mesh renderer)
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

        UpdateRaceProgress(); // Si hay jugadores en la carrera se llama a un método que actua como Update()
    }

    public void AddPlayer(Player player)
    {
        _players.Add(player); // Se añade un player a la lista de players
    }

    public bool ContainsPlayer(Player player)
    {
        return _players.Contains(player); // Devuelve true si el player se encuentra en la lista de players
    }
    
    public void UpdateRaceProgress()
    {
        for (int i = 0; i < _players.Count; ++i) // Por cada jugador se va a calcular la distancia recorrida del circuito
        {
            if (!_players[i].hasFinished) //El cálculo solo se hace si el jugador no ha terminado la carrera
            {
                _players[i].arcLength = ComputeCarArcLength(i); // Calcula esa distancia
                if (Mathf.Abs(_players[i].arcLength - _players[i].lastArcLength) > 20) // Si se produce un cambio muy brusco en el arclenth del jugador quiere decir que ha cruzado la linea de meta en sentido contrario
                {
                    _players[i].arcLength = _players[i].lastArcLength; // Al ocurrirse el caso, no se actualiza su arcLength y por tanto no se produce ningún error al calcular las posiciones de los jugadores en la carrera
                    _players[i].lineCrossed = true; // Se pone a true la variable que indica que el jugador ha realizado dicha acción
                }
                else
                {
                    _players[i].lastArcLength = _players[i].arcLength;  // lastArcLength lo utilizamos para guardar el valor de arcLength del frame anterior, así se puede comprovar la diferencia producida entre el arcLength nuevo y el antiguo
                    _players[i].lineCrossed = false; // Se pone a false la variable que indica que el jugador ha realizado dicha acción
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
        }); 

        // Por cada jugador de la lista, al estar ordenados, guardamos su posición en función del índice que tienen en la lista
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
            carPos = this._players[id].car.transform.position;  //Inicializa con la posición del coche
        }

        //Calcula la longitud del arco 
        float minArcL =
            this._circuitController.ComputeClosestPointArcLength(carPos, out _, out var carProj, out _);

        //Actualiza la posición de la esfera en la línea
        this._debuggingSpheres[id].transform.position = carProj;

        //Ajusta la longitud del arco según la vuelta en la que se encuentra el coche
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