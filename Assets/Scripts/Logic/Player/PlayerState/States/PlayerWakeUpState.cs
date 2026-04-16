using System.Collections;
using UnityEngine;

[System.Serializable]
public class PlayerWakeUpState : IPlayerState
{
    [SerializeField] private Camera _cam;
    [SerializeField] private float _zoomInRatio = 0.3f;
    [SerializeField] private float _zoomOutRatio = 1f;
    [SerializeField] private float _zoomOutDuration = 1.5f;
    [SerializeField] private float _animDuration = 2.107f;

    private MonoBehaviour _runner;
    private float _animTimer;
    private bool _isWaiting;

    private const string WAKEUP_PARAM = "WakeUp";
    private readonly int _hashWakeUp = Animator.StringToHash(WAKEUP_PARAM);

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool Enter(in PlayerContext context)
    {
        _isWaiting = true;
        _runner = UObject.AddComponent<FadeRunner>(context.tr.gameObject); // 코루틴 실행용 컴포넌트
        DataManager.Ins.Player.ChangeState(EPlayerState.WakeUp);
        ZoomIn(); // 카메라
        // 애니메이션
        context.anim.Play(_hashWakeUp, 0, 0f);
        context.anim.speed = 0f;
        return true;
    }

    public bool Frame(in PlayerContext context)
    {
        if (_isWaiting)
        {
            // 씬 로딩중
            if (GameManager.Ins.IsSceneLoading || UFade.IsFading)
            {
                return true;
            }
            // 씬 로딩 완료
            _isWaiting = false;
            context.anim.speed = 1f;
            _animTimer = 0f;
        }
        // 애니메이션 재생 중
        _animTimer += Time.deltaTime;
        if (_animTimer >= _animDuration)
        {
            return false;
        }
        return true;
    }

    public void Exit(in PlayerContext context)
    {
        context.anim.speed = 1f;
        ZoomOut();
    }
    #endregion

    private void ZoomIn()
    {
        if (!TryGetEssentials())
        {
            return;
        }
        // 줌 상태로 시작
        ScreenData screen = DataManager.Ins.Screen;
        UCamera.SetCameraAspectRatio(_cam, screen.Width, screen.Height, _zoomInRatio);
    }

    private void ZoomOut()
    {
        if (!TryGetEssentials())
        {
            return;
        }
        _runner.StartCoroutine(CoZoomOut());
    }

    private IEnumerator CoZoomOut()
    {
        float timer = K.SMALL_DISTANCE;
        while (timer < _zoomOutDuration)
        {
            // 보간 계산
            float t = timer / _zoomOutDuration;
            t = Mathf.Clamp01(t);
            float ratio = Mathf.Lerp(_zoomInRatio, _zoomOutRatio, t);
            // 카메라 설정
            ScreenData screen = DataManager.Ins.Screen;
            UCamera.SetCameraAspectRatio(_cam, screen.Width, screen.Height, ratio);
            // 다음 프레임
            timer += Time.deltaTime;
            yield return null;
        }
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

    private class FadeRunner : MonoBehaviour { }
}
