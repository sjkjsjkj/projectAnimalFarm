using UnityEngine;

/// <summary>
/// 현재 씬에서 카메라와 플레이어를 자동으로 탐색하여 카메라가 플레이어를 추적
/// </summary>
public class CameraFollowThePlayer : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("카메라 설정")]
    [SerializeField] private Transform _playerTr = null;
    [SerializeField] private Transform _cameraTr = null;
    [SerializeField] private float _smoothTime = 0.1f; // 낮을수록 목표까지 가는 시간이 빠르다.
    [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10f);
    [SerializeField] private Vector3 _velocity = Vector3.zero;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        if (!TryGetEssentials())
        {
            return;
        }
        FollowPlayer();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void FollowPlayer()
    {
        Vector3 desiredPos = _playerTr.position + _offset;
        Vector3 nextPos = Vector3.SmoothDamp(_cameraTr.position, desiredPos, ref _velocity, _smoothTime);
        _cameraTr.position = nextPos;
    }

    private bool TryGetEssentials()
    {
        if( _playerTr == null)
        {
            var component = GameObject.FindAnyObjectByType<PlayerController>();
            if (component == null)
            {
                UDebug.PrintOnce($"카메라가 추적할 플레이어가 존재하지 않습니다.", LogType.Warning);
                return false;
            }
            _playerTr = component.transform;
        }
        if( _cameraTr == null)
        {
            GameObject go = UObject.Find(K.NAME_MAIN_CAMERA);
            if (go == null)
            {
                UDebug.PrintOnce($"사용할 카메라가 존재하지 않습니다.", LogType.Warning);
                return false;
            }
            _cameraTr = go.transform;
        }
        return true;
    }
    #endregion
}
