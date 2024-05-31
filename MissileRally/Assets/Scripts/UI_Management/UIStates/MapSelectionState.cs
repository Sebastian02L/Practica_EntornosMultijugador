using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

internal class MapSelectionState : AUIState
{
    GameObject MapSelectionMenu; // Referencia al objeto del canvas llamado "SelectMapMenu"

    public MapSelectionState(IUI UI) : base(UI)
    {
    }

    // Al entrar en este estado obtenemos la referencia del objeto SelectMapMenu y activamos dicho objeto para hacerlo visible
    public override void Enter()
    {
        MapSelectionMenu = UI.Canvas.transform.Find("SelectMapMenu").gameObject;
        MapSelectionMenu.SetActive(true);
    }

    // Al salir del estado desactivamos el objeto SelectMapMenu para que no siga siendo visible
    public override void Exit()
    {
        MapSelectionMenu.SetActive(false);
    }

    public override void FixedUpdate()
    {

    }

    public override void Update()
    {

    }
}

