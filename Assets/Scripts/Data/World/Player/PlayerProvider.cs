using System;
using UnityEngine;

/// <summary>
/// 플레이어의 런타임 데이터를 저장 및 가공하는 클래스
/// </summary>
[Serializable]
public class PlayerProvider
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    // 위치 정보
    [SerializeField] private Vector2 _position;
    [SerializeField] private Vector2 _direction;
    // 상태 / 장비 / 재화
    [SerializeField] private EPlayerState _state;
    [SerializeField] private string _heldItemId;
    [SerializeField] private int _money;
    // 스탯 최대치
    [SerializeField] private float _maxWalkSpeed; // 초당 움직이는 거리
    [SerializeField] private float _maxRunMultiplier; // 달릴 시 몇 배 빨라지는가
    [SerializeField] private float _maxStamina;
    [SerializeField] private float _maxHunger;
    [SerializeField] private float _maxThirst;
    // 현재 상태
    [SerializeField] private float _curWalkSpeed;
    [SerializeField] private float _curRunMultiplier;
    [SerializeField] private float _curStamina;
    [SerializeField] private float _curHunger;
    [SerializeField] private float _curThirst;
    // 생활 기술 데이터
    [SerializeField] private int[] _skillLevels;
    [SerializeField] private int[] _skillExps;
    #endregion

    #region ─────────────────────────▶ 생성자 ◀─────────────────────────
    // 직렬화를 위한 빈 생성자
    public PlayerProvider() { }

    /// <summary>
    /// 플레이어 초기 설정
    /// </summary>
    /// <param name="so">플레이어 SO</param>
    public PlayerProvider(PlayerWorldSO so)
    {
        Initialize(so, K.PLAYER_SKILL_COUNT);
    }

    // 나중에 데이터 로드용으로 만들어둔 생성자
    public PlayerProvider(
        Vector2 position, Vector2 direction,
        EPlayerState state, string heldItemId, int money,
        float maxWalkSpeed, float maxRunMultiplier, float maxStamina, float maxHunger, float maxThirst,
        float curWalkSpeed, float curRunMultiplier, float curStamina, float curHunger, float curThirst,
        int[] skillLevels, int[] skillExps)
    {
        this._position = position;
        this._direction = direction;
        this._state = state;
        this._heldItemId = heldItemId;
        this._money = money;
        this._maxWalkSpeed = maxWalkSpeed;
        this._maxRunMultiplier = maxRunMultiplier;
        this._maxStamina = maxStamina;
        this._maxHunger = maxHunger;
        this._maxThirst = maxThirst;
        this._curWalkSpeed = curWalkSpeed;
        this._curRunMultiplier = curRunMultiplier;
        this._curStamina = curStamina;
        this._curHunger = curHunger;
        this._curThirst = curThirst;
        this._skillLevels = skillLevels;
        this._skillExps = skillExps;
    }

    /// <summary>
    /// SO 데이터로 플레이어 데이터 초기화
    /// </summary>
    public void Initialize(PlayerWorldSO so, int lifeSkillCount)
    {
        if (so == null)
        {
            UDebug.Print($"플레이어 초기화 로직에서 빈 SO를 받았습니다.", LogType.Warning);
            return;
        }
        // 위치 정보
        _position = Vector2.zero;
        _direction = Vector2.down;
        // 상태 / 장비 / 재화
        _state = EPlayerState.Idle;
        _heldItemId = string.Empty;
        _money = 0;
        // 현재 스탯
        _curWalkSpeed = so.WalkSpeed;
        _curRunMultiplier = so.RunMultiplier;
        _curStamina = so.MaxStamina;
        _curHunger = so.MaxHunger;
        _curThirst = so.MaxThirst;
        // 스탯 최대치
        _maxWalkSpeed = so.WalkSpeed;
        _maxRunMultiplier = so.RunMultiplier;
        _maxStamina = so.MaxStamina;
        _maxHunger = so.MaxHunger;
        _maxThirst = so.MaxThirst;
        // 생활 스킬
        _skillLevels = new int[lifeSkillCount];
        _skillExps = new int[lifeSkillCount];
        for (int i = 0; i < lifeSkillCount; ++i)
        {
            _skillLevels[i] = 1;
            _skillExps[i] = 0;
        }
        UDebug.Print($"플레이어 프로바이더 초기화를 완료했습니다.");
    }

    /// <summary>
    /// 플레이어가 초기화되지 않은 상태인지 검사하여 반환합니다.
    /// </summary>
    public bool IsEmpty() => (_state == EPlayerState.None);
    #endregion

    #region ─────────────────────────▶ 읽기 전용 멤버 ◀─────────────────────────
    public Vector2 Position => _position;
    public Vector2 Direction => _direction;
    public EPlayerState State => _state;
    public string HeldItemId => _heldItemId;
    public int Money => _money;
    public float MaxWalkSpeed => _maxWalkSpeed;
    public float MaxRunMultiplier => _maxRunMultiplier;
    public float MaxStamina => _maxStamina;
    public float MaxHunger => _maxHunger;
    public float MaxThirst => _maxThirst;
    public float CurWalkSpeed => _curWalkSpeed;
    public float CurRunMultiplier => _curRunMultiplier;
    public float CurStamina => _curStamina;
    public float CurHunger => _curHunger;
    public float CurThirst => _curThirst;
    public int GetSkillLevel(ELifeSkill skill) => _skillLevels[(int)skill - 1];
    public int GetSkillExp(ELifeSkill skill) => _skillExps[(int)skill - 1];
    #endregion

    #region ─────────────────────────▶ 설정 전용 멤버 ◀─────────────────────────
    /// <summary>
    /// 매 프레임 갱신되어야 합니다.
    /// 타일 이동 판단, 세이브 등에 사용됩니다.
    /// </summary>
    /// <param name="pos">현재 좌표</param>
    /// <param name="dir">현재 방향</param>
    public void SetTransform(Vector2 pos, Vector2 dir)
    {
        _position = pos;
        _direction = dir;
    }

    /// <summary>
    /// 플레이어의 상태를 변경합니다.
    /// </summary>
    /// <param name="newState">새로운 상태</param>
    public void ChangeState(EPlayerState newState)
    {
        if (_state == newState)
        {
            return;
        }
        OnPlayerStateChanged.Publish(_state, newState);
        _state = newState;
    }

    /// <summary>
    /// 플레이어가 손에 든 아이템을 변경합니다.
    /// </summary>
    /// <param name="itemId">아이템 ID</param>
    public void SetHeldItem(string itemId)
    {
        _heldItemId = itemId;
        OnPlayerHeldItemChanged.Publish(itemId);
    }

    /// <summary>
    /// 플레이어에게 돈을 지급합니다.
    /// 지급되지 못한 돈 수량을 반환합니다.
    /// </summary>
    /// <param name="amount">지급량</param>
    public int AddMoney(int amount)
    {
        if(amount <= 0)
        {
            return 0;
        }
        // 오버플로우 방지
        if (_money >= K.PLAYER_MAX_MONEY)
        {
            return amount;
        }
        // 오버플로우 방지 (변화 있음)
        if (K.PLAYER_MAX_MONEY - amount < _money)
        {
            int remain = amount - (K.PLAYER_MAX_MONEY - _money); // 지급하지 못한 돈
            _money = K.PLAYER_MAX_MONEY; // 최대치로 설정
            OnPlayerMoneyChanged.Publish(_money);
            return remain;
        }
        // 정상 처리
        _money += amount;
        OnPlayerMoneyChanged.Publish(_money);
        return 0;
    }

    /// <summary>
    /// 플레이어의 돈을 차감합니다.
    /// 차감하지 못한 돈 수량을 반환합니다.
    /// </summary>
    /// <param name="amount">차감량</param>
    public int TakeMoney(int amount)
    {
        if(_money <= 0 || amount <= 0)
        {
            return 0;
        }
        _money -= amount;
        if(_money < 0) // 돈이 음수가 되었을 경우
        {
            int remain = -_money;
            _money = 0;
            OnPlayerMoneyChanged.Publish(_money);
            return remain;
        }
        OnPlayerMoneyChanged.Publish(_money);
        return 0;
    }

    /// <summary>
    /// 스태미나를 소모합니다.
    /// 행동 요구량보다 현재 스태미나가 부족할 경우 행동을 취소(False 반환)합니다.
    /// </summary>
    /// <param name="amount">소모량</param>
    public bool ConsumeStamina(float amount)
    {
        // 0 이하 무시 / 스태미나가 충분한지?
        if (amount <= 0f || _curStamina < amount)
        {
            return false;
        }
        // 적용
        _curStamina -= amount;
        // 부동소수점 클램프
        if (_curStamina < 0f)
        {
            _curStamina = 0f;
        }
        OnPlayerStaminaChanged.Publish(_curStamina, _maxStamina);
        return true;
    }

    /// <summary>
    /// 스태미나를 회복합니다.
    /// 이미 최대치 이상일 경우 처리하지 않습니다(False 반환).
    /// </summary>
    /// <param name="amount">회복량</param>
    public bool RecoverStamina(float amount)
    {
        // 0 이하 무시 / 이미 컨디션 풀인지?
        if (amount <= 0f || _curStamina >= _maxStamina)
        {
            return false;
        }
        // 적용
        _curStamina = Mathf.Min(_curStamina + amount, _maxStamina);
        OnPlayerStaminaChanged.Publish(_curStamina, _maxStamina);
        return true;
    }


    /// <summary>
    /// 만복도를 소모합니다.
    /// 이미 0일 경우 False를 반환합니다.
    /// </summary>
    /// <param name="amount">소모량</param>
    public bool ConsumeHunger(float amount)
    {
        // 음수 차단 / 이미 허기짐
        if (amount < 0 || _curHunger <= 0f)
        {
            return false;
        }
        // 적용
        _curHunger = Mathf.Max(_curHunger - amount, 0f);
        OnPlayerHungerChanged.Publish(_curHunger, _maxHunger);
        return true;
    }

    /// <summary>
    /// 만복도를 회복합니다.
    /// 이미 최대치일 경우 False를 반환합니다.
    /// </summary>
    /// <param name="amount">회복량</param>
    public bool RecoverHunger(float amount)
    {
        // 음수 차단 / 이미 배부름
        if (amount < 0 || _curHunger >= _maxHunger)
        {
            return false;
        }
        // 적용
        _curHunger = Mathf.Min(_curHunger + amount, _maxHunger);
        OnPlayerHungerChanged.Publish(_curHunger, _maxHunger);
        return true;
    }

    /// <summary>
    /// 목마름을 소모합니다.
    /// 이미 0일 경우 False를 반환합니다.
    /// </summary>
    /// <param name="amount">소모량</param>
    public bool ConsumeThirst(float amount)
    {
        // 음수 차단 / 이미 목마름
        if (amount < 0 || _curThirst <= 0f)
        {
            return false;
        }
        // 적용
        _curThirst = Mathf.Max(_curThirst - amount, 0f);
        OnPlayerThirstChanged.Publish(_curThirst, _maxThirst);
        return true;
    }

    /// <summary>
    /// 목마름을 회복합니다.
    /// 이미 최대치일 경우 False를 반환합니다.
    /// </summary>
    /// <param name="amount">회복량</param>
    public bool RecoverThirst(float amount)
    {
        // 음수 차단 / 해갈
        if (amount < 0 || _curThirst >= _maxThirst)
        {
            return false;
        }
        // 적용
        _curThirst = Mathf.Min(_curThirst + amount, _maxThirst);
        OnPlayerThirstChanged.Publish(_curThirst, _maxThirst);
        return true;
    }

    /// <summary>
    /// 생활 기술 경험치를 증가시킵니다.
    /// 조건이 만족되면 자동으로 레벨 업 합니다.
    /// </summary>
    public void AddSkillExp(ELifeSkill skill, int expAmount, PlayerWorldSO so)
    {
        if (expAmount <= 0 || skill == ELifeSkill.None)
        {
            return;
        }
        int index = (int)skill - 1;
        OnPlayerSkillExpUp.Publish(skill, _skillExps[index], expAmount);
        _skillExps[index] += expAmount;
        TryLevelUp(skill, so);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 레벨 업을 시도합니다.
    private void TryLevelUp(ELifeSkill skill, PlayerWorldSO so)
    {
        int index = (int)skill - 1;
        int needExp = so.GetNeedExp(skill, _skillLevels[index]);
        bool levelUp = false;
        // 레벨 업 조건 검사
        int maxLevel = so.GetMaxLevel(skill);
        while (_skillExps[index] >= needExp && _skillLevels[index] < maxLevel)
        {
            levelUp = true;
            // 레벨 업
            _skillExps[index] -= needExp;
            _skillLevels[index]++;
            UDebug.Print($"생활 기술({skill})의 레벨이 {_skillLevels[index]} 레벨로 상승했습니다!");
            // 다음 레벨업 요구치 갱신
            needExp = so.GetNeedExp(skill, _skillLevels[index]);
        }
        // 레벨 업 하였음
        if (levelUp)
        {
            OnPlayerSkillLevelUp.Publish(skill, _skillLevels[index]);
        }
    }
    #endregion
}
