using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 인벤토리 변경을 감지하여
/// HUD 퀵슬롯 5칸에 각 도구 타입의 최고 등급 아이템을 표시하는 프리젠터
/// </summary>
public class HUDQuickSlotPresenter : BaseMono
{
    #region ─────────────────────────▶ 싱글 참조 ◀─────────────────────────
    public static HUDQuickSlotPresenter Ins { get; private set; }
    #endregion

    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("HUD 퀵슬롯 뷰")]
    [SerializeField] private HUDQuickSlotView _view;

    [Header("현재 선택 슬롯이 도구 퀵슬롯이면 장비도 다시 맞춰줄지")]
    [SerializeField] private bool _syncHeldItemWhenInventoryChanged = true;

    [Header("로그")]
    [SerializeField] private bool _logEnabled = false;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Inventory _playerInventory;
    private Coroutine _bindRoutine;
    private ToolQuickSlotUtility.QuickSlotToolData[] _cachedSlots;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool TryGetQuickSlotTool(int quickSlotIndex, out ToolQuickSlotUtility.QuickSlotToolData result)
    {
        result = ToolQuickSlotUtility.QuickSlotToolData.CreateEmpty(
            quickSlotIndex,
            ToolQuickSlotUtility.GetToolTypeByQuickSlotIndex(quickSlotIndex));

        if (_cachedSlots == null)
        {
            return false;
        }

        if (quickSlotIndex < 0 || quickSlotIndex >= _cachedSlots.Length)
        {
            return false;
        }

        result = _cachedSlots[quickSlotIndex];
        return result.HasItem;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private bool ValidateReferences()
    {
        if (_view == null)
        {
            UDebug.Print("HUDQuickSlotPresenter에 HUDQuickSlotView가 연결되지 않았습니다.", LogType.Assert);
            return false;
        }

        return true;
    }

    private IEnumerator CoWaitAndBindInventory()
    {
        while (enabled)
        {
            if (InventoryManager.Ins != null &&
                InventoryManager.Ins.IsSettingFinish &&
                InventoryManager.Ins.PlayerInventory != null)
            {
                _playerInventory = InventoryManager.Ins.PlayerInventory;
                _playerInventory.OnChangeSlot += HandleInventorySlotChanged;
                _playerInventory.OnChangeSlots += HandleInventorySlotsChanged;

                RefreshQuickSlots();

                if (_logEnabled)
                {
                    Debug.Log("[HUDQuickSlotPresenter] 플레이어 인벤토리 바인딩 완료");
                }

                yield break;
            }

            yield return null;
        }
    }

    private void RefreshQuickSlots()
    {
        if (_view == null)
        {
            return;
        }

        if (_playerInventory == null)
        {
            _cachedSlots = new ToolQuickSlotUtility.QuickSlotToolData[ToolQuickSlotUtility.QUICK_SLOT_COUNT];
            _view.ClearAll();
            return;
        }

        _cachedSlots = ToolQuickSlotUtility.BuildQuickSlotData(_playerInventory);
        _view.SetSlots(_cachedSlots);

        int currentSlot = -1;
        if (DataManager.Ins != null && DataManager.Ins.Player != null)
        {
            currentSlot = DataManager.Ins.Player.CurSlot;
        }

        if (ToolQuickSlotUtility.IsToolQuickSlotIndex(currentSlot))
        {
            _view.SetSelected(currentSlot);

            if (_syncHeldItemWhenInventoryChanged)
            {
                OnPlayerSelectSlot.Publish(currentSlot);
            }
        }
        else
        {
            _view.SetSelected(-1);
        }
    }

    private void HandleInventorySlotChanged(EInventoryType invenType, InventorySlot slot)
    {
        if (invenType != EInventoryType.PlayerInventory)
        {
            return;
        }

        RefreshQuickSlots();
    }

    private void HandleInventorySlotsChanged(EInventoryType invenType, Inventory inventory)
    {
        if (invenType != EInventoryType.PlayerInventory)
        {
            return;
        }

        RefreshQuickSlots();
    }

    private void HandlePlayerSlotChanged(OnPlayerSlotChanged channel)
    {
        if (_view == null)
        {
            return;
        }

        if (ToolQuickSlotUtility.IsToolQuickSlotIndex(channel.slotIndex))
        {
            _view.SetSelected(channel.slotIndex);
        }
        else
        {
            _view.SetSelected(-1);
        }
    }

    private void HandleQuickSlotClicked(int quickSlotIndex)
    {
        if (ToolQuickSlotUtility.IsToolQuickSlotIndex(quickSlotIndex) == false)
        {
            return;
        }

        OnPlayerSelectSlot.Publish(quickSlotIndex);
    }

    private void UnbindInventory()
    {
        if (_playerInventory == null)
        {
            return;
        }

        _playerInventory.OnChangeSlot -= HandleInventorySlotChanged;
        _playerInventory.OnChangeSlots -= HandleInventorySlotsChanged;
        _playerInventory = null;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        if (Ins == null)
        {
            Ins = this;
        }
        else if (Ins != this)
        {
            UDebug.Print("HUDQuickSlotPresenter가 중복으로 존재합니다.", LogType.Warning);
        }
    }

    private void OnEnable()
    {
        if (ValidateReferences() == false)
        {
            return;
        }

        _view.OnQuickSlotClicked += HandleQuickSlotClicked;
        EventBus<OnPlayerSlotChanged>.Subscribe(HandlePlayerSlotChanged);

        if (_bindRoutine != null)
        {
            StopCoroutine(_bindRoutine);
        }

        _bindRoutine = StartCoroutine(CoWaitAndBindInventory());
    }

    private void OnDisable()
    {
        _view.OnQuickSlotClicked -= HandleQuickSlotClicked;
        EventBus<OnPlayerSlotChanged>.Unsubscribe(HandlePlayerSlotChanged);

        if (_bindRoutine != null)
        {
            StopCoroutine(_bindRoutine);
            _bindRoutine = null;
        }

        UnbindInventory();
    }

    private void Reset()
    {
        if (_view == null)
        {
            _view = GetComponent<HUDQuickSlotView>();
        }
    }
    #endregion
}
