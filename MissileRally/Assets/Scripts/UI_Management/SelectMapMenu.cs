using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SelectMapMenu : MonoBehaviour
{
    UIManager manager;
    int mapSelection;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("@UIManager").GetComponent<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Método invocado cuando pulsas sobre la imagen del mapa.
    public void SelectMap(int map)
    {
        mapSelection = map;
    }

    public void ExitMenu()
    {
        GameManager.Instance.mapSelectedId = mapSelection;
        GameManager.Instance.player.mapSelectedId.Value = mapSelection;
        manager.State = new LobbyState(manager);
    }
}
