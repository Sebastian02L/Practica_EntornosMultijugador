using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SelectCarColorMenu : MonoBehaviour
{
    UIManager manager;
    public event Action colorChanged;

    private void Start()
    {
        manager = GameObject.Find("@UIManager").GetComponent<UIManager>();
    }

    public void ChangeCarColor(Image image)
    {
        GameManager.Instance.player.data.colorRed = image.color.r;
        GameManager.Instance.player.data.colorGreen = image.color.g;
        GameManager.Instance.player.data.colorBlue = image.color.b;
        GameManager.Instance.player.data.colorAlpha = image.color.a;
        GameManager.Instance.player.car.transform.Find("body").gameObject.GetComponent<MeshRenderer>().materials[1].color = image.color;
    }

    public void ExitMenu()
    {
        colorChanged?.Invoke();
        manager.State = new GameInterfaceState(manager);
    }
}
