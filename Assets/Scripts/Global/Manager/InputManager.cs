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
    private void Update() // 추가
    {
        // 매 프레임 현재 입력값 발행
        OnPlayerMove.Publish(_moveInput);
    }

    /// <summary>
    /// 매 프레임 현재 이동 입력값을 발행합니다.
    /// Input System 콜백은 값 저장에만 사용하며 실제 발행은 Update에서 처리합니다.
    /// </summary>
    public void OnMove(InputAction.CallbackContext context) // 수정
    {
        if (context.performed || context.canceled)
        {
            _moveInput = context.ReadValue<Vector2>(); 
        }
    }
    
    //public void OnMove(InputAction.CallbackContext context)
    //{
    //    OnPlayerMove.Publish(context.ReadValue<Vector2>());
    //}
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
