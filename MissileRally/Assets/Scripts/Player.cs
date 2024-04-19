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
    PlayerInput playerInput;
    CinemachineVirtualCamera camara;

    public override string ToString()
    {
        return Name;
    }

    private void Start()
    {
        GameManager.Instance.currentRace.AddPlayer(this);
        playerInput = FindObjectOfType<PlayerInput>();

        if (!IsOwner)
        {
            playerInput.enabled = false;
        } 
        else
        {
            playerSetup();
        }
    }

    void playerSetup()
    {
        //Asignacion de objetivo de camara
        camara = FindAnyObjectByType<CinemachineVirtualCamera>();
        camara.Follow = transform;

        //
        //playerInput.currentActionMap
    }
}