using UnityEngine;

/// <summary>
/// 상점 UI와 로직을 스스로 수집하여 중간 다리를 연결해주는 컴포넌트
/// </summary>
public class CraftPresenter : BaseMono
{
    [Header("인터페이스를 준수하는 각 컴포넌트 참조 연결")]
    [SerializeField] private BaseMono _craftUiMono;
    [SerializeField] private BaseMono _craftLogicalMono;

    private ICraftUI _craftUi;
    private ICraftLogical _craftLogical;

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 아이템 제작을 시도했을 때
    private void CraftButtonPressedHandle(string recipeId)
    {
        if(_craftUi == null || _craftLogical == null) return;
        // 제작 성공
        if (_craftLogical.TryCraftItem(recipeId, out string message))
        {
            _craftUi.CraftSuccessHandle(message);
        }
        // 제작 실패
        else
        {
            _craftUi.CraftFailureHandle(message);
        }
    }

    // 카테고리에서 제작할 아이템을 클릭했을 때
    private void ItemClickedHandle(string recipeId)
    {
        if (_craftUi == null || _craftLogical == null) return;
        // 구조체 전달
        var materials = _craftLogical.GetMaterials(recipeId);
        _craftUi.ReceiveMaterialsHandle(out materials);
    }

    private bool TryGetCraftUi()
    {
        if (_craftUiMono == null)
        {
            UDebug.Print($"인스펙터에 상점 UI가 등록되지 않았습니다.", LogType.Assert);
            return false;
        }
        if (!_craftUiMono.TryGetComponent(out _craftUi))
        {
            UDebug.Print($"인스펙터에 등록된 오브젝트에 ICraftUI 인터페이스가 없습니다.", LogType.Assert);
            return false;
        }
        return true;
    }

    private bool TryGetCraftLogic()
    {
        if (_craftLogicalMono == null)
        {
            UDebug.Print($"인스펙터에 상점 로직이 등록되지 않았습니다.", LogType.Assert);
            return false;
        }
        if (!_craftLogicalMono.TryGetComponent(out _craftLogical))
        {
            UDebug.Print($"인스펙터에 등록된 오브젝트에 ICraftLogical 인터페이스가 없습니다.", LogType.Assert);
            return false;
        }
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void OnEnable()
    {
        if (TryGetCraftUi())
        {
            _craftUi.OnCraftButtonPressed += CraftButtonPressedHandle;
            _craftUi.OnItemClicked += ItemClickedHandle;
        }
        TryGetCraftLogic();
    }

    private void OnDisable()
    {
        if (_craftUi != null)
        {
            _craftUi.OnCraftButtonPressed -= CraftButtonPressedHandle;
            _craftUi.OnItemClicked -= ItemClickedHandle;
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
            if (_craftUiMono == null && mono is ICraftUI)
            {
                UDebug.Print($"제작 UI 컴포넌트를 자동 탐색했습니다.");
                _craftUiMono = mono;
            }
            if (_craftLogicalMono == null && mono is ICraftLogical)
            {
                UDebug.Print($"제작 로직 컴포넌트를 자동 탐색했습니다.");
                _craftLogicalMono = mono;
            }
            if (_craftUiMono != null && _craftLogicalMono != null)
            {
                UDebug.Print($"필요한 제작 컴포넌트 탐색을 완료했습니다.");
                return;
            }
        }
    }
    #endregion
}
