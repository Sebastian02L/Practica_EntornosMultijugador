using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReady : MonoBehaviour
{
    UIManager manager;
    public event Action playerReady;
    float clickCooldown = 1f;
    float timePassed = 0f;
    bool clicked = false;
    public Button readyButton;
    TextMeshProUGUI buttonText;

    private void Start()
    {
        manager = GameObject.Find("@UIManager").GetComponent<UIManager>();
        buttonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
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
