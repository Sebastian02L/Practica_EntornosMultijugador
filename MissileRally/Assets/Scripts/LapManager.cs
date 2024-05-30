using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LapManager : MonoBehaviour
{
    public BoxCollider startLine;
    public BoxCollider firstCheck;
    public BoxCollider secondCheck;

    // Start is called before the first frame update
    void Start()
    {
        firstCheck.enabled = false;
        secondCheck.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponentInParent<PlayerInput>() != null && other.gameObject.GetComponentInParent<PlayerInput>().enabled == true)
        {
            if (startLine.enabled && !GameManager.Instance.player.lineCrossed)
            {
                startLine.enabled = false;
                firstCheck.enabled = true;
                GameManager.Instance.player.UpdateCurrentLapServerRpc();

            }

            else if (firstCheck.enabled && !GameManager.Instance.player.lineCrossed)
            {
                firstCheck.enabled = false;
                secondCheck.enabled = true;

            }

            else if (secondCheck.enabled && !GameManager.Instance.player.lineCrossed)
            {
                secondCheck.enabled = false;
                startLine.enabled = true;
            }
        }
    }
}
