using Unity.Netcode;
using UnityEngine;

//Componente encargado de permitir que un jugador se pueda recuperar de un vuelco
public class RecoverComponent : NetworkBehaviour
{
    readonly int checkFrecuency = 2; //Frecuencia con la que comprueba si el coche se ha volcado en segundos
    float recoverLimit = 2;          //Tiempo de gracia para determinar si debemos restaurar la posicion del coche
    float currentTime = 0.0f;        //Contador del tiempo pasado desde el ultimo chequeo realizado
    float activeRecover = 0.0f;      //Temporizador del periodo de gracia
    bool recover = false;            //Variable que indica si el coche puede recuperarse o no
    public Transform recoverPosition;       //Referencia a la transformada de la esfera asociada al jugador
    public GameObject car;

    //En el Start, guardamos la referencia a la esfera blanca asociada al jugador
    private void Start()
    {
        if (IsServer)
        {
            car = gameObject.GetComponent<Player>().car;
        }
    }

    //En el Update, se realiza toda la l�gica que controla el sistema de recuperaci�n
    void Update()
    {
        if(IsServer)
        {
            recoverPosition = recoverPosition == null ? gameObject.GetComponent<Player>().spherePosition : recoverPosition;

            currentTime += Time.deltaTime;      //Aumentamos el tiempo pasado desde el �ltimo chequeo
            if (currentTime > checkFrecuency)   //Si el tiempo es mayor al indicado en checkFrecuency
            {
                //Calculamos el �ngulo entre eje Y del coche y el eje Y del mundo
                float result = Vector3.Angle(car.transform.up, Vector3.up);
                //Si el �ngulo es mayor o igual a 30�, permitimos que el coche pueda recuperarse, en caso contrario no
                recover = result >= 30 ? true : false;
                //Reiniciamos el contador para el siguiente chequeo
                currentTime = 0f;
            }

            //Para asegurarnos de que el jugador est� volcado, contaremos el tiempo que recover est� en true
            //Si el contador llega a su limite (recoverLimit), significa que el jugador sigue volcado.
            if (recover)
            {
                activeRecover += Time.deltaTime;
            }

            //Si el coche esta volcado y se ha llegado al tiempo limite
            if (recover && activeRecover >= recoverLimit)
            {
                //Giramos el coche haciendo coincidir los ejes Y local y del mundo, hacemos que mire hacia delante y le
                //asignamos a su transformada la posici�n del punto de recuperacion, el cual es su esfera asociada.
                //serverRecoverServerRpc();

                //Giramos el coche haciendo coincidir los ejes Y local y del mundo, hacemos que mire hacia delante y le
                //asignamos a su transformada la posici�n del punto de recuperacion, el cual es su esfera asociada.
                Vector3 forwardDirection = transform.forward + transform.position;
                car.transform.up = Vector3.up;
                car.transform.LookAt(forwardDirection);
                car.transform.Rotate(Vector3.left, -20);
                car.transform.position = recoverPosition.position;
                recover = false;
            }

            //Si recover est� en falso, significa que el jugador finalmente no est� volcado, por lo tanto, ponemos el contador a 0
            if (!recover) activeRecover = 0;
        }
        else
        {
            Debug.Log("No soy server");
        }
    }

    [ServerRpc]
    void serverRecoverServerRpc()
    {
        //Giramos el coche haciendo coincidir los ejes Y local y del mundo, hacemos que mire hacia delante y le
        //asignamos a su transformada la posici�n del punto de recuperacion, el cual es su esfera asociada.
        Vector3 forwardDirection = transform.forward + transform.position;
        transform.up = Vector3.up;
        transform.LookAt(forwardDirection);
        transform.Rotate(Vector3.left, -20);
        //transform.position = recoverPosition.position;
    }
}
