using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : NetworkBehaviour
{
    private CarController car;

    private void Start()
    {
        car = GetComponent<Player>().car.GetComponent<CarController>();
    }

    [ServerRpc]
    void OnMoveServerRpc(Vector2 input)
    {
        car.InputAcceleration = input.y;
        car.InputSteering = input.x;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        OnMoveServerRpc(context.ReadValue<Vector2>());
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<float>();
        car.InputBrake = input;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
    }
}