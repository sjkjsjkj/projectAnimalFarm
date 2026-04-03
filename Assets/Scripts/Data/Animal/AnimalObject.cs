using UnityEngine;
/// <summary>
/// 동물의 MonoBehaivour를 관리하는 객체입니다.
/// 데이터는 .Data로 접근할 수 있습니다.
/// </summary>
 
public class AnimalObject : InfoObject, ISaveable , IInteractable
{
    #region ─────────────────────────▶ 인스 펙터 ◀─────────────────────────
    [Header("MonoBehaviour")]
    [SerializeField] private GameObject _productFinishIcon;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private SpriteRenderer _spRenderer;
    private AnimalData _data;
    private EAnimalState _state;
    private float _tickTimer = 0;        // Data는 일반 클래스이기 때문에 Update를 사용할 수 없음. 
    private float _tickInterval = K.ANIMAL_TICK_INTERVAL; // 그렇기 때문에 여기서 Update를 대신 돌아 줌. 

    private float _actionTimer = 0;       // Idle <> Move 상태를 자연스럽게 변경해줄 때 사용할 타이머
    private float _actionInterval = K.ANIMAL_ACTION_INTERVAL; // 3초마다 한번씩 Move/Idle일 경우 랜덤하게 Move/Idle로 행동을 변경할 예정.
    private Animator _animator;

    private Vector3 _foodBoxPos;
    private Vector3 _moveDir;

    private string _productItemId;
    private bool _isProductFinish;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public AnimalData Data => _data;
    public string ID => _data.Id;
    // 매니저에서 사용할 유닛 ID
    public string UnitId { get; set; } = Id.World_Animal_Cow;

    // 매니저에서 호출하는 데이터 직렬화 함수
    public string SaveData()
    {
        UnitSaveData data = new();
        data.id = ID;
        data.data = _data;
        data.state = _state;
        data.tickTimer = _tickTimer;
        data.actionTimer = _actionTimer;
        data.moveDir = _moveDir;

        data.productItemId = _productItemId;

        data.pos = this.transform.position;
        data.rot = this.transform.rotation;
        return JsonUtility.ToJson(data, true);
    }

    // 매니저에서 호출하는 데이터 복구 함수
    public void LoadData(string dataJson)
    {
        UnitSaveData data = JsonUtility.FromJson<UnitSaveData>(dataJson);
        
        this._data = new AnimalData(data.data);
        this._state = data.state;
        this._tickTimer = data.tickTimer;
        this._actionTimer = data.actionTimer;

        AnimalWorldSO tempSO = DatabaseManager.Ins.AnimalWorld(data.id);

        this._animator.runtimeAnimatorController = tempSO.AnimController;
        this._foodBoxPos = new Vector3(K.FOOD_BOX_POS_X, K.FOOD_BOX_POS_Y, 0);

        this._moveDir = data.moveDir;

        this._productItemId = data.productItemId;

        this._actionInterval = K.ANIMAL_ACTION_INTERVAL;
        this._tickInterval = K.ANIMAL_TICK_INTERVAL;
        this.transform.position = data.pos;
        this.transform.rotation = data.rot;

        ConnectionEvent();

        this._productFinishIcon.gameObject.SetActive(data.data.ProductProgress >= K.MAX_PRODUCT_PROGRESS);
    }

    public bool OnInteract() 
    {
        if(!_isProductFinish)
        {
            return false;
        }

        UDebug.Print("인터랙트!");
        //여기서 직접 아이템을 건내줘도 되는건가?

        ItemCollectionCoordinator.Ins.TryCollectItem(_productItemId, 1);

        _productFinishIcon.SetActive(false);
        _isProductFinish = false;
        _data.ProductReset();

        return true;
    }

    public override void SetInfo(UnitSO dataSO)
    {
        _spRenderer.sprite = dataSO.Image;
        if (!(dataSO as AnimalWorldSO))
        {
            UDebug.Print("잘못된 데이터가 들어오고 있음. 이 부분에서는 AnimalWorldSO가 들어와야함.", LogType.Warning);
            return;
        }
        AnimalWorldSO tempSO = (AnimalWorldSO)dataSO;

        _data = new AnimalData(tempSO);

        UDebug.Print($"생산된 {tempSO.Name} 의 생산품은 {tempSO.ProductId} 입니다.");
        _productItemId = tempSO.ProductId;

        ConnectionEvent();
        
        _animator.runtimeAnimatorController = tempSO.AnimController;
    }
    //내가 바라보는 방향이 움직일 수 있는 곳인지 체크
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
            //UDebug.Print($"랜덤 액션 발동!");

