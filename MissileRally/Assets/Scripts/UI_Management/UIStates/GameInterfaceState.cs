using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameInterfaceState : AUIState
{
    GameObject gameUI;
    TextMeshProUGUI speedText;

    public GameInterfaceState(IUI UI) : base(UI)
    {
    }

    public override void Enter()
    {
        gameUI = UI.Canvas.transform.Find("GameUI").gameObject;
        speedText = gameUI.transform.Find("Panel").Find("SpeedText").GetComponent<TextMeshProUGUI>();
        gameUI.SetActive(true);
    }

    public override void Exit()
    {

    }

    public override void FixedUpdate()
    {

    }

    public override void Update()
    {
        int speed = (int)GameManager.Instance.player.car.GetComponent<CarController>().Speed;
        Debug.Log(speed);
        speedText.text = speed.ToString() + " km/h";
    }
}
