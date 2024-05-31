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
    public GameObject car;
    public bool outOfCircuit = false;

    //En el Start, guardamos la referencia a la esfera blanca asociada al jugador
    private void Start()
    {
        if (IsServer)
        {
            car = gameObject.GetComponent<Player>().car;
        }
    }

    //En el Update, se realiza toda la lógica que controla el sistema de recuperación
    void Update()
    {
        if(IsServer)
        {
            //Si ha colisionado con los colliders que estan fuera de los mapas, debe recuperarlo inmediatamente.
            if(outOfCircuit)
            {
                RecoverCar();
                outOfCircuit = false;
            }

            currentTime += Time.deltaTime;      //Aumentamos el tiempo pasado desde el último chequeo
            if (currentTime > checkFrecuency)   //Si el tiempo es mayor al indicado en checkFrecuency
            {
                //Calculamos el ángulo entre eje Y del coche y el eje Y del mundo
                float result = Vector3.Angle(car.transform.up, Vector3.up);
                //Si el ángulo es mayor o igual a 30º, permitimos que el coche pueda recuperarse, en caso contrario no
                recover = result >= 30 ? true : false;
                //Reiniciamos el contador para el siguiente chequeo
                currentTime = 0f;
            }

            //Para asegurarnos de que el jugador está volcado, contaremos el tiempo que recover está en true
            //Si el contador llega a su limite (recoverLimit), significa que el jugador sigue volcado.
            if (recover)
            {
                activeRecover += Time.deltaTime;
            }

            //Si el coche esta volcado y se ha llegado al tiempo limite
            if (recover && activeRecover >= recoverLimit)
            {
                //Giramos el coche haciendo coincidir los ejes Y local y del mundo, hacemos que mire hacia delante y le
                //asignamos a su transformada la posición del punto de recuperacion, el cual es su esfera asociada.
                RecoverCar();
            }

            //Si recover está en falso, significa que el jugador finalmente no está volcado, por lo tanto, ponemos el contador a 0
            if (!recover) activeRecover = 0;
        }
    }

    void RecoverCar()
    {
        car.GetComponent<Rigidbody>().velocity = new Vector3(0f,0f,0f);
        Vector3 forwardDirection = transform.forward + transform.position;
        car.transform.up = Vector3.up;
        car.transform.LookAt(forwardDirection);
        car.transform.Rotate(Vector3.left, -20);
        if (GameManager.Instance.gameStarted)
        {
            CircuitController controller = GameManager.Instance.circuitManager.GetComponent<CircuitController>();
            controller.ComputeClosestPointArcLength(car.transform.position, out _, out var carProj, out _);
            car.transform.position = carProj;
        }
        recover = false;
    }
}
