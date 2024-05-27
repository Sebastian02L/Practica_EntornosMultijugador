using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
