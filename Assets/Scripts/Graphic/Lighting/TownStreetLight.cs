using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TownStreetLight : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("거리의 빛")]
    [SerializeField] private Light2D[] _lightList;

    [Header("밝기 변화")]
    [SerializeField] private float _fadeTime = 2f;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private float[] _maxIntensity;
    private Coroutine _fadeCo;
    private float _curRatio;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void EnableAllLight()
    {
        if(_fadeCo != null)
        {
            StopCoroutine(_fadeCo);
        }
        //
        _fadeCo = StartCoroutine(FadeLight(1f));
    }
    private void DisableAllLight()
    {
        if (_fadeCo != null)
        {
            StopCoroutine(_fadeCo);
        }
        //
        _fadeCo = StartCoroutine(FadeLight(0f));
    }
    private IEnumerator FadeLight(float targetRatio)
    {
        float startRatio = _curRatio;
        float timer = 0f;
        while (timer < _fadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / _fadeTime;
            _curRatio = Mathf.Lerp(startRatio, targetRatio, t);
            SetLightIntensity(_curRatio);
            yield return null;
        }
        // 마지막
        _curRatio = targetRatio;
        SetLightIntensity(targetRatio);
    }
    private void SetLightIntensity(float ratio)
    {
        int length = _lightList.Length;
        for (int i = 0; i < length; ++i)
        {
            Light2D light = _lightList[i];
            if(light != null)
            {
                light.intensity = _maxIntensity[i] * ratio;
            }
        }
    }
    private void TimeChangedHandle(OnTimeChanged ctx)
    {
        // 낮
        if (ctx.isDay)
        {
            DisableAllLight();
        }
        // 밤
        else
        {
            EnableAllLight();
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        if (UDebug.IsNull(_lightList)) return;
        // 각 조명의 최대 밝기값 저장
        int length = _lightList.Length;
        _maxIntensity = new float[length];
        for (int i = 0; i < length; ++i)
        {
            Light2D light = _lightList[i];
            _maxIntensity[i] = light.intensity;
        }
        // 낮일 경우
        if (TimeAndLight.IsDay)
        {
            _curRatio = 0f;
        }
        // 밤일 경우
        else
        {
            _curRatio = 1f;
        }
        SetLightIntensity(_curRatio);
    }
    private void OnEnable()
    {
        EventBus<OnTimeChanged>.Subscribe(TimeChangedHandle);
    }
    private void OnDisable()
    {
        EventBus<OnTimeChanged>.Unsubscribe(TimeChangedHandle);
    }
    #endregion
}
