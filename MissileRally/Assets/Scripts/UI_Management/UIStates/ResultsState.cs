using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultsState : AUIState
{
    GameObject resultsUI;   //Referencia al GameObject que contiene la interfaz de resultados

    public ResultsState(IUI UI) : base(UI)
    {
    }

    // Al entrar en este estado se activa la interfaz de resultados para que sea visible
    public override void Enter()
    {
        resultsUI = UI.Canvas.transform.Find("ResultsUI").gameObject;
        resultsUI.SetActive(true);
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
