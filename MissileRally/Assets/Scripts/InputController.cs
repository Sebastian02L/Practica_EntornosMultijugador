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

    ////////////////////////////////////////////// METODOS RPC EJECUTADOS POR EL SERVIDOR //////////////////////////////////////////////
    /// Estos metodos son invocados por los clientes pasando la informacion como parametros. Luego el servidor ejecuta dichos metodos.
    /// Netcode se encarga de actualizar los valores para cada clon del jugador.
    [ServerRpc] 
    void OnMoveServerRpc(Vector2 input)
    {
        car.InputAcceleration = input.y;
        car.InputSteering = input.x;
    }

    [ServerRpc]
    void OnBrakeServerRpc(float input)
    {
        car.InputBrake = input;
    }

    [ServerRpc]
    void OnAttackServerRpc(float input)
    {
    }

    ////////////////////////////////////////////// METODOS EJECUTADOS POR LOS CLIENTES //////////////////////////////////////////////
    public void OnMove(InputAction.CallbackContext context)
    {
        OnMoveServerRpc(context.ReadValue<Vector2>());
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        OnBrakeServerRpc(context.ReadValue<float>());
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
    }
}