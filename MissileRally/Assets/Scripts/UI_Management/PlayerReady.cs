using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReady : MonoBehaviour
{
    public event Action playerReady; // Evento que se activa cuando el jugador pulsa el botón con el texto "Ready"
    float clickCooldown = 1f; // Valor de cooldown para pulsar el botón "Ready"
    float timePassed = 0f; // Tiempo pasado desde la última pulsación del botón 
    bool clicked = false; // Indica si se ha pulsado el botón o no
    public Button readyButton; // Referencia al botón
    TextMeshProUGUI buttonText; // Texto del botón
    bool buttonAvailable = false; // Indica si el botón está disponible para pulsar

    private void Start()
    {
        buttonText = readyButton.GetComponentInChildren<TextMeshProUGUI>(); // Guardamos la referencia del botón
    }

    private void Update()
    {
        //Hacemos que el boton no sea interactuable hasta que el host no haya seleccionado un mapa, asi en caso damos tiempo
        //de que la variable de red se sincronice correctamente.
        if(GameManager.Instance.mapSelectedId != 0 && !buttonAvailable)
        {
            readyButton.interactable = true;
            buttonAvailable = true;
        }

        //Si se ha hecho click en el boton, debe esperar un tiempo para volver a pulsarlo
        if(clicked)
        {
            timePassed += Time.deltaTime;
        }

        //Si se ha cumplido el tiempo de espera, habilitamos el boton
        if( timePassed > clickCooldown )
        {
            clicked = false;
            timePassed = 0f;
            readyButton.interactable = true;
        }
    }
    public void OnplayerReady()
    {
        //Si se puede hacer click en el boton, lo desactivamos por un tiempo especifico, de manera que no se pueda hacer multiples pulsaciones
        if(!clicked)
        {
            playerReady?.Invoke();
            clicked = true;
            buttonText.text = (buttonText.text == "Ready") ? "Unready" : "Ready";
            readyButton.interactable = false;
        }

    }
}
