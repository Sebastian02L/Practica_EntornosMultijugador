using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SelectMapMenu : MonoBehaviour
{
    UIManager manager;
    int mapSelection = 1;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("@UIManager").GetComponent<UIManager>();
    }

    //Método invocado cuando pulsas sobre la imagen del mapa.
    public void SelectMap(int map) 
    {
        mapSelection = map;
    }

    //Método invocado cuando pulsas el botón de "Vale".
    public void ExitMenu()
    {
        // Se les da el valor adecuado a las variables del GameManager y se cambia de estado la interfaz
        GameManager.Instance.mapSelectedId = mapSelection;
        GameManager.Instance.player.mapSelectedId.Value = mapSelection;
        manager.State = new LobbyState(manager);
    }
}
