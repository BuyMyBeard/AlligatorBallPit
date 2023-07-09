using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputs : MonoBehaviour {

    public static PlayerInputs Instance;
    public Inputs inputMap;

    [SerializeField] float jumpInputBuffer = 0.1f;
    [SerializeField] Menu menu;
    public Vector2 MoveInput { get; private set; } = Vector2.zero;
    public bool JumpPressInput { get; private set; } = false;
    public bool JumpHoldInput { get; private set; } = false;
    public bool NudgeLeft { get; private set; } = false;
    public bool NudgeRight { get; private set; } = false;

    InputAction move, jump, pause, nudgeLeft, nudgeRight;
    void Awake() {

        if (Instance != null) {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        GameObject.DontDestroyOnLoad(this.gameObject);

        {

            inputMap = new Inputs();

            move = inputMap.FindAction("Move");
            jump = inputMap.FindAction("Jump");
            pause = inputMap.FindAction("Pause");
            nudgeLeft = inputMap.FindAction("Nudge Left");
            nudgeRight = inputMap.FindAction("Nudge Right");

        }
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
        pause.started += _ => GameObject.FindGameObjectWithTag("Pause Menu").GetComponentInChildren<Menu>().TogglePause();
        nudgeLeft.Enable();
        nudgeRight.Enable();
        nudgeLeft.started += _ => StartCoroutine(TapLeft());
        nudgeRight.started += _ => StartCoroutine(TapRight());
    }
    IEnumerator BufferJump()
    {
        JumpPressInput = true;
        yield return new WaitForSeconds(jumpInputBuffer);
        JumpPressInput = false;
    }
    IEnumerator TapLeft()
    {
        NudgeLeft = true;
        yield return new WaitForEndOfFrame();
        NudgeLeft = false;
    }
    IEnumerator TapRight()
    {
        NudgeRight = true;
        yield return new WaitForEndOfFrame();
        NudgeRight = false;
    }
}
