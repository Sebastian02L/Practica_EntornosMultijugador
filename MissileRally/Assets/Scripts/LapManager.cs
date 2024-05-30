using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LapManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponentInParent<PlayerInput>().enabled == true)
        {
            GameManager.Instance.player.UpdateCurrentLapServerRpc();
        }
    }
}
