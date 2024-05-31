using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//COMPONENTE QUE SIRVE PARA HACER QUE UN OBJETO MIRE A LA C�MARA (USADO PARA EL CANVAS DEL NOMBRE DEL JUGADOR)
public class LookAtCamera : MonoBehaviour
{
    Camera camara;
    // Start is called before the first frame update
    void Start()
    {
        camara = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //Solo hay una c�mara en cada iteraci�n del juego, y la tiene el jugador due�o de esa iteraci�n
        transform.LookAt(camara.transform.position);
    }
}
