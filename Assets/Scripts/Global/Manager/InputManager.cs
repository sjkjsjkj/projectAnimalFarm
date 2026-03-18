using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input System으로 키 입력을 받아 이벤트를 뿌립니다.
/// </summary>
public class InputManager : GlobalSingleton<InputManager>, InputDispatcher.IMainActionMapActions
{
    private bool _isInitialized = false;
    private InputDispatcher _input;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 외부 호출 용도가 아닙니다.
    public void OnMove(InputAction.CallbackContext context)
    {
        OnPlayerMove.Publish(context.ReadValue<Vector2>());
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
    // Awake 대용
    public override void Initialize()
    {
        if(_isInitialized)
        {
            return;
        }
        // 생성 및 초기화
        _input = new InputDispatcher();
        _input.MainActionMap.SetCallbacks(this);
        _isInitialized = true;
    }
    #endregion

    #region ─────────────────────────▶ 메세지 함수 ◀─────────────────────────
    // 외부에서 호출해도 OK
    public void OnEnable()
    {
        _input.Enable();
    }
    public void OnDisable()
    {
        _input.Enable();
    }
    #endregion
}
