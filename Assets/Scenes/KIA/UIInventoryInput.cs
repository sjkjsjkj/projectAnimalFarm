using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// New Input System의 Inventory 액션 입력을 받아
/// 인벤토리 UI 열기/닫기를 담당합니다.
/// 현재 프로젝트에서는 MainActionMap/Inventory 액션을 사용합니다.
/// </summary>
public class UIInventoryInput : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("대상 UI")]
    [SerializeField] private UIPlayerInventory _uiInventory;

    [Header("입력 액션")]
    [SerializeField] private InputActionReference _inventoryAction;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// Inventory 입력이 들어왔을 때 인벤토리 UI를 토글합니다.
    /// </summary>
    private void OnPerformedInventory(InputAction.CallbackContext context)
    {
        if (_uiInventory == null)
        {
            return;
        }

        _uiInventory.SetToggleUI();
    }

    /// <summary>
    /// Inventory 액션 이벤트를 연결하고 활성화합니다.
    /// </summary>
    private void BindInput()
    {
        if (_inventoryAction == null || _inventoryAction.action == null)
        {
            UDebug.Print("[UIInventoryInput] Inventory Action이 연결되지 않았습니다.", LogType.Warning);
            return;
        }

        _inventoryAction.action.performed -= OnPerformedInventory;
        _inventoryAction.action.performed += OnPerformedInventory;
        _inventoryAction.action.Enable();
    }

    /// <summary>
    /// Inventory 액션 이벤트를 해제하고 비활성화합니다.
    /// </summary>
    private void UnbindInput()
    {
        if (_inventoryAction == null || _inventoryAction.action == null)
        {
            return;
        }

        _inventoryAction.action.performed -= OnPerformedInventory;
        _inventoryAction.action.Disable();
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        BindInput();
    }

    private void OnDisable()
    {
        UnbindInput();
    }
    #endregion
}