            RandomAction();
        }

        switch (_state)
        {
            case EAnimalState.MoveToEat:
                UpdateMoveToEat();
                break;
            case EAnimalState.Move:
                UpdateMove();
                break;
            case EAnimalState.Sleep:
                UpdateSleep();
                break;
            case EAnimalState.Eat:
                UpdateEat();
                break;
            default:
                break;
        }

    }

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void SetState(EAnimalState nextState)
    {
        EAnimalState prevState = _state;
        _state = nextState;
        switch (_state)
        {
            case EAnimalState.Idle:
                _animator.SetBool("Move", false);
                break;
            case EAnimalState.Move:
                _moveDir = RandomDirSetting();
                _animator.SetBool("Move", true);
                break;
            case EAnimalState.Sleep:
                break;
            case EAnimalState.Eat:
                break;
            case EAnimalState.MoveToEat:
                _moveDir = (_foodBoxPos - transform.localPosition).normalized;
                break;
            case EAnimalState.MoveToBed:
                break;
            case EAnimalState.Dead:
                break;
        }
    }
    private void ConnectionEvent()
    {
        _data.OnHungry -= SetHungry;
        _data.OnHungry += SetHungry;

        _data.OnProductFinish -= SetProductFinish;
        _data.OnProductFinish += SetProductFinish;
    }
    //껍데기 밖에 없던 animalObject에 스프라이트와 데이터, 애니메이터 컨트롤러를 넣어주는 작업.
    
    private void SetHungry()
    {
        SetState(EAnimalState.MoveToEat);
    }
    private void SetProductFinish()
    {
        _isProductFinish = true;
        _productFinishIcon.gameObject.SetActive(true);
    }
    //먹이통의 위치를 알려주는 인터페이스를 건내받고 먹이통의 위치를 기억해주는 메서드.
    //BreedingArea에서 List에 AnimalObject를 추가하며 자동으로 먹이통의 위치를 알려준다고 생각하면 됩니다.
    public void SetFoodProvider(IFoodProvider foodProvider)
    {
        _foodBoxPos = foodProvider.GetFoodBoxPosition();
    }
 
    
    private void UpdateMoveToEat()
    {
        Move(_moveDir);
    }
    private void UpdateMove()
    {
        Move(_moveDir);
    }
    private void UpdateSleep()
    {

    }
    private void UpdateEat()
    {

    }
    private void Move(Vector3 dir)
    {
        var tile = TileManager.Ins.Tile;
        if (tile != null)
        {
            transform.position = tile.GetValidPos(transform.position, _data.Size, dir, Time.deltaTime);
        }
    }
    private Vector3 RandomDirSetting()
    {
        float dirX, dirY;
        int resultDir;  // 1 : (동)서 / 2 : 남 / 3 : 북
        dirX = Random.Range(-1.0f, 1.0f);
        dirY = Random.Range(-1.0f, 1.0f);

        if (Mathf.Abs(dirX) >= Mathf.Abs(dirY))
        {
            resultDir = 1;
            if (dirX >= 0.0f)
            {
                _spRenderer.flipX = true;
            }
            else
            {
                _spRenderer.flipX = false;
            }
        }
        else
        {
            resultDir = dirY >= 0.0f ? 3 : 2;
        }
        _animator.SetInteger("FaceDir", resultDir);

        return new Vector3(dirX, dirY).normalized;
    }
    //Idle <> Move 상태를 자연스럽게 변경해주기 위해 다음 액션을 랜덤하게 선택
    private void RandomAction()
    {
        if (!(_state ==  EAnimalState.Idle  || _state == EAnimalState.Move))
        {
            return;
        }

        _actionInterval = Random.Range(2.0f, 4.0f);
        _actionTimer = 0;

        //랜덤 행동
        //현재 Idle 애니메이션이 없는 동물이 많아서 이동의 비중을 높힘.
        float randomValue = Random.Range(0.0f, 1.0f);
        //UDebug.Print($"RandomAction : {randomValue}");
        if(randomValue >= 0.3f)
        {
            SetState(EAnimalState.Move);
        }
        else
        {
            SetState(EAnimalState.Idle);
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
    [System.Serializable]
    private struct UnitSaveData
    {
        public string id;
        public AnimalData data;
        public EAnimalState state;
        public float tickTimer;
        public float tickInterval;

        public string productItemId;

        public float actionTimer;
        public float actionInterval;
        
        public Vector3 moveDir;

        public Vector3 pos;
        public Quaternion rot;
    }
    #endregion
}
