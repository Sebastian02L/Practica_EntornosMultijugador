using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartingState : AUIState
{
    public StartingState(IUI UI) : base(UI)
    {
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void FixedUpdate()
    {
    }

    // Si el jugador ha elegido ser cliente o host y además se ha instanciado su objeto player, se cambia al estado de seleccionar un color
    public override void Update()
    {
        if ((NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer) && GameManager.Instance.player != null)
        {
            UI.State = new ColorSelectionState(UI);
        }
    }
}
