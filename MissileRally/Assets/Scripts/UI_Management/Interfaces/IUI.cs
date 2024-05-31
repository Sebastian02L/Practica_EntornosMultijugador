using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Interfaz que define los m�todos que debe tener el contexto del patr�n state.
//Como contexto que es del patr�n state, se debe poder obtener y establecer el estado. Tambi�n, como se trata de un state para gestionar la interfaz, se debe poder obtener y 
//establecer el canvas.
public interface IUI
{
    IState State { get; set; }
    GameObject Canvas { get; set; }
}
