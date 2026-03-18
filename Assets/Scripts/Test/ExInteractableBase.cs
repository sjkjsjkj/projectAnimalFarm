using UnityEngine;

/// <summary>
/// 모든 상호작용 대상의 공통 규칙을 가지는 클래스
/// </summary>
public abstract class ExInteractableBase : MonoBehaviour
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("텍스트")]
    [SerializeField] private string _displayName = "Interactable"; // 필요한건가?
    [SerializeField] private string _verbText = "사용"; // 상호작용 시 어떤 문장?
    [Header("규칙")]
    [SerializeField] private bool _useOnce = false; // 한 번만 사용 가능하게?
    [SerializeField] private float _cooldown = 0f; // 상호작용 쿨타임은?
    [Header("힌트")]
    [SerializeField] private Transform _hintAnchor; // 힌트 기준점
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _usedOnce = false;
    private float _nextTime = 0f;
    #endregion

    #region ─────────────────────────▶ 메서드 ◀─────────────────────────
    public bool IsAvailable()
    {
        if(_usedOnce) {
            return false;
        }
        if(Time.time < _nextTime) {
            return false;
        }
        return true;
    }

    // Interactor쪽에서 힌트나 UI 등을 참고할 때 사용할 수 있다.
    // ex) 문, 레버, 아이템 A, 아이템 B 등
    // 이런 단순한 게 상호작용 한해서 굉장히 중요한 함수다.
    // 차후에 래핑 : 추가적으로 보강하거나 새롭게 함수를 만들어서 확장성을 준다고 기억하자.
    public string GetDisplayName() => _displayName;
    public string GetVerbText() => _verbText;
    // 월드 좌표를 반환, 우리가 원하는 위치에 안 그려질 수 있기 때문에.
    public Vector3 GetHintAnchorPosition()
    {
        if(_hintAnchor != null) {
            return _hintAnchor.position;
        } else {
            return transform.position;
        }
    }

    // 자식 전용 → 오버라이드를 전제해서 설계한다.
    // ex) 잠긴 문을 열었는데 인벤토리에 키가 없다.
    protected virtual bool CanInteract(ExInteractor interactor, out string failReason)
    {
        failReason = "";
        // 방어 코드
        return interactor != null;
    }

    // 상호작용 메서드를 반드시 구현하라
    // 이건 추상적인 느낌보다 실제 행동에 대한 상호작용 결과
    protected abstract void OnInteract(ExInteractor interactor);

    // 상호작용이 성공했을 때 적용되는 공통 규칙을 갱신하면 좋다.
    private void ApplyCommonAfterInteract()
    {
        // 쿨타임 적용
        if (_cooldown > 0f) {
            _nextTime = Time.time + _cooldown;
        }
        // 1회성 처리
        if (_useOnce) {
            _usedOnce = true;
        }
    }

    // 상호작용의 단일 진입점 (제일 중요한 함수)
    // 규칙 검사 + 실제 행동을 담당한다.
    public void TryInteract(ExInteractor interactor)
    {
        // ExInteractor는 누가 상호작용했는지 정보를 넘겨준다.
        // Base → 공통 규칙만 검사
        // 자식 → 자기만의 조건과 행동을 제공
        // 공통 규칙은 Base / 개별 규칙 + 행동은 자식에서 수행한다.
        if (!IsAvailable()) {
            UDebug.Print($"[{_displayName}] 현재 사용할 수 없습니다. (일회성 또는 쿨타임)");
            return;
        }
        // 자식 전용 조건 검사
        if(!CanInteract(interactor, out string reason)) {
            UDebug.Print($"[{_displayName}] 현재 사용할 수 없습니다. (자식 조건)");
            return;
        }
        // 공통 규칙, 조건이 통과된 상태
        OnInteract(interactor);
        // 공통 후처리 진행
        ApplyCommonAfterInteract();
    }
    #endregion
}
