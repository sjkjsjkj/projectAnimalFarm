using UnityEngine;

/// <summary>
/// 현재 씬에서 카메라와 플레이어를 자동으로 탐색하여 카메라가 플레이어를 추적
/// </summary>
public class CameraFollowThePlayer : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("카메라 설정")]
    [SerializeField] private Transform _playerTr;
    [SerializeField] private Transform _cameraTr;
    [SerializeField] private Camera _cam;
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
    // Clamp한 목표 좌표를 반환합니다.
    private Vector3 BuildDesiredPos()
    {
        Vector3 desiredPos = _playerTr.position + _offset;
        if ((int)GameManager.Ins.Scene > (int)EScene.Cave)
        {
            return desiredPos;
        }
        var tile = TileManager.Ins;
        if (tile == null || tile.Tile == null)
        {
            return desiredPos;
        }
        // 맵 테두리
        float startX, endX, startY, endY;
        (startX, endX, startY, endY) = tile.Tile.MapOutline();
        // 카메라 크기 적용
        float camW = _cam.orthographicSize * _cam.aspect;
        float camH = _cam.orthographicSize;
        startX += (camW + 1f);
        endX -= (camW + 1f);
        startY += (camH + 1f);
        endY -= (camH + 1f);

        // 클램프 적용
        desiredPos.x = Mathf.Clamp(desiredPos.x, startX, endX);
        desiredPos.y = Mathf.Clamp(desiredPos.y, startY, endY);

        return desiredPos;
    }

    private void FollowPlayer()
    {
        Vector3 cameraPos = _cameraTr.position;
        Vector3 desiredPos = BuildDesiredPos();
        Vector3 nextPos;
        // 거리가 멀 경우 스냅
        if ((desiredPos - cameraPos).sqrMagnitude > 15f)
        {
            nextPos = desiredPos;
        }
        // 화면 이내로 추정될 경우 보간
        else
        {
            nextPos = Vector3.SmoothDamp(cameraPos, desiredPos, ref _velocity, _smoothTime);
        }
        _cameraTr.position = nextPos;
    }

    private bool TryGetEssentials()
    {
        if (_playerTr == null)
        {
            var component = GameObject.FindAnyObjectByType<PlayerController>();
            if (component == null)
            {
                UDebug.PrintOnce($"카메라가 추적할 플레이어가 존재하지 않습니다.", LogType.Warning);
                return false;
            }
            _playerTr = component.transform;
        }
        if (_cam == null)
        {
            _cam = UCamera.MainCamera;
            if (_cam == null)
            {
                UDebug.PrintOnce($"사용할 카메라가 존재하지 않습니다.", LogType.Warning);
                return false;
            }
            _cameraTr = _cam.transform;
        }
        return true;
    }
    #endregion
}
