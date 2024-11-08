using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyState : AUIState
{
    GameObject lobbyInterface;
    TextMeshProUGUI playersCounterText; //Referencia al texto de la interfaz con el numero de jugadores conectados
    TextMeshProUGUI messageStatusText;  //Referencia al texto de la interfaz que indica si se ha seleccionado el mapa y esperando a que los jugadores esten listos
    TextMeshProUGUI playersReadyText;   //Referencia al texto de la interfaz que indica el n�mero de jugadores listos del lobby 
    int playersCounter; //Variable que almacena el numero de jugadores conectados
    string messageStatus; //Variable que almacena el estado del Lobby (esperando mapa y que los jugadores esten listos)
    int playersReadyCounter; //

    bool statusChanged = false;

    public LobbyState(IUI UI) : base(UI)
    {
    }

    public override void Enter()
    {
        //Se buscan todas las referencias a objetos y se activa la interfaz necesaria
        lobbyInterface = UI.Canvas.transform.Find("LobbyUI").gameObject;
        lobbyInterface.SetActive(true);
        playersCounterText = lobbyInterface.transform.Find("Panel1").Find("Counter").GetComponent<TextMeshProUGUI>();
        messageStatusText = lobbyInterface.transform.Find("Panel2").Find("MessageStatus").GetComponent<TextMeshProUGUI>();
        playersReadyText = lobbyInterface.transform.Find("Panel3").Find("Counter").GetComponent<TextMeshProUGUI>();

        if (GameManager.Instance.mapSelectedId == 0) //Si el host de la partida na ha seleccionado un mapa todav�a, el texto de la interfaz de todos los jugadores cambia
        {
            messageStatus = "Esperando a que el host seleccione un mapa...";
            messageStatusText.text = messageStatus;
        }
    }

    public override void Exit() //Al salir del estado,se desactiva la interfaz
    {
        lobbyInterface.SetActive(false);
    }

    public override void FixedUpdate()
    {
    }

    public override void Update() //Se actualizan todos los contadores de la interfaz
    {
        playersCounter = GameManager.Instance.currentPlayers;
        playersReadyCounter = GameManager.Instance.readyPlayers;

        //playersCounter debe actualizar su valor al del numero de jugadores conectados
        playersCounterText.text = playersCounter.ToString();
        //Actualizamos el numero de jugadores listos de la partida
        playersReadyText.text = $"{playersReadyCounter} / {playersCounter}";

        if (!statusChanged && GameManager.Instance.mapSelectedId != 0) //Si el host de la partida ya ha seleccionado un mapa, el texto de la interfaz de todos los jugadores cambia
        {
            statusChanged = true;
            messageStatus = "Esperando a que los jugadores est�n listos...";
            messageStatusText.text = messageStatus;
        }
    }
}
