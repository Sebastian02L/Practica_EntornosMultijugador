using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyState : AUIState
{
    GameObject lobbyInterface;
    TextMeshProUGUI playersCounterText; //Referencia al texto de la interfaz con el numero de jugadores conectados
    TextMeshProUGUI messageStatusText;  //Referencia al texto de la interfaz que indica si se ha seleccionado el mapa y esperando a que los jugadores esten listos
    int playersCounter; //Variable que almacena el numero de jugadores conectados
    string messageStatus; //Variable que almacena el estado del Lobby (esperando mapa y que los jugadores esten listos)

    public LobbyState(IUI UI) : base(UI)
    {
    }

    public override void Enter()
    {
        UI.Canvas.transform.Find("SelectMapMenu").GetComponent<SelectMapMenu>().mapSelected += OnMapSelected;
        lobbyInterface = UI.Canvas.transform.Find("LobbyUI").gameObject;
        lobbyInterface.SetActive(true);
        playersCounterText = lobbyInterface.transform.Find("Panel1").Find("Counter").GetComponent<TextMeshProUGUI>();
        messageStatusText = lobbyInterface.transform.Find("Panel2").Find("MessageStatus").GetComponent<TextMeshProUGUI>();

        if (!GameManager.Instance.mapSelected)
        {
            messageStatus = "Esperando a que el host seleccione un mapa...";
            messageStatusText.text = messageStatus;
        }
    }

    public override void Exit()
    {
        lobbyInterface.SetActive(false);
    }

    public override void FixedUpdate()
    {
    }

    public override void Update()
    {

        playersCounter = GameManager.Instance.currentPlayers;

        //playersCounter debe actualizar su valor al del numero de jugadores conectados
        playersCounterText.text = playersCounter.ToString();
    }

    void OnMapSelected()
    {
        messageStatus = "Esperando a que los jugadores est�n listos...";
        messageStatusText.text = messageStatus;
    }
}
