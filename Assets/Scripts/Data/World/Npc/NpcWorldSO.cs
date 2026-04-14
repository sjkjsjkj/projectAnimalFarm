using UnityEngine;

/// <summary>
/// NPC가 월드에서 가지는 정적 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "NpcWorldSO_", menuName = "ScriptableObjects/World/NPC", order = 1)]
public class NpcWorldSO : WorldSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("애니메이션")]
    [SerializeField, CsvIgnore] protected RuntimeAnimatorController _animController;

    [Header("대사")]
    [SerializeField, CsvIgnore] protected string[] _dialog;

    [Header("속성")]
    [SerializeField] protected Vector2 _initPosition;
    [SerializeField, CsvIgnore] private float _moveSpeed;
    [SerializeField, CsvIgnore] private ENpcMoveType _eNpcMoveType;
    [SerializeField] private string _sfxId_footStepSound;
    [SerializeField] private string _sfxId_buzzing;
    // NPC나 상점 / 맵에 종속된 컨텐츠의 경우, Map SO를 만들던지 해서 관리하는게 편하다고 판단.
    // NPC가 존재할 씬 ID?   X
    // NPC가 이동하는 포인트 좌표?    O
    // NPC가 이동하는 속도           일단은 1로 고정
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public RuntimeAnimatorController AnimController => _animController;
    public string[] Dialog => _dialog;
    public float MoveSpeed => _moveSpeed;
    public Vector2 InitPosition => _initPosition;
    public ENpcMoveType NpcMoveType => _eNpcMoveType;
    public string FootStepSoundId => _sfxId_footStepSound;
    public string BuzzingSoundId => _sfxId_buzzing;

    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_type != EType.NpcWorld) return false;
        if (_animController == null) return false;
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
