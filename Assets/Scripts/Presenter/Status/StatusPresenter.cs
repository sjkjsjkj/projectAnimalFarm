using UnityEngine;

/// <summary>
/// 플레이어 상태 UI와 로직을 스스로 수집하여 중간 다리를 연결해주는 컴포넌트
/// </summary>
public class StatusPresenter : BaseMono
{
    [Header("인터페이스를 준수하는 각 컴포넌트 참조 연결")]
    [SerializeField] private BaseMono _statusUiMono;
    [SerializeField] private BaseMono _statusLogicalMono;

    private IStatusUI _statusUi;
    private IStatusLogical _statusLogical;

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 도구 슬롯을 클릭했을 때
    private void ToolSlotClickedHandle(EType toolType)
    {
        if(_statusUi == null || _statusLogical == null) return;
        // 보유한 도구 전달
        var tools = _statusLogical.GetTools(toolType);
        _statusUi.ToolSlotClickedHandle(out tools);
    }

    // 구체적인 도구를 선택했을 때
    private void ToolItemClickedHandle(string toolId)
    {
        if (_statusUi == null || _statusLogical == null) return;
        // 스왑 성공
        if (_statusLogical.TrySwapTool(toolId, out string message))
        {
            _statusUi.SwapToolSuccessHandle(message);
        }
        else // 스왑 실패
        {
            _statusUi.SwapToolFailureHandle(message);
        }
    }

    private bool TryGetStatusUi()
    {
        if (_statusUiMono == null)
        {
            UDebug.Print($"인스펙터에 상점 UI가 등록되지 않았습니다.", LogType.Assert);
            return false;
        }
        if (!_statusUiMono.TryGetComponent(out _statusUi))
        {
            UDebug.Print($"인스펙터에 등록된 오브젝트에 IStatusUI 인터페이스가 없습니다.", LogType.Assert);
            return false;
        }
        return true;
    }

    private bool TryGetStatusLogic()
    {
        if (_statusLogicalMono == null)
        {
            UDebug.Print($"인스펙터에 상점 로직이 등록되지 않았습니다.", LogType.Assert);
            return false;
        }
        if (!_statusLogicalMono.TryGetComponent(out _statusLogical))
        {
            UDebug.Print($"인스펙터에 등록된 오브젝트에 IStatusLogical 인터페이스가 없습니다.", LogType.Assert);
            return false;
        }
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnEnable()
    {
        if (TryGetStatusUi())
        {
            _statusUi.OnToolSlotClicked += ToolSlotClickedHandle;
            _statusUi.OnToolItemClicked += ToolItemClickedHandle;
        }
        TryGetStatusLogic();
    }

    private void OnDisable()
    {
        if (_statusUi != null)
        {
            _statusUi.OnToolSlotClicked -= ToolSlotClickedHandle;
            _statusUi.OnToolItemClicked -= ToolItemClickedHandle;
        }
    }

    // 인스펙터 편의성
    protected void Reset()
    {
        // 비활성화된 객체를 포함하여 탐색
        BaseMono[] monos = FindObjectsOfType<BaseMono>(true);
        // 인터페이스를 준수하는 컴포넌트 탐색
        int length = monos.Length;
        for (int i = 0; i < length; ++i)
        {
            BaseMono mono = monos[i];
            if (_statusUiMono == null && mono is IStatusUI)
            {
                UDebug.Print($"상태 UI 컴포넌트를 자동 탐색했습니다.");
                _statusUiMono = mono;
            }
            if (_statusLogicalMono == null && mono is IStatusLogical)
            {
                UDebug.Print($"상태 로직 컴포넌트를 자동 탐색했습니다.");
                _statusLogicalMono = mono;
            }
            if (_statusUiMono != null && _statusLogicalMono != null)
            {
                UDebug.Print($"필요한 상태 컴포넌트 탐색을 완료했습니다.");
                return;
            }
        }
    }
    #endregion
}
