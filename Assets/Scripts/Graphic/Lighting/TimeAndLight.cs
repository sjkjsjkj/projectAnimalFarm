using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 시간에 따라 빛의 색상과 강도 조절
/// </summary>
public class TimeAndLight : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("글로벌 라이트")]
    [SerializeField] private Light2D _globalLight;
    [SerializeField] private Gradient _gradient; // 색상
    [SerializeField] private AnimationCurve _curve; // 밝기

    [Header("시간 비율")] // 하루에 10분
    [SerializeField, Range(0f, 1f)] private float _startTime = 1f; // 낮 = 1
    [SerializeField, Range(5f, 1800f)] private float _dayDurationSeconds = 600f;
    [SerializeField, Range(0f, 1f)] private float _dayCondition = 0.5f;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private float _curNormalize = 0f;
    private static bool _isDay = true;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public static bool IsDay => _isDay;
    public static bool IsNight => !_isDay;

    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        UpdateLight();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void UpdateLight()
    {
        EScene curScene = GameManager.Ins.Scene;
        switch (curScene)
        {
            case EScene.Title:
                SetLight(1f, Color.white);
                break;
            case EScene.Main:
                LightFlow();
                break;
            case EScene.Forest:
                LightFlow();
                break;
            case EScene.Cave:
                SetLight(0.1f, new Color(0.5f, 0.5f, 0.75f)); // 어두운 푸른색
                break;
            default:
                SetLight(1f, Color.white);
                return;
        }
        TimeProgress();
    }
    private void TimeProgress()
    {
        // 하루의 시간 비율
        float ratio = Time.time / _dayDurationSeconds + _startTime;
        _curNormalize = Mathf.PingPong(ratio, 1f);
        // 낮과 밤 구분
        bool curIsDay = _curNormalize > _dayCondition; // 0.5보다 작으면 낮, 크면 밤
        if(_isDay != curIsDay)
        {
            _isDay = curIsDay;
            OnTimeChanged.Publish(_isDay);
            UDebug.Print($"시간이 {(_isDay ? "낮" : "밤")}으로 변화했습니다.");
        }
    }

    // 시간의 흐름에 따른 빛 업데이트
    private void LightFlow()
    {
        if (_globalLight == null)
        {
            return;
        }
        float intensity = _curve.Evaluate(_curNormalize); // 현재 강도
        Color color = _gradient.Evaluate(_curNormalize); // 현재 색상
        SetLight(intensity, color);
    }

    // 색상과 강도 설정
    private void SetLight(float intensity, Color color)
    {
        _globalLight.color = color;
        _globalLight.intensity = intensity;
    }
    #endregion

    private new void Awake()
    {
        _curNormalize = Mathf.PingPong(_startTime, 1f);
        _isDay = _curNormalize > _dayCondition;
    }
}
