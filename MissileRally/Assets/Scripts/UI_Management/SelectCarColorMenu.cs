using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SelectCarColorMenu : MonoBehaviour
{
    public event Action colorSelected; // Evento que se llamará en el player cuando el jugador pulse el botón con el texto "Vale"

    private void Start()
    {
    }

    public void ChangeCarColor(Image image)
    {
        //Se guarda el color de la imagen seleccionada en el data del player y se asigna dicho color a su material
        GameManager.Instance.player.data.colorRed = image.color.r;
        GameManager.Instance.player.data.colorGreen = image.color.g;
        GameManager.Instance.player.data.colorBlue = image.color.b;
        GameManager.Instance.player.data.colorAlpha = image.color.a;
        GameManager.Instance.player.car.transform.Find("body").gameObject.GetComponent<MeshRenderer>().materials[1].color = image.color;
    }

    public void ExitMenu()
    {
        colorSelected?.Invoke(); 
    }
}
