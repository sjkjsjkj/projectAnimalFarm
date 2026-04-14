using System.Collections;
using UnityEngine;
/// <summary>
/// 동물의 MonoBehaivour를 관리하는 객체입니다.
/// 데이터는 .Data로 접근할 수 있습니다.
/// </summary>
 
public class AnimalObject : InfoObject, ISaveable , IAutoInteractable
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

    private FoodBox _foodBox;

    //효과음들
    private string _sfxId_Cry;
    private string _sfxId_Eat;
    private string _sfxId_ProductFinish;
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
        data.isProductFinish = _isProductFinish;

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
        this._isProductFinish = data.isProductFinish;

        this._actionInterval = K.ANIMAL_ACTION_INTERVAL;
        this._tickInterval = K.ANIMAL_TICK_INTERVAL;
        this.transform.position = data.pos;
        this.transform.rotation = data.rot;

        ConnectionEvent();
        SetSfxIdSetting();
        this._productFinishIcon.gameObject.SetActive(data.data.ProductProgress >= K.MAX_PRODUCT_PROGRESS);
    }
    public bool CanInteract(GameObject player)
    {
        // 조건 검사
        return InventoryManager.Ins.PlayerInventory.CheckSlots() && _isProductFinish;
    }

    public string GetMessage()
    {
        UDebug.Print("생산 완료!");
        return "생산 완료!";
    }

    public void Interact(GameObject player)
    {
        _isProductFinish = false;

        ItemCollectionCoordinator.Ins.TryCollectItem(_productItemId, 1);

        _productFinishIcon.SetActive(false);
        _isProductFinish = false;
        _data.ProductReset();
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

        //UDebug.Print($"생산된 {tempSO.Name} 의 생산품은 {tempSO.ProductId} 입니다.");
        _productItemId = tempSO.ProductId;

        ConnectionEvent();
        SetSfxIdSetting();
        _animator.runtimeAnimatorController = tempSO.AnimController;
    }
    //효과음을 미리 캐싱해두는 메서드
    private void SetSfxIdSetting()
    {
        switch(ID)
        {
            case Id.World_Animal_Chicken:
                _sfxId_Cry = Id.Sfx_Creature_Chicken_Cries_1;
                _sfxId_Eat = Id.Sfx_Creature_Llama_Eat_1;
                break;
            case Id.World_Animal_Cow:
                _sfxId_Cry = Id.Sfx_Creature_Cow_Cries_1;
                _sfxId_Eat = Id.Sfx_Creature_Panda_Eat_03;
                break;
            case Id.World_Animal_Duck:
                _sfxId_Cry = Id.Sfx_Creature_Duck_Cries;
                _sfxId_Eat = Id.Sfx_Creature_Llama_Eat_1;
                break;
            case Id.World_Animal_Goat:
                _sfxId_Cry = Id.Sfx_Creature_Goat_Cries_1;
                _sfxId_Eat = Id.Sfx_Creature_Llama_Eat_1;
                break;
            case Id.World_Animal_Horse:
                _sfxId_Cry = Id.Sfx_Creature_Horse_Cries_1;
                _sfxId_Eat = Id.Sfx_Creature_Llama_Eat_1;
                break;
            case Id.World_Animal_Ostrich:
                _sfxId_Cry = Id.Sfx_Creature_Tiger_Cries_1;
                _sfxId_Eat = Id.Sfx_Creature_Llama_Eat_1;
                break;
            case Id.World_Animal_Pig:
                _sfxId_Cry = Id.Sfx_Creature_Pig_Cries_1;
                _sfxId_Eat = Id.Sfx_Creature_Llama_Eat_1;
                break;
            case Id.World_Animal_Sheep:
                _sfxId_Cry = Id.Sfx_Creature_SheepCries_1;
                _sfxId_Eat = Id.Sfx_Creature_Llama_Eat_1;
                break;
        }

        _sfxId_ProductFinish = Id.Sfx_Other_Alert_2;
    }
    //내가 바라보는 방향이 움직일 수 있는 곳인지 체크
    public override EPriority Priority => EPriority.Last;
    public override void ExecuteFrame()
    {
        _tickTimer += Time.deltaTime;
        _actionTimer += Time.deltaTime;

        if (_tickTimer >= _tickInterval)
        {
            //UDebug.Print($"Current State : {_state.ToString()}");
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
    private IEnumerator CoEatFoodCoroutine()
    {
        while(true)
        {
            //먹이통에 먹이가 있을 때 까지 반복
            yield return StartCoroutine(CoWaitFoodBoxInFeed());

            FeedItemSO tempFeedItemSO = _foodBox.ReturnFeed();

            _data.EatFood(tempFeedItemSO.Amount);

            _animator.SetBool("Eat", true);
            USound.PlaySfx(_sfxId_Eat,transform);
            //먹는중
            yield return new WaitForSeconds(2.0f);

            //먹고 나서도 배고픈지 확인
            if (!(_data.IsHungry))
            {
                break;
            }
        }
        _animator.SetBool("Eat", false);
        SetState(EAnimalState.Idle);
    }
    private IEnumerator CoWaitFoodBoxInFeed()
    {
        while(true)
        {
            yield return null;
            if(_foodBox.TryFindFeed())
            {
                break;
            }
        }
    }
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
                SetFaceDir(_moveDir);
                _animator.SetBool("Move", true);
                break;
            case EAnimalState.Sleep:
                break;
            case EAnimalState.Eat:
                _animator.SetBool("Move", false);
                _animator.SetBool("Eat", true);
                StartCoroutine(CoEatFoodCoroutine());
                break;
            case EAnimalState.MoveToEat:
                _animator.SetBool("Move", true);
                _moveDir = (_foodBoxPos-transform.position).normalized;
                SetFaceDir(_foodBoxPos - transform.position);
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
    //Data에서 배가고파지면 실행되는 이벤트를 구독하고 있는 메서드
    private void SetHungry()
    {
        SetState(EAnimalState.MoveToEat);
    }

    //생산완료
    private void SetProductFinish()
    {
        _isProductFinish = true;
        _productFinishIcon.gameObject.SetActive(true);
        USound.PlaySfx(_sfxId_ProductFinish, transform);
    }
    //먹이통의 위치를 알려주는 인터페이스를 건내받고 먹이통의 위치를 기억해주는 메서드.
    //BreedingArea에서 List에 AnimalObject를 추가하며 자동으로 먹이통의 위치를 알려준다고 생각하면 됩니다.
    public void SetFoodProvider(IFoodProvider foodProvider)
    {
        _foodBoxPos = foodProvider.GetFoodBoxPosition();
        _foodBox = foodProvider.GetFoodBox();
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
        if(!(_state == EAnimalState.MoveToEat))
        {
            return;
        }

        float dis = Vector3.Distance(transform.position, _foodBoxPos);

        if (dis <= 0.5f)
        {
            SetState(EAnimalState.Eat);
        }
    }
    private Vector3 RandomDirSetting()
    {
        float dirX, dirY;

        dirX = Random.Range(-1.0f, 1.0f);
        dirY = Random.Range(-1.0f, 1.0f);

        SetFaceDir(new Vector3(dirX, dirY));

        return new Vector3(dirX, dirY).normalized;
    }
    
    private void SetFaceDir(Vector3 moveDir)
    {
        int resultDir = -1;
        if (Mathf.Abs(moveDir.x) >= Mathf.Abs(moveDir.y))
        {
            resultDir = 1;  // 1 : (동)서 / 2 : 남 / 3 : 북
            if (moveDir.x >= 0.0f)
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
            resultDir = moveDir.y >= 0.0f ? 3 : 2;
        }
        _animator.SetInteger("FaceDir", resultDir);
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

        randomValue *= 10;
        if((int)(randomValue)%2==0)
        {
            USound.PlaySfx(_sfxId_Cry, transform);
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
        public bool isProductFinish;

        public float actionTimer;
        public float actionInterval;
        
        public Vector3 moveDir;

        public Vector3 pos;
        public Quaternion rot;
    }
    #endregion
}
