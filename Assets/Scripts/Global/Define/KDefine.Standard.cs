#pragma warning disable IDE1006

public static partial class K
{
    // 카메라 / UI 동작 방식 정의
    public static readonly int SCREEN_WIDTH = 1280;
    public static readonly int SCREEN_HEIGHT = 720;
    // 카메라 / 이동 속도 / 충돌 크기의 기준점
    public static readonly float UNIT_SCALE = 0.01f;
    // 바닥 거리 통일 → 모든 UI가 카메라와 두는 거리 통일
    public static readonly float DEFAULT_PLANE_DISTANCE = 5f;
    // 매니저 우선순위 크기
    public static readonly int MANAGER_PRIORITY_SIZE = 11;
    public static readonly int MANAGER_FRAMEABLE_SIZE = 10;
}
