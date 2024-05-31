using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Componente que tiene los cubos ocultos de los escenarios. Si el jugador colisiona con ellos, marca dentro de su
//RecoverComponent una variable como true, indicando que debe recuperar al coche.
public class OutOfCircuitComponent : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.gameObject.GetComponentInParent<Player>();
        
        if(player != null)
        {
            player.GetComponentInParent<RecoverComponent>().outOfCircuit = true;
        }
    }
}
