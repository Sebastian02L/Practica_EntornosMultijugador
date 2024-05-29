using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReady : MonoBehaviour
{
    UIManager manager;
    public event Action playerReady;

    private void Start()
    {
        manager = GameObject.Find("@UIManager").GetComponent<UIManager>();
    }
    public void OnplayerReady()
    {
        playerReady?.Invoke();
        //Ahora mismo transiciona a GameInterface, pero no deberia
        //manager.State = new GameInterfaceState(manager);
    }
}
