using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 플레이어 추적 조명 (Sin Lerp)
/// </summary>
[RequireComponent(typeof(Light2D))]
public class PlayerFollowLight : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("추적 대상")]
    [SerializeField] private Transform _playerTr;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 0.4f, 0f);

    [Header("밝기 변화")]
    [SerializeField] private float _minIntensity = 0.95f;
    [SerializeField] private float _maxIntensity = 1.05f;
    [SerializeField] private float _intensitySpeed = 2f;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Light2D _light;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        if(_playerTr == null)
        {
            return;
        }
        transform.position = _playerTr.position + _offset;
        float t = Mathf.Sin((Time.time * _intensitySpeed) + 1f) / 2f; // -1 ~ 1 → 0 ~ 1
        _light.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, t);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private new void Awake()
    {
        _light = GetComponent<Light2D>();
    }
    private void Start()
    {
        if (UDebug.IsNull(_playerTr))
        {
            return;
        }
        transform.position = _playerTr.position + _offset; // 우선 동기화
    }
    #endregion
}
