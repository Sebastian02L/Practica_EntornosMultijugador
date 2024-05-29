using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectMapMenu : MonoBehaviour
{
    UIManager manager;
    public event Action mapSelected;
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

    public void SelectMap(int map)
    {
        mapSelection = map;
    }

    public void ExitMenu()
    {
        GameManager.Instance.mapSelectedId = mapSelection;
        GameManager.Instance.mapSelected = true;
        mapSelected?.Invoke();
        manager.State = new LobbyState(manager);
    }
}
