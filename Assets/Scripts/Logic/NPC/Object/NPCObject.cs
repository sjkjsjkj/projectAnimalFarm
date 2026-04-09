using System.Collections;
using UnityEngine;

/// <summary>
/// NPC의 구조 전체를 담당하는 스크립트입니다.
/// </summary>
public class NPCObject : Frameable
{
    #region ─────────────────────────▶ 인스 펙터 ◀─────────────────────────
    [Header("MonoBehaviour")]
    [SerializeField] private SpriteRenderer _spRenderer;
    [SerializeField] private NpcWorldSO _npcSO;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private NPCData _data;
    private ENpcState _state;

    private float _actionTimer = 0;       // Idle <> Move 상태를 자연스럽게 변경해줄 때 사용할 타이머
    private float _actionInterval = 3.0f; // 3초마다 한번씩 Move/Idle일 경우 랜덤하게 Move/Idle로 행동을 변경할 예정.

    [SerializeField] private Animator _animator;
    private NpcMoveTypeBase _moveMaster;

    private string _sfxId_footStepSound;
    private string _stxId_buzzingSound;
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
                StartCoroutine(CoFootStepSoundCoroutine());
                break;
            case ENpcState.Interaction:
                break;
        }
    }
    private IEnumerator CoFootStepSoundCoroutine()
    {
        while(_state == ENpcState.Move)
        {
            //int random = UnityEngine.Random.Range(1,5);
            USound.PlaySfx(_sfxId_footStepSound, transform);
          
            yield return new WaitForSeconds(0.7f);
        }
    }
    //껍데기 밖에 없던 animalObject에 스프라이트와 데이터, 애니메이터 컨트롤러를 넣어주는 작업.
    private void SetInfo()
    {
        _spRenderer.sprite = _npcSO.Image;
        
        _data = new NPCData(_npcSO);

        switch(_npcSO.NpcMoveType)
        {
            case ENpcMoveType.DontMove:
                gameObject.AddComponent<NpcMoveTypeDontMove>();
                break;
            case ENpcMoveType.AreaMove:
                if (!(_npcSO is NpcWorldAreaMoveSO areaMoveSO))
                {
                    UDebug.Print("인스펙터 에러. 모드에 맞는 SO를 넣으세요.", LogType.Assert);
                    return; 
                }
                UDebug.Print("AreaMove Mode Npc 생성");
                NpcMoveTypeAreaMove tempAreaMoveMode = gameObject.AddComponent<NpcMoveTypeAreaMove>();
                tempAreaMoveMode.InitSetting(areaMoveSO.InitPosition, areaMoveSO.MinPos, areaMoveSO.MaxPos,areaMoveSO.MoveSpeed);

                tempAreaMoveMode.GetComponent<NpcMoveTypeAreaMove>().OnNextMove -= Reorganize;
                tempAreaMoveMode.GetComponent<NpcMoveTypeAreaMove>().OnNextMove += Reorganize;
                break;
            case ENpcMoveType.PatrolMove:
                if (!(_npcSO is NpcWorldPatrolMoveSo patrolMoveSO))
                {
                    UDebug.Print("인스펙터 에러. 모드에 맞는 SO를 넣으세요.", LogType.Assert);
                    return;
                }
                NpcMoveTypePatrolMove tempPatrolMode = gameObject.AddComponent<NpcMoveTypePatrolMove>();
                tempPatrolMode.InitSetting(patrolMoveSO.PatrolPoints, patrolMoveSO.MoveSpeed);

                tempPatrolMode.GetComponent<NpcMoveTypePatrolMove>().OnNextMove -= Reorganize;
                tempPatrolMode.GetComponent<NpcMoveTypePatrolMove>().OnNextMove += Reorganize;
                break;
        }

        _moveMaster = GetComponent<NpcMoveTypeBase>();
        //TODO: 이벤트 연결
        _animator.runtimeAnimatorController = _npcSO.AnimController;

        _sfxId_footStepSound = _npcSO.FootStepSoundId;
        _stxId_buzzingSound = _npcSO.BuzzingSoundId;

        SetState(ENpcState.Idle);
    }
    private void Reorganize()
    {
        SetState(ENpcState.Idle);
    }
    private void UpdateInteraction()
    {
        
    }
    private void UpdateMove()
    {
        //UDebug.Print("이동중");
        _moveMaster.Move();
    }
    private void UpdateIdle()
    {
        //UDebug.Print("멍때리는중");
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
        float randomValue = Random.Range(0.0f, 1.0f);
       
        if (randomValue >= 0.5f)
        {
            SetState(ENpcState.Move);
            //UDebug.Print("이제부터 움직여라");
            FaceDirCheck(_moveMaster.NextTargetFind());
        }
        else
        {
            SetState(ENpcState.Idle);
        }
    }

    private void FaceDirCheck(int resultFaceDir)
    {
        if(resultFaceDir == 1)
        {
            _spRenderer.flipX = true;
        }
        else
        {
            _spRenderer.flipX = false;
        }
        _animator.SetInteger("FaceDir", resultFaceDir);
    }


    public override EPriority Priority => EPriority.Last;
    public override void ExecuteFrame()
    {
        _actionTimer += Time.deltaTime;

        if (_actionTimer >= _actionInterval)
        {
            RandomAction();
            float tempRandNum = Random.Range(0.0f, 1.0f)*10;
            if ((int)(tempRandNum) % 2 == 0)
            {
                USound.PlaySfx(_stxId_buzzingSound,transform);
            }
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

        SetInfo();
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
