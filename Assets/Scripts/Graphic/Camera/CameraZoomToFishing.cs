using System.Collections;
using UnityEngine;

/// <summary>
/// 낚시할 시 카메라 확대
/// </summary>
public class CameraZoomToFishing : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("줌 설정")]
    [SerializeField] private float _smooth = 1f; // 높을수록 줌 속도가 빨라짐
    [SerializeField] private float _outRatio = 1f;
    [SerializeField] private float _inRatio = 0.5f;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Camera _cam;
    private Coroutine _fishingCo;
    private float _targetRatio;
    private float _curRatio;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    // 매 프레임 타겟 크기로 보간한다.
    public override void ExecuteFrame()
    {
        if (!TryGetEssentials())
        {
            return;
        }
        // 타겟으로 다가가기
        ScreenData screen = DataManager.Ins.Screen;
        if (!Mathf.Approximately(_curRatio, _targetRatio))
        {
            float ratio = Mathf.Lerp(_curRatio, _targetRatio, _smooth * Time.deltaTime);
            UCamera.SetCameraAspectRatio(_cam, screen.Width, screen.Height, ratio);
            _curRatio = ratio;
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void ZoomIn() => _targetRatio = _inRatio;
    private void ZoomOut() => _targetRatio = _outRatio;

    private IEnumerator CoFishing(float duration)
    {
        ZoomIn();
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        ZoomOut();
        _fishingCo = null;
    }

    private bool TryGetEssentials()
    {
        if (_cam == null)
        {
            _cam = UCamera.MainCamera;
            if (_cam == null)
            {
                UDebug.PrintOnce($"사용할 카메라가 존재하지 않습니다.", LogType.Warning);
                return false;
            }
        }
        return true;
    }

    private void PlayerFishingHandle(OnPlayerFishing ctx)
    {
        TryStopCo();
        _fishingCo = StartCoroutine(CoFishing(ctx.duration));
    }

    private void PlayerCanceledHandle(OnPlayerCanceled ctx)
    {
        if (TryStopCo())
        {
            ZoomOut();
        }
    }

    private bool TryStopCo()
    {
        if (_fishingCo != null)
        {
            StopCoroutine(_fishingCo);
            _fishingCo = null;
            return true;
        }
        return false;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        _curRatio = _outRatio;
        _targetRatio = _outRatio;
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus<OnPlayerFishing>.Subscribe(PlayerFishingHandle);
        EventBus<OnPlayerCanceled>.Subscribe(PlayerCanceledHandle);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus<OnPlayerFishing>.Unsubscribe(PlayerFishingHandle);
        EventBus<OnPlayerCanceled>.Unsubscribe(PlayerCanceledHandle);
        TryStopCo();
    }
    #endregion
}
