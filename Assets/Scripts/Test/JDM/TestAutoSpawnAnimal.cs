using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TestAutoSpawnAnimal : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("참조 연결")]
    [SerializeField] private BreedingArea _area;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private float _nextTime;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Lv2;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        if(Time.time < _nextTime)
        {
            return;
        }
        _nextTime = Time.time + 5f;
        _area.SpawnAnimal(Id.World_Animal_Cow);
        _area.SpawnAnimal(Id.World_Animal_Chicken);
        _area.SpawnAnimal(Id.World_Animal_Duck);
        _area.SpawnAnimal(Id.World_Animal_Goat);
        /*_area.SpawnAnimal(Id.World_Animal_Horse);
        _area.SpawnAnimal(Id.World_Animal_Ostrich);
        _area.SpawnAnimal(Id.World_Animal_Pig);
        _area.SpawnAnimal(Id.World_Animal_Sheep);*/
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        _nextTime = Time.time;
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
