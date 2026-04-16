using System.Collections;
using TMPro;
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
    [SerializeField] private NpcMoveTypeSO _moveTypeSO;

    [SerializeField] private string[] _dialog;
    [SerializeField] private GameObject _dialogCanvas;
    [SerializeField] private RectTransform _dialogBack;
    [SerializeField] private TextMeshProUGUI _dialogText;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private NPCData _data;
    private ENpcState _state;
    private ENpcMoveType _moveType;

    private float _actionTimer = 0;       // Idle <> Move 상태를 자연스럽게 변경해줄 때 사용할 타이머
    private float _actionInterval = 1.0f; // 3초마다 한번씩 Move/Idle일 경우 랜덤하게 Move/Idle로 행동을 변경할 예정.

    private float _dialogTimer = 0;
    private float _dialogInterval = 10.0f;
    private float _dialogMessageTime = 5.0f;
    private Vector3 _initPos;

    [SerializeField] private Animator _animator;
    private NpcMoveTypeBase _moveMaster;

    private string _sfxId_footStepSound;
    private string _stxId_buzzingSound;

    private bool _isTalked;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public NPCData Data => _data;
    public SpriteRenderer SpRenderer => _spRenderer;
    public Animator Animator => _animator;

    public void ShowDialog()
    {
        if (_dialog.Length == 0)
        {
            return;
        }
        RandomDialog(1);
    }
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

        _initPos = transform.position;

        if (_moveTypeSO == null)
        {
            UDebug.Print("인스펙터 에러. 모드에 맞는 SO를 넣으세요.", LogType.Assert);
            return;
        }

        if ((_moveTypeSO is NpcWorldDontMoveSO dontMoveSO))
        {
            _moveType = ENpcMoveType.DontMove;
        }

        else if ((_moveTypeSO is NpcWorldAreaMoveSO areaMoveSO))
        {
            _moveType = ENpcMoveType.AreaMove;
            UDebug.Print("AreaMove Mode Npc 생성");
            NpcMoveTypeAreaMove tempAreaMoveMode = gameObject.AddComponent<NpcMoveTypeAreaMove>();
            tempAreaMoveMode.InitSetting(_initPos, areaMoveSO.MinPos, areaMoveSO.MaxPos, _npcSO.MoveSpeed);

            tempAreaMoveMode.GetComponent<NpcMoveTypeAreaMove>().OnNextMove -= Reorganize;
            tempAreaMoveMode.GetComponent<NpcMoveTypeAreaMove>().OnNextMove += Reorganize;
        }

        else if (_moveTypeSO is NpcWorldPatrolMoveSo patrolMoveSO)
        {
            _moveType = ENpcMoveType.PatrolMove;
            NpcMoveTypePatrolMove tempPatrolMode = gameObject.AddComponent<NpcMoveTypePatrolMove>();
            tempPatrolMode.InitSetting(patrolMoveSO.PatrolPoints, _npcSO.MoveSpeed);

            tempPatrolMode.GetComponent<NpcMoveTypePatrolMove>().OnNextMove -= Reorganize;
            tempPatrolMode.GetComponent<NpcMoveTypePatrolMove>().OnNextMove += Reorganize;
        }
        
        _moveMaster = GetComponent<NpcMoveTypeBase>();
        //TODO: 이벤트 연결
        _animator.runtimeAnimatorController = _npcSO.AnimController;

        _sfxId_footStepSound = _npcSO.FootStepSoundId;
        _stxId_buzzingSound = _npcSO.BuzzingSoundId;

        _animator.SetInteger("FaceDir", 2);

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
        if(_moveType == ENpcMoveType.DontMove)
        {
            return;
        }
        if (!(_state == ENpcState.Idle || _state == ENpcState.Move))
        {
            return;
        }
        _actionInterval = 0.5f;//Random.Range(2.0f, 4.0f);
        _actionTimer = 0;

        //랜덤 행동
        float randomValue = Random.Range(0.0f, 1.0f);
       
        if (randomValue >= 0.5f)
        {
            SetState(ENpcState.Move);
            //UDebug.Print("이제부터 움직여라");
            SetFaceDir(_moveMaster.NextTargetFind());
        }
        else
        {
            SetState(ENpcState.Idle);
        }

        float tempRandNum = Random.Range(0.0f, 1.0f) * 10;
        if ((int)(tempRandNum) % 2 == 0)
        {
            USound.PlaySfx(_stxId_buzzingSound, transform);
        }
    }
    private bool RandomDialog(float percentage = 0.5f)
    {
        if(_isTalked)
        {
            return false;
        }
        float tempRandNum = Random.Range(0.0f, 1.0f);

        if(tempRandNum >= percentage)
        {
            return false;
        }

        int tempRandDialogIdx = Random.Range(0, _dialog.Length);
        //UDebug.Print($"show [{tempRandDialogIdx}] Dialog | Dialog Length : {_dialog.Length}");
        //UDebug.Print($"{ _dialog[tempRandDialogIdx] }");
        StartCoroutine(CoDialog(_dialog[tempRandDialogIdx]));
        return true;
    }
    private IEnumerator CoDialog(string dialog)
    {
        _isTalked = true;
        _dialogBack.localScale = new Vector3(0.6f , 0.7f, 1);
        _dialogCanvas.SetActive(true);
        _dialogText.text = dialog;
        yield return new WaitForSeconds(_dialogMessageTime);
        _dialogCanvas.SetActive(false);
        _isTalked = false;
    }

    private void SetFaceDir(Vector3 targetDir)
    {
        Vector3 moveDir = targetDir - transform.position;

        int resultDir = -1;
        if (Mathf.Abs(moveDir.x) >= Mathf.Abs(moveDir.y))
        {
            resultDir = 1;  // 1 : (동)서 / 2 : 남 / 3 : 북
            if (moveDir.x >= 0.0f)
            {
                _spRenderer.flipX = false;
            }
            else
            {
                _spRenderer.flipX = true;
            }
        }
        else
        {
            resultDir = moveDir.y >= 0.0f ? 3 : 2;
        }
        _animator.SetInteger("FaceDir", resultDir);
    }

    public override EPriority Priority => EPriority.Last;
    public override void ExecuteFrame()
    {
        if(_state == ENpcState.Idle)
        {
            _actionTimer += Time.deltaTime;
        }
        if (_dialog.Length > 0)
        {
            _dialogTimer += Time.deltaTime;
        }

        if (_actionTimer >= _actionInterval)
        {
            RandomAction();
        }
        if (_dialogTimer >= _dialogInterval )
        {
            if (!RandomDialog())
            {
                _dialogInterval += 2;
            }
            else
            {
                _dialogTimer = 0;
                _dialogInterval = 10;
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
