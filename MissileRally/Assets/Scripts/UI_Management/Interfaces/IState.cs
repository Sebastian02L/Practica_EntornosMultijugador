using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Interfaz que define el comportamiento o métodos que todo estado del patrón state debe realizar.
public interface IState
{
    public void Enter(); //Comportamiento al entrar al estado.
    public void Exit(); //Comportamiento al salir del estado.
    public void Update(); //Comportamiento del estado en la actualización de paso variable.
    public void FixedUpdate(); //Comportamiento del estado en la actualización de paso fijo.
}
