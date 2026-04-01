using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 초기 스탯과 설정 값을 담는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerWorldSO_", menuName = "ScriptableObjects/World/Player", order = 1)]
public class PlayerWorldSO : WorldSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("애니메이션")]
    [SerializeField, CsvIgnore] protected RuntimeAnimatorController _animController;

    [Header("이동 초기값")]
    [SerializeField] protected float _walkSpeed = 5f;
    [SerializeField] protected float _runMultiplier = 1.5f;

    [Header("스탯 최대치 초기값")]
    [SerializeField] protected float _maxStamina = 100f; // 최대 스태미나
    [SerializeField] protected float _maxHunger = 100f; // 최대 만복도
    [SerializeField] protected float _maxThirst = 100f; // 최대 목마름

    [Header("최대 돈 보유량")]
    [SerializeField] protected int _maxMoney = int.MaxValue;

    [Header("기술 레벨 경험치 테이블")]
    [SerializeField] protected SkillExpData[] _skillExpTables;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Dictionary<ELifeSkill, int[]> _skillExpDict;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public RuntimeAnimatorController AnimController => _animController;
    public float WalkSpeed => _walkSpeed;
    public float RunMultiplier => _runMultiplier;
    public float MaxStamina => _maxStamina;
    public float MaxHunger => _maxHunger;
    public float MaxThirst => _maxThirst;
    public int MaxMoney => _maxMoney;

    /// <summary>
    /// 생활 기술 레벨업을 위한 경험치를 반환합니다.
    /// </summary>
    /// <param name="skill">생활 스킬</param>
    /// <param name="curLevel">현재 레벨</param>
    /// <returns></returns>
    public int GetNeedExp(ELifeSkill skill, int curLevel)
    {
        // 딕셔너리가 비어있다면 지연 초기화
        if (_skillExpDict == null)
        {
            BuildExpDict();
        }
        // 해당 스킬의 테이블이 존재하지 않거나 비어있으면 0 반환
        if (!_skillExpDict.TryGetValue(skill, out int[] table)
            || table == null
            || table.Length <= 0)
        {
            UDebug.Print($"스킬({skill})이 테이블에 존재하지 않습니다.", LogType.Assert);
            return 0;
        }
        // 인덱스 클램프
        int index = Mathf.Clamp(curLevel - 1, 0, table.Length - 1);
        return table[index];
    }

    public int GetMaxLevel(ELifeSkill skill)
    {
        // 딕셔너리가 비어있다면 지연 초기화
        if (_skillExpDict == null)
        {
            BuildExpDict();
        }
        // 해당 스킬의 테이블이 존재하지 않거나 비어있으면 0 반환
        if (!_skillExpDict.TryGetValue(skill, out int[] table)
            || table == null
            || table.Length <= 0)
        {
            UDebug.Print($"스킬({skill})이 테이블에 존재하지 않습니다.", LogType.Assert);
            return 0;
        }
        // 인덱스 클램프
        return table.Length;
    }

    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_type != EType.PlayerWorld) return false;
        if (_animController == null) return false;
        if (_walkSpeed <= 0f) return false;
        if (_runMultiplier <= 1f) return false;
        if (_maxStamina <= 0f) return false;
        if (_maxHunger <= 0f) return false;
        if (_maxThirst <= 0f) return false;
        if (_maxMoney <= 0) return false;
        if (!UArray.IsInitedArray(_skillExpTables)) return false;
        for (int i = 0; i < _skillExpTables.Length; i++)
        {
            SkillExpData table = _skillExpTables[i];
            if (!UArray.IsInitedArray(table.expTable)) return false;
            if (table.skillType == ELifeSkill.None) return false;
            int[] exp = table.expTable;
            for (int j = 0; j < exp.Length; ++j)
            {
                if (exp[j] <= 0) return false;
            }
        }
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 구조체 배열을 딕셔너리로 변환합니다.
    private void BuildExpDict()
    {
        _skillExpDict = new Dictionary<ELifeSkill, int[]>();
        if (_skillExpTables == null)
        {
            return;
        }
        // 딕셔너리 작성
        for (int i = 0; i < _skillExpTables.Length; i++)
        {
            SkillExpData table = _skillExpTables[i];
            if (!_skillExpDict.ContainsKey(table.skillType))
            {
                _skillExpDict.Add(table.skillType, table.expTable);
            }
            else
            {
                UDebug.Print($"PlayerWorldSO({_id})에 중복된 스킬 테이블({table.skillType})이 존재합니다.", LogType.Assert);
            }
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    // 인스펙터 변수 유효성 검사
    protected override void OnValidate()
    {
        base.OnValidate();
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {_id}, Type = {this.GetType().Name})", LogType.Warning);
        }
        else
        {
            // 에디터에서 값을 수정할 때마다 즉각적으로 딕셔너리를 동기화
            BuildExpDict();
        }
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    [Serializable]
    public struct SkillExpData
    {
        public ELifeSkill skillType;
        public int[] expTable;
    }
    #endregion
}
