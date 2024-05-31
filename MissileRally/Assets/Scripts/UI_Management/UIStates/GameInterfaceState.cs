using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameInterfaceState : AUIState
{
    GameObject gameUI;
    TextMeshProUGUI speedText;
    TextMeshProUGUI positionText;
    TextMeshProUGUI lapText;
    TextMeshProUGUI totalTimeText;
    TextMeshProUGUI lapOneTimeText;
    TextMeshProUGUI lapTwoTimeText;
    TextMeshProUGUI lapThreeTimeText;
    CarController carController;
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

        gameUI.SetActive(true);
        carController = GameManager.Instance.player.car.GetComponent<CarController>();
    }

    public override void Exit()
    {

    }

    public override void FixedUpdate()
    {

    }

    public override void Update()
    {
        if (carController != null)
        {
            int speed = (int)carController.Speed * (3600/1000);
            speedText.text = speed.ToString() + " km/h";
        }

        racePosition = GameManager.Instance.player.actualRacePos;
        positionText.text = racePosition.ToString() + "º";

        lapText.text = "Lap " + GameManager.Instance.player.CurrentLap + " / 3";

        totalTimeText.text = "Total Time: " + GameManager.Instance.gameplayTimer + " s";
    }
}
