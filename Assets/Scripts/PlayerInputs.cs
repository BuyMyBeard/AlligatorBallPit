using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputs : MonoBehaviour
{
    [SerializeField] float jumpInputBuffer = 0.1f;
    [SerializeField] Menu menu;
    public Vector2 MoveInput { get; private set; } = Vector2.zero;
    public bool JumpPressInput { get; private set; } = false;
    public bool JumpHoldInput { get; private set; } = false;

    InputAction move, jump, pause;

    private void Awake()
    {
        Inputs inputMap = new Inputs();
        move = inputMap.FindAction("Move");
        jump = inputMap.FindAction("Jump");
        pause = inputMap.FindAction("Pause");
    }

    private void OnEnable()
    {
        move.Enable();
        move.performed += (InputAction.CallbackContext ctx) => MoveInput = ctx.ReadValue<Vector2>();
        move.canceled += _ => MoveInput = Vector2.zero;

        jump.Enable();
        jump.started += _ => StartCoroutine(BufferJump());
        jump.performed += _ => JumpHoldInput = true;
        jump.canceled += _ => JumpHoldInput = false;
        pause.Enable();
        pause.started += _ => menu.TogglePause();
    }
    IEnumerator BufferJump()
    {
        JumpPressInput = true;
        yield return new WaitForSeconds(jumpInputBuffer);
        JumpPressInput = false;
    }
}
