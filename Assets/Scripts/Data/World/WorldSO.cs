using UnityEngine;

/// <summary>
/// 월드에 존재할 수 있는 오브젝트가 가지는 정적 데이터입니다.
/// </summary>
public abstract class WorldSO : UnitSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("월드 기본 정보")]
    [SerializeField] protected Vector2 _size = new Vector2(1f, 1f);
    // ↑ 하지만 콜라이더 등으로 처리할 예정이므로 채울 필요 X
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Vector2 Size => _size;

    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_size.x <= 0) return false;
        if (_size.y <= 0) return false;
        return true;
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
    }
    #endregion
}
