using UnityEngine;

/// <summary>
/// 사실상 플레이어다.
/// 플레이어는 앞에 있는 상호작용 가능한 것이 있다면 특정 키를 눌러 상호작용한다.
/// </summary>
public class ExInteractor : MonoBehaviour
{
    // 필요한 것은 '누른다'와 '탐지'
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("필수 요소 등록")]
    [SerializeField] private Transform _player;

    [Header("사용자 정의 설정")]
    [SerializeField] private Transform _origin;                 // 탐지 기준점
    [SerializeField] private float _radius = 3f;                // 상호작용 거리
    [SerializeField] private LayerMask _interactableLayer = 0;  // 상호작용 레이어 (0 = 전체)
    [SerializeField] private KeyCode _interactKey = KeyCode.E;
    [SerializeField] private bool _logEnable = true;
    [SerializeField] private bool _showGizmo = true;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private ExInteractableBase _current;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 플레이어 근처에 상호작용 가능한 대상
    private void FindInteractable()
    {
        // 플레이어 근처에 상호작용 가능한 대상이 무엇이 있나?
        // 여러개가 잡힘녀 가장 가까운 것 하나를 선택핢
        // 상호작용 가능한 것을 current로 저장
        int mask = _interactableLayer.value;
        if(mask == 0) {
            mask = Physics2D.AllLayers;
        }
        // 콜라이더 + 배열 = 성능 부담 있음, 상대에 콜라이더가 없으면 탐지가 안됨.
        // 오버랩 : 트리거 여부 상관없이 다 잡는다. 단 객체가 많아질수록 반드시 레이어로 분리해서 관리해야 부담이 적음
        //         또한 아무런 처리를 하지 않는 타일맵이랑 궁합이 좋지 않다.
        Collider2D[] hits = Physics2D.OverlapCircleAll(_origin.position, _radius, mask);
        // 원 탐지는 프로그래머 기준에서 봤을 때 죄악과 같은 행위임 사실.. 연산량이 너무 높음
        // 일반적으로 원 탐지는 퍼포먼스에서 가장 좋지 않은 탐색 방법이다.
        if(hits == null) {
            return;
        }
        int length = hits.Length;
        if(length <= 0) {
            return;
        }
        ExInteractableBase best = null;
        float bestDist = float.MaxValue;
        for (int i = 0; i < length; ++i) {
            // 왜 Children? → 연결 즉 상속을 전제하는 경우가 많기 때문에 확인한다.
            // 콜라이더는 보통 자식에 붙어있는 편이고, 스크립트는 부모에 붙는 경우가 많기 때문이다.
            // 그래서 충돌체를 가진 오브젝트의 부모까지 올라가서 Interactable을 찾는다.
            ExInteractableBase target = hits[i].GetComponentInParent<ExInteractableBase>();
            if(target == null) {
                continue;
            }
            // 쿨타임 & 1회성
            if (!target.IsAvailable()) {
                continue;
            }
            // 거리 계산
            // 기준점은 _origin(플레이어)와 target.GetHintAnchorPosition()
            // 오브젝트 피벗 등이 어긋날 수 있기 때문에 상호작용 기준점을 따로 둘 수 있게 세팅하겠다.
            // 분명 캐릭터랑 맞닿은 것 같은데 왜 충돌이 안되지? 이런 일을 방지한다.
            float distance = Vector2.Distance(_origin.position, target.GetHintAnchorPosition());
            if (distance < bestDist) {
                // 거리가 더 가까운 걸로 설정
                bestDist = distance;
                best = target;
            }
        }
        _current = best;
    }
    // 힌트 출력 (개발 용도)
    // 나중에 UI 붙을테니 그때가면 필요없어지니까 그때는 삭제
    private void PrintHintIfNeeded()
    {
        if (!_logEnable) {
            return;
        }
        if(_current == null) {
            UDebug.Print($"[E] 주변에 상호작용 대상 없음");
            return;
        }
        // 게임 상호작용은 대부분 이 형태 고정이다. (동사 + 이름)
        // 텍스트 자체가 Base에서 나오기 때문에 (공통) → 새로운 상호작용 오브젝트를 추가해도 힌트 UI에 대한 코드는 바꿀 필요가 없다.
        // 그러면 텍스트 형태도 정해져있는거네
        string name = _current.GetDisplayName();
        string verb = _current.GetVerbText();
        UDebug.Print($"[E] {name} {verb}");
    }

    // 실제 입력이 들어왔을 때 호출되는 함수
    private void TryInteract()
    {
        if(_current == null) {
            return;
        }
        // 상속 구조를 만들고 왔기 때문에 편해졌다.
        // 규칙 검사 (쿨타임 / 1회성 / 특정 아이템이 필요하냐?) + 실제 행동 (문 열기 / 줍기 / 올라선다)은 대상이 담당
        _current.TryInteract(this);
    }
    // 확장 요소 → 미래에 큰 그림을 그리는 나에게 주는 선물
    // 흠.. 이 클래스 플레이어나 마찬가지라며? 그럼 그냥 플레이어 좌표 아냐?
    // → 누가 눌렀는지 + 눌렀던 위치가 필요한 경우가 생길 수 있다. (사운드 + 이펙트 등)
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Update()
    {
        FindInteractable();
        PrintHintIfNeeded(); // 디버깅용 로그
        if (Input.GetKeyDown(_interactKey)) {
            TryInteract();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!_showGizmo) {
            return;
        }
        Transform o = (_origin != null) ? _origin : transform;
        // 알파값은 대부분 프레임 드랍의 주범이다. 특히 3D는 그렇다.
        Gizmos.color = new Color(0.2f, 1f, 0.8f, 0.35f);
        Gizmos.DrawSphere(o.position, _radius);
    }
    #endregion
}
