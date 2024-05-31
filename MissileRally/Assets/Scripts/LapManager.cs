using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LapManager : MonoBehaviour
{

    //Este script se encarga de comprovar que se realiza una vuelta completa al circuito

    public BoxCollider startLine;   //Collider trigger en la linea de salida/meta
    public BoxCollider firstCheck;  //Collider trigger en aproximadamente un tercio del circuito
    public BoxCollider secondCheck; //Collider trigger en aproximadamente dos tercios del circuito

    // Start is called before the first frame update
    void Start()
    {
        //Se activa el segundo collider (1/3 del circuito)
        startLine.enabled = false; 
        firstCheck.enabled = true;
        secondCheck.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //Cuando el coche pase por alguno de los colliders, este se desactivará y se activará el siguiente
        //Solo el coche del Owner puede ejecutar esto, al comprobar que su PlayerInput este activo
        if (other.gameObject.GetComponentInParent<PlayerInput>() != null && other.gameObject.GetComponentInParent<PlayerInput>().enabled == true)
        {
            if (startLine.enabled && !GameManager.Instance.player.lineCrossed)
            {
                //Si es el collider de la linea de salida el que está activo, significa que se ha completado una vuelta
                startLine.enabled = false;
                firstCheck.enabled = true;
                GameManager.Instance.player.UpdateCurrentLapServerRpc();

            }

            else if (firstCheck.enabled && !GameManager.Instance.player.lineCrossed)
            {
                firstCheck.enabled = false;
                secondCheck.enabled = true;

            }

            else if (secondCheck.enabled && !GameManager.Instance.player.lineCrossed)
            {
                secondCheck.enabled = false;
                startLine.enabled = true;
            }
        }
    }
}
