using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Unity.Netcode;
using Cinemachine;


public class Player : NetworkBehaviour
{
    // Player Info
    public string Name { get; set; }
    public int ID { get; set; }

    // Race Info
    public GameObject car;
    public int CurrentPosition { get; set; }
    public int CurrentLap { get; set; } //Vuelta en la que está

    PlayerInput _playerInput;
    CinemachineVirtualCamera _camera;

    public override string ToString()
    {
        return Name;
    }

    private void Start()
    {
        GameManager.Instance.currentRace.AddPlayer(this); //Agregamos un jugador nuevo a la carrera.
        _playerInput = GetComponent<PlayerInput>();       //Guardamos la referencia del PlayerInput del prefab del jugador.
        Name = GameObject.Find("@UIManager").GetComponent<UIManager>().playerName; //

        //Nos interesa que un jugador pueda mover el coche generado por su juego, no el de los demas, por lo tanto, si es propietario del coche:
        if (IsOwner)
        {
            playerSetup();                  //Llamada al metodo que se encarga de los preparativos cuando el juagdor se une a la partida.
            _playerInput.enabled = true;    //Habilitamos su PlayerInput, de manera que pueda controlar su coche.
        }
    }

    //Metodo encargado de asignar el prefab del jugador a la camara de ChineMachine
    void playerSetup()
    {
        _camera = FindAnyObjectByType<CinemachineVirtualCamera>(); //Guardamos una referencia de la camara de CineMachine, buscandola en la jerarquia.
        _camera.Follow = car.transform;                            //Indicamos que la camara debe seguir a la transformada del coche del prefab.
        _camera.LookAt = car.transform;                            //Indicamos que el vector LookAt apunte a la transformada del coche.
    }
}