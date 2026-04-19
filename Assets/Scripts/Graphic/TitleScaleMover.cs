using UnityEngine;

/// <summary>
/// 배경 sin보간
/// </summary>
public class TitleScaleMover : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private float _startScale = 1f;
    [SerializeField] private float _endScale = 1.2f;
    [SerializeField] private float _speed = 2f;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        float t = (Mathf.Sin(Time.time * _speed) + 1f) / 2f;
        transform.localScale = Vector3.one * Mathf.Lerp(_startScale, _endScale, t);
    }
    #endregion
}
