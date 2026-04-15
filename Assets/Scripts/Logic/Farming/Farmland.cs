using System;
using UnityEngine;
/// <summary>
/// 경작지 각각의 타일의 정보를 담고있는 클래스
/// 플레이어가 타일의 데이터를 읽어오며 경작 가능여부를 확인할 수 있다면 일반 클래스로, 그렇지 않다면 Mono로 해서 collider Check 방식으로
/// </summary>
[Serializable]
public class Farmland
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    [SerializeField] private EFarmlandState _state;       // 땅의 단계  0:기본 흙 | 1: 일궈진 흙 | 2:
    [SerializeField] private string _seededId;           //심어진 씨앗의 id
    [SerializeField] private int _pos;            //경작지의 배열좌표
    [SerializeField] private string _harvestItemId;
    //Tick
    //N초 (미정) 마다 식물의 성장 주기를 올리는 역할을 함.
    //경작지의 상태가 MoistLand 상태일 때에만 증가함.
    //Idleland > soil(다져진땅) > seeded (씨앗뿌린땅) > MoistLand (물뿌린땅) 의 순서.
    [SerializeField] private int _grownUpTick;         //씨앗이 전부 자랄 때 까지 얼마나 많은 Tick이 지나야 하는가.
    [SerializeField] private int _currentTick;         //현재 얼마나 Tick 이 지났는가.
    [SerializeField] private float _tickTimer = 0;

    [SerializeField] private uint _soiledConnectDir;           //현재 주변에 경작지들과 같은 상태라면 (soiled 와 moist만 비교)스프라이트 연결. 이것은 현재 연결된 방향들을 Flag 형식으로 나타낸 것.
    [SerializeField] private uint _moistConnectDir;
    [SerializeField] private uint _stateFlag;            //state를 Flag 형태로 나타낸 것. 주변 경작지의 상태 비교에 사용.

    [SerializeField] private IFarmlandObjectProvider _farmlandObjectProvider;

    //효과음 Id들
    [SerializeField] private string _sfxId_Soiled;
    [SerializeField] private string _sfxId_Seeded;
    [SerializeField] private string _sfxId_Moist;
    [SerializeField] private string _sfxId_GrownUp;
    #endregion

    #region ─────────────────────────▶  외부 공개  ◀─────────────────────────
    public EFarmlandState State => _state;
    public uint StateFlag => _stateFlag;
    public string UnitId { get; set; }

    #region 테스트
    public uint SoildConnectDir => _soiledConnectDir;
    public uint MoistConnectDir => _moistConnectDir;
    #endregion


    public event Action<FarmStateChangeStruct> OnFarmStateChange;
    public event Action<FarmlandConnetionChangeStruct> OnFarmlandConnetionChange;
    public event Action<int> OnGrownUp;
    public event Action<int> OnGetHarvest;
    #endregion

    #region ─────────────────────────▶  생성자  ◀─────────────────────────
    public Farmland()
    {

    }
    public Farmland(int pos, IFarmlandObjectProvider farmlandObjectProvider)
    {
        //경작지의 배열좌표
        _pos = pos;
        SetClear();
        SetSfxIdSetting();
        _farmlandObjectProvider = farmlandObjectProvider;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    //사운드 ID를 미리 캐싱해두는 메서드
    private void SetSfxIdSetting()
    {
        _sfxId_Soiled = Id.Sfx_Other_Cutter_2;
        _sfxId_Seeded = Id.Sfx_Environment_CropGrowth_1;
        _sfxId_Moist = Id.Sfx_Environment_WaterExit_1;
        _sfxId_GrownUp = Id.Sfx_Other_BlowUp_2;
    }

    //인터랙트 시도에서 경작지의 상태가 변할 때 불러와질 메서드
    //private void SetState(EFarmlandState nextState)
    private void SetState(EFarmlandState nextState)
    {
        EFarmlandState beforeState = _state;
        Transform tr;

        _state = nextState;
        switch (_state)
        {
            case EFarmlandState.IdleLand:

                break;
            case EFarmlandState.SoiledLand:
                OnPlayerPlow.Publish();
                tr = _farmlandObjectProvider.GetFarmlandObject(_pos).transform;
                USound.PlaySfx(_sfxId_Soiled, tr);
                OnPlayerCanceled.Publish();
                OnPlayerShovel.Publish(tr.position, 0.15f);
                break;
            case EFarmlandState.SeededLand:
                OnPlayerPlantingSeeds.Publish(_seededId);
                tr = _farmlandObjectProvider.GetFarmlandObject(_pos).transform;
                USound.PlaySfx(_sfxId_Seeded, tr);
                OnPlayerCanceled.Publish();
                OnPlayerCrouching.Publish(tr.position, 0.22f);
                break;
            case EFarmlandState.MoistLand:
                tr = _farmlandObjectProvider.GetFarmlandObject(_pos).transform;
                USound.PlaySfx(_sfxId_Moist, tr);
                OnPlayerCanceled.Publish();
                OnPlayerWatering.Publish(tr.position, 0.15f);
                break;
            case EFarmlandState.GrownUp:
                tr = _farmlandObjectProvider.GetFarmlandObject(_pos).transform;
                USound.PlaySfx(_sfxId_GrownUp, tr);
                break;
        }

        _stateFlag |= (uint)nextState;

        //경작지 전체를 관리하는 FarmArea에게 나의 좌표(배열 좌표)와 상태를 전달한다.
        OnFarmStateChange?.Invoke(new FarmStateChangeStruct(_state, _pos, _seededId, _currentTick));
    }
    //최초의 경작지의 상태로
    private void SetClear()
    {
        _seededId = "";
        _harvestItemId = "";
        _grownUpTick = 1; //K.FARMLAND_GROWNTIME;
        _currentTick = 0;
        _soiledConnectDir = 0;
        _moistConnectDir = 0;

        _stateFlag = (uint)EFarmlandState.IdleLand;
        SetState(EFarmlandState.IdleLand);
        //UDebug.Print($"Clear된 경작지의 StateFlag = {_stateFlag}");
    }
    //인터랙트가 가능한지의 여부를 반환해줄 메서드
    public bool CanInteract()
    {
        switch (_state)
        {
            case EFarmlandState.IdleLand:
                return true;
            case EFarmlandState.SeededLand:
                //Todo:플레이어의 물뿌리개 레벨이..
                return true;
            case EFarmlandState.SoiledLand:
                int slotIdx = InventoryManager.Ins.PlayerInventory.FindItemType(EType.SeedItem);
                if (slotIdx == -1)
                {
                    return false;
                }
                return true;
            case EFarmlandState.MoistLand:

                return false;
            case EFarmlandState.GrownUp:
                return true;
            default:
                UDebug.Print("있을 수 없는 일");
                return false;
        }
    }

    //외부에서 인터랙트를 시도했을 때 불러와질 메서드
    public void Interact(string seedid = "") //플레이어 및 인벤토리가 제작되면 해당 부분에서 아무것도 받아오지 않아도 됨. 지금은 임시로 씨앗의 정보를 받아 옴.
    {
        UDebug.Print("Interact");

        switch (_state)
        {
            case EFarmlandState.IdleLand:
                IdleLandInteract();
                break;
            case EFarmlandState.SoiledLand:
                SoiledLandInteract();
                break;
            case EFarmlandState.SeededLand:
                SeedLandInteract();
                break;
            case EFarmlandState.MoistLand:
                MoistLandInteract();
                //아무런 효과 없음.
                break;
            case EFarmlandState.GrownUp:
                GrownUpInterInteract();
                //인벤토리에 농작물 추가 시도.
                //성공하면
                //SetState(Idleland);
                //실패하면
                //메서드 호출 스택 초기화.
                break;
        }
    }


    private void IdleLandInteract()
    {
        SetState(EFarmlandState.SoiledLand);
    }
    private void SoiledLandInteract()
    {
        int slotIdx = 0;
        Inventory playerInven = InventoryManager.Ins.PlayerInventory;

        if (playerInven == null)
        {
            UDebug.Print("플레이어 인벤 찾지 못함.");
            return;
        }
        int count = 0;
        while (true)
        {
            if (++count >= K.PLAYER_INVENTORY_SIZE)
            {
                break;
            }

            slotIdx = playerInven.FindItemType(EType.SeedItem, slotIdx);
            if (slotIdx == -1)
            {
                UDebug.Print("인벤토리에 씨앗 없음");
                return;
            }

            ItemSO tempItemSO = playerInven.GetItemType(EType.SeedItem);   //InventorySlots[slotIdx].ItemSO;

            if (!(tempItemSO as SeedItemSO))
            {
                UDebug.Print("인벤토리의 FindItemType의 반환이 잘못되었습니다.", UnityEngine.LogType.Warning);
                return;
            }
            else
            {
                //ToDo
                //씨앗의 _needFarmingLevel 과 플레이어의 농사 스탯 비교하여 같은지 확인.
                //if (DataManager.Ins.Player.농사레벨 > tempSeedItemSO.NeedFarmingLevel)
                //{
                //    _area.Farmlands[_idx].Interact(tempItemSO.Id);
                //    return;
                //}

                SeedItemSO tempSeedItemSO = (SeedItemSO)tempItemSO;
                _harvestItemId = tempSeedItemSO.HarvestItemId;

                _seededId = tempSeedItemSO.Id;

                SetState(EFarmlandState.SeededLand);

                UDebug.Print("씨앗심기!");


                //playerInven.InventorySlots[slotIdx].RemoveAmount(1);

                break;
            }
        }
    }
    private void SeedLandInteract()
    {
        SetState(EFarmlandState.MoistLand);
    }
    private void MoistLandInteract()
    {
        UDebug.Print("해당 단계에서는 인터랙트 불가.");
    }
    private void GrownUpInterInteract()
    {
        OnGetHarvest?.Invoke(_pos);
        //ItemSO tempItemSO = DatabaseManager.Ins.Item(_harvestItemId);
        if (ItemCollectionCoordinator.Ins.TryCollectItem(_harvestItemId, 1))
        {
            OnPlayerHarvesting.Publish(_harvestItemId);
            Vector3 pos = _farmlandObjectProvider.GetFarmlandObject(_pos).transform.position;
            OnPlayerCanceled.Publish();
            OnPlayerSickle.Publish(pos, 0.15f);
        }
        SetClear();
    }


    //성장 타이머
    public void Tick(float deltaTime)
    {
        // 물 준 상태가 아니거나, 이미 다 자란 상태일 경우 방어
        if (_state != EFarmlandState.MoistLand || _state == EFarmlandState.GrownUp)
        {
            return;
        }


        _tickTimer += deltaTime;

        if (_tickTimer >= _grownUpTick)
        {
            GrowUp();
        }
    }
    private void GrowUp()
    {
        _tickTimer = 0;

        if ((_currentTick = (int)MathF.Min(_currentTick + 1, K.FARMLAND_MAX_GROWNPROGRESS)) == K.FARMLAND_MAX_GROWNPROGRESS)
        {
            SetState(EFarmlandState.GrownUp);
            OnGrownUp?.Invoke(_pos);
            return;
        }

        OnFarmStateChange?.Invoke(new FarmStateChangeStruct(_state, _pos, _seededId, _currentTick));
    }
    public void SetDisConnectSoil(uint connection)
    {
        if (_soiledConnectDir == 0)
        {
            return;
        }

        _soiledConnectDir ^= connection;

        OnFarmlandConnetionChange?.Invoke(new FarmlandConnetionChangeStruct(_soiledConnectDir, EFarmlandState.SoiledLand, _pos));
    }
    public void SetDisConnectMoist(uint connection)
    {
        if (_moistConnectDir == 0)
        {
            return;
        }
        _moistConnectDir ^= connection;

        OnFarmlandConnetionChange?.Invoke(new FarmlandConnetionChangeStruct(_moistConnectDir, EFarmlandState.MoistLand, _pos));
    }


    public void SetConnectSoil()
    {
        OnFarmlandConnetionChange?.Invoke(new FarmlandConnetionChangeStruct(0, EFarmlandState.SoiledLand, _pos));
    }
    public void SetConnectSoil(uint connection)
    {
        _soiledConnectDir |= connection;
        OnFarmlandConnetionChange?.Invoke(new FarmlandConnetionChangeStruct(_soiledConnectDir, EFarmlandState.SoiledLand, _pos));
    }
    public void SetConnectMoist()
    {
        OnFarmlandConnetionChange?.Invoke(new FarmlandConnetionChangeStruct(0, EFarmlandState.MoistLand, _pos));
    }
    public void SetConnectMoist(uint connection)
    {
        _moistConnectDir |= connection;
        OnFarmlandConnetionChange?.Invoke(new FarmlandConnetionChangeStruct(_moistConnectDir, EFarmlandState.MoistLand, _pos));
    }
    /// <summary>
    /// 세이브된 데이터를 로드하기 위해 사용.
    /// 로드 시, FarmArea가 FarmLand 전체의 해당 메서드를 실행.
    /// </summary>
    public void OnLoadFunction()
    {
        // 다 자란 작물 말풍선 표시 용도
        if (_currentTick >= K.FARMLAND_MAX_GROWNPROGRESS)
        {
            SetState(EFarmlandState.GrownUp);
            OnGrownUp?.Invoke(_pos);
        }
        //
        if ((StateFlag & (int)EFarmlandState.SoiledLand) == 0)
        {
            return;
        }

        OnFarmStateChange?.Invoke(new FarmStateChangeStruct(EFarmlandState.SoiledLand, _pos, _seededId, _currentTick));

        if (_seededId == "")
        {
            return;
        }

        OnFarmStateChange?.Invoke(new FarmStateChangeStruct(EFarmlandState.SeededLand, _pos, _seededId, _currentTick));

        if ((StateFlag & (int)EFarmlandState.MoistLand) == 0)
        {
            return;
        }

        OnFarmStateChange?.Invoke(new FarmStateChangeStruct(EFarmlandState.MoistLand, _pos, _seededId, _currentTick));
    }

    // 데이터 다시 덮어씌우기
    public void Overwrite(Farmland savedData)
    {
        this._state = savedData._state;
        this._seededId = savedData._seededId;
        this._pos = savedData._pos;
        this._grownUpTick = savedData._grownUpTick;
        this._currentTick = savedData._currentTick;
        this._tickTimer = savedData._tickTimer;
        this._soiledConnectDir = savedData._soiledConnectDir;
        this._moistConnectDir = savedData._moistConnectDir;
        this._stateFlag = savedData._stateFlag;
    }
    #endregion
}
