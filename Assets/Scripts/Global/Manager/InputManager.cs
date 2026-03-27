using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input System으로 키 입력을 받아 이벤트를 뿌립니다.
/// </summary>
public class InputManager : GlobalSingleton<InputManager>, InputDispatcher.IMainActionMapActions
{
    private bool _isInitialized = false;
    private InputDispatcher _input;
    private Vector2 _moveInput; // 추가

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 외부 호출 용도가 아닙니다.
    private void Update()
    {
        OnPlayerMove.Publish(_moveInput);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPlayerJump.Publish();
        }
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPlayerInteract.Publish();
        }
    }
    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPlayerInventory.Publish();
        }
    }

    // 추가
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            OnPlayerRun.Publish(true);
        }
        else if (context.canceled)
        {
            OnPlayerRun.Publish(false);
        }
    }
    // Awake 대용
    public override void Initialize()
    {
        if (_isInitialized) return;

        _input = new InputDispatcher();
        _input.MainActionMap.SetCallbacks(this);
        _input.Enable();
        _isInitialized = true;
    }
    #endregion

    #region ─────────────────────────▶ 메세지 함수 ◀─────────────────────────
    // 외부에서 호출해도 OK
    public void OnEnable()
    {
        if (_input == null)
        {
            return;
        }

        _input.Enable();
    }
    public void OnDisable()
    {
        if (_input == null)
        {
            return;
        }

        _input.Disable();
    }
    #endregion
}
