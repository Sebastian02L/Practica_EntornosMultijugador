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
    GameObject MapSelectionMenu;

    public MapSelectionState(IUI UI) : base(UI)
    {
    }

    public override void Enter()
    {
        MapSelectionMenu = UI.Canvas.transform.Find("SelectMapMenu").gameObject;
        MapSelectionMenu.SetActive(true);
    }

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

