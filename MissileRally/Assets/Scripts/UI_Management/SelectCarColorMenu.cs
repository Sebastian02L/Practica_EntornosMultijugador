using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectCarColorMenu : MonoBehaviour
{
    UIManager manager;

    private void Start()
    {
        manager = GameObject.Find("@UIManager").GetComponent<UIManager>();
    }

    public void ChangeCarColor(Image image)
    {
        GameManager.Instance.player.car.transform.Find("body").gameObject.GetComponent<MeshRenderer>().materials[1].color = image.color;
    }

    public void ExitMenu()
    {
        gameObject.SetActive(false);
        manager.State = new GameInterfaceState(manager);
    }
}
