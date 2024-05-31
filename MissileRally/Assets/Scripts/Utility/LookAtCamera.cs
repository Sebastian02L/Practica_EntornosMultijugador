using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//COMPONENTE QUE SIRVE PARA HACER QUE UN OBJETO MIRE A LA CÁMARA (USADO PARA EL CANVAS DEL NOMBRE DEL JUGADOR)
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
        //Solo hay una cámara en cada iteración del juego, y la tiene el jugador dueño de esa iteración
        transform.LookAt(camara.transform.position);
    }
}
