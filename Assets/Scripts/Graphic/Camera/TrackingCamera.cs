using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class NewFrame : Frameable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("플레이어")]
    [SerializeField] private Transform _player;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        if (_player == null)
        {
            // 새로운 동물 탐색
            _player = UObject.FindComponent<AnimalObject>("BreedingArea").transform;
            AnimalObject[] search = FindObjectsByType<AnimalObject>(FindObjectsSortMode.None);
            int length = search.Length;
            int rand = Random.Range(0, length);
            _player = search[rand].transform;
            if (_player == null)
            {
                return;
            }
        }
        // 방어 코드로 거르기
        Camera cam = UCamera.MainCamera;
        if (cam == null)
        {
            return;
        }
        // 카메라 추적
        Vector3 pos = _player.transform.position;
        pos.z = -10;
        cam.transform.position = pos;
        // 새로운 동물로 탐색
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _player = null;
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
