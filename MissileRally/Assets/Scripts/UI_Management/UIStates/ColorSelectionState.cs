using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSelectionState : AUIState
{
    GameObject colorSelectionUI;

    public ColorSelectionState(IUI UI) : base(UI)
    {
    }

    public override void Enter() //Cuando se entra en este estado se activa el men� de selecci�n de color
    {
        colorSelectionUI = UI.Canvas.transform.Find("SelectCarColorMenu").gameObject;
        colorSelectionUI.SetActive(true);
    }

    public override void Exit() // Al salir del estado se desactiva el men�
    {
        colorSelectionUI.SetActive(false);
    }

    public override void FixedUpdate()
    {
    }

    public override void Update()
    {
    }
}
