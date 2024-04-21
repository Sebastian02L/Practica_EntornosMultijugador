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
    //InputController inputController;
    PlayerInput playerInput;
    CinemachineVirtualCamera camara;

    public override string ToString()
    {
        return Name;
    }

    private void Start()
    {
        GameManager.Instance.currentRace.AddPlayer(this);
        playerInput = GetComponent<PlayerInput>();
        //inputController = GetComponent<InputController>();

        if (IsOwner)
        {
            playerSetup();
            playerInput.enabled = true;
        }
    }

    void playerSetup()
    {
        //Asignacion de objetivo de camara
        camara = FindAnyObjectByType<CinemachineVirtualCamera>();
        camara.Follow = car.transform;
        camara.LookAt = car.transform;

        //playerInput.actions.FindAction("Move").performed += ctx => inputController.OnMove(ctx);
    }
}