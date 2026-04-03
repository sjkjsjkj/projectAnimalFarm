using UnityEngine;

/// <summary>
/// NPC의 구조 전체를 담당하는 스크립트입니다.
/// </summary>
public class NPCObject : InfoObject
{
    #region ─────────────────────────▶ 인스 펙터 ◀─────────────────────────
    [Header("MonoBehaviour")]
    [SerializeField] private SpriteRenderer _spRenderer;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private NPCData _data;
    private ENpcState _state;
    private ENpcMoveType _moveType;

    private float _tickTimer = 0;        // Data는 일반 클래스이기 때문에 Update를 사용할 수 없음. 
    private float _tickInterval = 10.0f; // 그렇기 때문에 여기서 Update를 대신 돌아 줌. 

    private float _actionTimer = 0;       // Idle <> Move 상태를 자연스럽게 변경해줄 때 사용할 타이머
    private float _actionInterval = 3.0f; // 3초마다 한번씩 Move/Idle일 경우 랜덤하게 Move/Idle로 행동을 변경할 예정.

    private Animator _animator;
    private NpcMoveTypeBase _moveMaster;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public NPCData Data => _data;
    public SpriteRenderer SpRenderer => _spRenderer;
    public Animator Animator => _animator;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void SetState(ENpcState nextState)
    {
        ENpcState prevState = _state;
        _state = nextState;
        switch (_state)
        {
            case ENpcState.Idle:
                _animator.SetBool("Move", false);
                break;
            case ENpcState.Move:
                _animator.SetBool("Move", true);
                break;
            case ENpcState.Interaction:
                break;
        }
    }

    //껍데기 밖에 없던 animalObject에 스프라이트와 데이터, 애니메이터 컨트롤러를 넣어주는 작업.
    public override void SetInfo(UnitSO dataSO)
    {
        _spRenderer.sprite = dataSO.Image;
        if (!(dataSO as NpcWorldSO))
        {
            UDebug.Print("잘못된 데이터가 들어오고 있음. 이 부분에서는 AnimalWorldSO가 들어와야함.", LogType.Warning);
            return;
        }
        NpcWorldSO tempSO = (NpcWorldSO)dataSO;

        _data = new NPCData(tempSO);

        switch(tempSO.NpcMoveType)
        {
            case ENpcMoveType.DontMove:
                gameObject.AddComponent<NpcMoveTypeDontMove>();
                break;
            case ENpcMoveType.AreaMove:
                gameObject.AddComponent<NpcMoveTypeAreaMove>();
                break;
            case ENpcMoveType.PatrolMove:
                gameObject.AddComponent<NpcMoveTypePatrolMove>();
                break;
        }

        _moveMaster = GetComponent<NpcMoveTypeBase>();
        //TODO: 이벤트 연결

        _animator.runtimeAnimatorController = tempSO.AnimController;
    }
   
    private void UpdateInteraction()
    {
        
    }
    private void UpdateMove()
    {
        _moveMaster.Move();
    }
    private void UpdateIdle()
    {

    }
    
    //Idle <> Move 상태를 자연스럽게 변경해주기 위해 다음 액션을 랜덤하게 선택
    private void RandomAction()
    {
        if (!(_state == ENpcState.Idle || _state == ENpcState.Move))
        {
            return;
        }

        _actionInterval = Random.Range(2.0f, 4.0f);
        _actionTimer = 0;

        //랜덤 행동
        //현재 Idle 애니메이션이 없는 동물이 많아서 이동의 비중을 높힘.
        float randomValue = Random.Range(0.0f, 1.0f);
        //UDebug.Print($"RandomAction : {randomValue}");
        if (randomValue >= 0.3f)
        {
            SetState(ENpcState.Move);
        }
        else
        {
            SetState(ENpcState.Idle);
        }
    }
    //내가 바라보는 방향이 움직일 수 있는 곳인지 체크
    private void MoveDirTileCheck()
    {
        //내 타일 위치 체크
        //이동방향 (_moveDir) 에 있는 타일의 State 체크
        //_moveDir  ETileState.Moveable 과 비트비교. 
        //1이라면? true라면 냅둠.
        //거짓이라면 즉시 RandomAction();
        //예외처리 해야함. 먹이를 먹으러 가는데 가는 곳이 이동불가다? > RandomAction()으로 초기화도 안됨.
    }
    public override EPriority Priority => EPriority.Last;
    public override void ExecuteFrame()
    {
        _tickTimer += Time.deltaTime;
        _actionTimer += Time.deltaTime;

        if (_tickTimer >= _tickInterval)
        {
            _tickTimer = 0;
            _data.Tick();
        }

        if (_actionTimer >= _actionInterval)
        {
            RandomAction();
        }

        switch (_state)
        {
            case ENpcState.Interaction:
                UpdateInteraction();
                break;
            case ENpcState.Move:
                UpdateMove();
                break;
            case ENpcState.Idle:
                UpdateIdle();
                break;
            default:
                break;
        }

    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();

        _spRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        UDebug.IsNull(_spRenderer, LogType.Warning);
        UDebug.IsNull(_animator, LogType.Warning);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
