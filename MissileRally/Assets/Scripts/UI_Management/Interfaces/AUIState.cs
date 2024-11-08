using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase abstracta que define la funcionalidad com�n de todos los estados. En este caso tener una referencia al UIManager, nuestro contexto del patr�n state.
public abstract class AUIState : IState
{
    protected IUI UI;

    public AUIState(IUI UI)
    {
        this.UI = UI;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void FixedUpdate();
    public abstract void Update();
}
