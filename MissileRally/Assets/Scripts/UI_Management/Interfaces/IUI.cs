using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Interfaz que define los métodos que debe tener el contexto del patrón state.
//Como contexto que es del patrón state, se debe poder obtener y establecer el estado. También, como se trata de un state para gestionar la interfaz, se debe poder obtener y 
//establecer el canvas.
public interface IUI
{
    IState State { get; set; }
    GameObject Canvas { get; set; }
}
