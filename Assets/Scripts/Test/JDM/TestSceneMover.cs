using UnityEngine;

/// <summary>
/// 대괄호로 씬 한칸씩 이동하기
/// </summary>
public class TestSceneMover : Frameable
{
    private int _curSceneIndex = 0;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.First;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        // 디버그 키 안내
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            UDebug.Print(
                $"\n우측 씬 로드 : 오른쪽 대괄호" +
                $"\n왼측 씬 로드 : 왼쪽 대괄호");
        }

        // 씬 전환
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            _curSceneIndex++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            _curSceneIndex--;
        }
        else
        {
            return; // 키 입력 없음
        }
        UDebug.Print($"이동할 씬 인덱스 : {_curSceneIndex}");
        GameManager.Ins.LoadSceneAsync(_curSceneIndex, SceneCallbackHandle, SceneProgressHandle);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void SceneProgressHandle(float progress)
    {
        UDebug.Print($"씬 프로세스 핸들을 실행합니다. ({progress}%)");
    }
    private void SceneCallbackHandle()
    {
        UDebug.Print($"씬 콜백 핸들을 실행합니다.");
    }
    private void SceneLoadEndHandle(OnSceneLoadEnd ctx)
    {
        _curSceneIndex = (int)ctx.nextScene;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus<OnSceneLoadEnd>.Subscribe(SceneLoadEndHandle);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus<OnSceneLoadEnd>.Unsubscribe(SceneLoadEndHandle);
    }
    #endregion
}
