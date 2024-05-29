using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameInterfaceState : AUIState
{
    GameObject gameUI;
    TextMeshProUGUI speedText;
    CarController carController;

    public GameInterfaceState(IUI UI) : base(UI)
    {
    }

    public override void Enter()
    {
        gameUI = UI.Canvas.transform.Find("GameUI").gameObject;
        speedText = gameUI.transform.Find("Panel").Find("SpeedText").GetComponent<TextMeshProUGUI>();
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
    }
}
