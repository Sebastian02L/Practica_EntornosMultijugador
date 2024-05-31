using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Interfaz que define el comportamiento o m�todos que todo estado del patr�n state debe realizar.
public interface IState
{
    public void Enter(); //Comportamiento al entrar al estado.
    public void Exit(); //Comportamiento al salir del estado.
    public void Update(); //Comportamiento del estado en la actualizaci�n de paso variable.
    public void FixedUpdate(); //Comportamiento del estado en la actualizaci�n de paso fijo.
}
