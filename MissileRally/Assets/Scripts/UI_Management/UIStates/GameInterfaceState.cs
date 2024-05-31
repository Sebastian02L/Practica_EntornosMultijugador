using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//Estado de la interfaz del juego en sí, durante la partida.
public class GameInterfaceState : AUIState
{
    GameObject gameUI; //Referencia al objeto de la interfaz que representa la interfaz del juego.
    TextMeshProUGUI speedText; //Referencia al texto que mostrará la velocidad del coche del jguador.
    TextMeshProUGUI positionText; //Referencia al texto que mostrará la posición del jugador en la carrera.
    TextMeshProUGUI lapText; //Referencia al texto que indica la vuelta por la que va el jugador.
    TextMeshProUGUI totalTimeText; //Referencia al texto que muestra el tiempo total transcurrido durante la partida.
    CarController carController; //Referencia al CarController del jugador para poder acceder a su velocidad y mostrarla en la interfaz.
    int racePosition;

    public GameInterfaceState(IUI UI) : base(UI)
    {
    }

    public override void Enter()
    {
        gameUI = UI.Canvas.transform.Find("GameUI").gameObject;
        speedText = gameUI.transform.Find("Panel").Find("SpeedText").GetComponent<TextMeshProUGUI>();
        positionText = gameUI.transform.Find("PanelPos").Find("PositionText").GetComponent<TextMeshProUGUI>();
        lapText = gameUI.transform.Find("PanelLap").Find("LapCounter").GetComponent<TextMeshProUGUI>();
        totalTimeText = gameUI.transform.Find("PanelTimes").Find("TotalTime").GetComponent<TextMeshProUGUI>();

        gameUI.SetActive(true); //Al entrar en el estado se activa la interfaz del juego.
        carController = GameManager.Instance.player.car.GetComponent<CarController>(); //Se accede al CarController del coche del jugador del runtime.
    }

    public override void Exit()
    {
        gameUI.SetActive(false); //Al salir del estado se desactiva la interfaz del juego.
    }

    public override void FixedUpdate()
    {

    }

    public override void Update()
    {
        if (carController != null)
        {
            int speed = (int)carController.Speed * (3600/1000); //Se accede a la velocidad en el CarController en cada frame y se pasa de m/s a km/h.
            speedText.text = speed.ToString() + " km/h"; //Se muestra la velocidad calculada en el texto.
        }

        racePosition = GameManager.Instance.player.actualRacePos; //Se accede a la posición actual del jugador del runtime, almacenada en su objeto accesible desde el GameManager.
        positionText.text = racePosition.ToString() + "º"; //Se muestra la posición en el texto.

        lapText.text = "Lap " + GameManager.Instance.player.CurrentLap + " / 3"; //Del mismo modo, se muestra la vuelta actual del jugador del runtime.

        totalTimeText.text = "Total Time: " + GameManager.Instance.gameplayTimer + " s"; //Se muestra el tiempo total de partida almacenado en el GameManager.

        if (GameManager.Instance.player.hasFinished) //Si el jugador del runtime ha terminado la carrera, pasa a la pantalla de resultados.
        {
            UI.State = new ResultsState(UI);
        }
    }
}
