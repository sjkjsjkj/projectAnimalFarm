using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 플레이어가 상호작용할 대상을 찾는 컴포넌트
/// </summary>
public class PlayerInteractor : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("상호작용 설정")]
    [SerializeField] private float _interactRadius = 1f;
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private float _autoInteractInterval = 0.1f;
    [SerializeField] private LayerMask _autoInteractableLayer;

    [Header("내부 변수")]
    [SerializeField] private int _overlapBufferSize = 15 * 15;
    [SerializeField] private bool _log = false;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private IInteractable _interactTarget; // 매 프레임 갱신
    private List<IAutoInteractable> _autoInteractableList = new(); // 매 프레임 갱신
    private Collider2D[] _overlapBuffer;
    private float _nextAutoInteractTime;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Lv1;

    // 외부 접근용 프로퍼티
    public IInteractable InteractTarget => _interactTarget;
    public IReadOnlyList<IAutoInteractable> AutoInteractables => _autoInteractableList;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        SearchInteractableObjects();
        if (UMath.TryCooldownEnd(Time.time, ref _nextAutoInteractTime, _autoInteractInterval))
        {
            AutoInteractObject();
        }
    }

    // 테스트 용도 메시지 함수
    [ContextMenu("내부 변수 출력")]
    public void PrintVariable()
    {
        StringBuilder sb = new();
        if (_interactTarget != null)
        {
            sb.AppendLine($"수동 상호작용 대상 : {_interactTarget}");
        }
        if (_autoInteractableList != null)
        {
            int length = _autoInteractableList.Count;
            for (int i = 0; i < length; ++i)
            {
                var val = _autoInteractableList[i];
                if (val == null) continue;
                sb.AppendLine($"{i}번째 자동 상호작용 대상 : {val}");
            }
        }
        UDebug.Print(sb.ToString());
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 범위 내의 단일 대상 하나에게 자동으로 상호작용한다.
    private void AutoInteractObject()
    {
        int length = _autoInteractableList.Count;
        GameObject go = gameObject;
        for (int i = 0; i < length; ++i)
        {
            var val = _autoInteractableList[i];
            if (val.CanInteract(go))
            {
                val.Interact(go);
                return;
            }
        }
    }

    // 상호작용할 대상을 모두 수집한다.
    private void SearchInteractableObjects()
    {
        // 버퍼에 수집한 오브젝트 대입
        int objectCount = Physics2D.OverlapCircleNonAlloc
            (transform.position, _interactRadius, _overlapBuffer, _interactableLayer.value | _autoInteractableLayer.value);
        // 자동 상호작용 대상 검출 준비
        _autoInteractableList.Clear();
        // 수동 상호작용 대상 검출 준비
        IInteractable interactTarget = null;
        float minSqrDistance = float.MaxValue;
        Vector2 playerPos = transform.position;
        // 오브젝트 순회
        for (int i = 0; i < objectCount; ++i)
        {
            Collider2D col = _overlapBuffer[i];
            // 자동 상호작용 대상 검출 시도
            if (col.TryGetComponent(out IAutoInteractable autoInteractableComp))
            {
                _autoInteractableList.Add(autoInteractableComp);
            }
            // 수동 상호작용 대상 검출 시도
            if (col.TryGetComponent(out IInteractable interactTargetComp))
            {
                Vector2 targetPos = col.transform.position;
                float sqrDist = (targetPos - playerPos).sqrMagnitude;
                // 새로운 가까운 수동 상호작용 대상 갱신
                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    interactTarget = interactTargetComp;
                }
            }
        }
        _interactTarget = interactTarget;
    }

    // 선택된 상호작용 대상과 상호작용 시도
    private void InteractHandle(OnPlayerInteract ctx)
    {
        if(_interactTarget == null)
        {
            return;
        }
        // 대상이 존재하므로 실행
        GameObject my = this.gameObject;
        if (_interactTarget.CanInteract(my))
        {
            _interactTarget.Interact(my);
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        _overlapBuffer = new Collider2D[_overlapBufferSize];
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus<OnPlayerInteract>.Subscribe(InteractHandle);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus<OnPlayerInteract>.Unsubscribe(InteractHandle);
    }
    #endregion
}
