using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSelectionState : AUIState
{
    GameObject colorSelectionUI;

    public ColorSelectionState(IUI UI) : base(UI)
    {
    }

    public override void Enter()
    {
        colorSelectionUI = UI.Canvas.transform.Find("SelectCarColorMenu").gameObject;
        colorSelectionUI.SetActive(true);
    }

    public override void Exit()
    {
    }

    public override void FixedUpdate()
    {
    }

    public override void Update()
    {
    }
}
