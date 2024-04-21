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
        GameManager.Instance.currentRace.AddPlayer(this);

        _playerInput = GetComponent<PlayerInput>();

        if (IsOwner)
        {
            playerSetup();
            _playerInput.enabled = true;
        }
    }

    void playerSetup()
    {
        //Asignacion de objetivo de camara
        _camera = FindAnyObjectByType<CinemachineVirtualCamera>();
        _camera.Follow = car.transform;
        _camera.LookAt = car.transform;
    }
}