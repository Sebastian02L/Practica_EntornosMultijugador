using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultsScreen : MonoBehaviour
{
    public TextMeshProUGUI[] playerNames; //Array con las posiciones donde van los nombres de los jugadores
    public TextMeshProUGUI[] playerTimes; //Array con las posiciones donde van los tiempos de los jugadores

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        //Cuando un jugador acabe la carrera, se transicionara a esta interfaz, donde se pondrá a actualizar el nombre y los tiempos.
        for (int i = 0; i < GameManager.Instance.currentRace._players.Count; i++)
        {
            Player player = GameManager.Instance.currentRace._players[i];

            if (player != null && player.hasFinished && GameManager.Instance.currentPlayers > 1)
            {
                playerNames[i].text = $"{i + 1}. {player.Name}";
                playerTimes[i].text = $"Total Time: {player.finalTime.Value} s";
            }
            else
            {
                playerNames[0].text = $"{1}. {player.Name}";
                playerTimes[0].text = $"Por Abandono";
            }
        }

    }
}
