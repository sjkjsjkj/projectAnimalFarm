using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 핵심 오브젝트에 대한 접근과 씬 로드를 지원합니다.
/// </summary>
public class GameManager : GlobalSingleton<GameManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private static Transform _uiRoot;
    private static Transform _objectRoot;
    private static Transform _enableObjectRoot;
    private static Transform _disableObjectRoot;
    private bool _isInitialized = false;
    private EScene _curScene;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public EScene Scene => _curScene;

    public static Transform UIRoot => RootProvider(_uiRoot, K.NAME_UI_ROOT);
    public static Transform ObjectRoot => RootProvider(_objectRoot, K.NAME_OBJECT_ROOT);
    public static Transform EnableObjectRoot => RootProvider(_enableObjectRoot, K.NAME_ENABLE_OBJECT_ROOT);
    public static Transform DisableObjectRoot => RootProvider(_disableObjectRoot, K.NAME_DISABLE_OBJECT_ROOT);

    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        // 생성 및 초기화
        _curScene = (EScene)SceneManager.GetActiveScene().buildIndex;
        _isInitialized = true;
    }

    /// <summary>
    /// 해당 씬을 동기 로드합니다.
    /// 동일한 이름을 가지는 씬도 있을 수 있기 때문에 표준적으로는 인덱스 사용이 권장됩니다.
    /// </summary>
    /// <param name="index">씬 인덱스</param>
    [Obsolete("비동기 씬 로드를 권장합니다.")]
    public void LoadScene(int index)
    {
        if (!IsValidScene(index))
        {
            return;
        }
        string scenePath = SceneUtility.GetScenePathByBuildIndex(index);
        LoadScene(scenePath); // 경로를 넣어도 씬 매니저에서 알아서 해준다.
    }

    /// <summary>
    /// 해당 씬을 동기 로드합니다.
    /// </summary>
    /// <param name="name">씬 이름</param>
    [Obsolete("비동기 씬 로드를 권장합니다.")]
    public void LoadScene(string name)
    {
        if (!IsValidScene(name))
        {
            return;
        }
        PreProcessing(_curScene, name);
        SceneManager.LoadScene(name, LoadSceneMode.Single);
        PostProcessing(_curScene, name);
    }

    /// <summary>
    /// 해당 씬을 비동기 로드합니다.
    /// 동일한 이름을 가지는 씬도 있을 수 있기 때문에 표준적으로는 인덱스 사용이 권장됩니다.
    /// </summary>
    /// <param name="index">씬 인덱스</param>
    /// <param name="callback">씬 로드 완료 시 호출할 메서드</param>
    /// <param name="onProgress">씬 로드 진행율을 받을 메서드</param>
    /// <param name="delay">씬 로드 시작 전에 대기할 시간(초)</param>
    /// <param name="loadSceneMode">씬 로드 모드</param>
    public void LoadSceneAsync(
        int index, Action callback, Action<float> onProgress, float delay = 0f,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        if (!IsValidScene(index))
        {
            return;
        }
        string scenePath = SceneUtility.GetScenePathByBuildIndex(index); // 경로를 넣어도 씬 매니저에서 알아서 해준다.
        LoadSceneAsync(scenePath, callback, onProgress, delay, loadSceneMode);
    }

    /// <summary>
    /// 해당 씬을 비동기 로드합니다.
    /// </summary>
    /// <param name="name">씬 이름</param>
    /// <param name="callback">씬 로드 완료 시 호출할 메서드</param>
    /// <param name="onProgress">씬 로드 진행율을 받을 메서드</param>
    /// <param name="delay">씬 로드 시작 전에 대기할 시간(초)</param>
    /// <param name="loadSceneMode">씬 로드 모드</param>
    public void LoadSceneAsync(
        string name, Action callback, Action<float> onProgress, float delay = 0f,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        if (!IsValidScene(name))
        {
            return;
        }
        PreProcessing(_curScene, name);
        StartCoroutine(DoLoadSceneAsync(name, callback, onProgress, delay, loadSceneMode));
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 루트 오브젝트를 안전하게 가져오고 없으면 새로 생성
    private static Transform RootProvider(Transform root, string name)
    {
        if (root == null)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                root = UObject.Create(name).transform;
                UDebug.Print($"{name} 루트를 찾지 못하여 빈 오브젝트를 새로 생성했습니다.");
            }
            else
            {
                root = go.transform;
            }
        }
        return root;
    }

    // 씬 로드 선행처리
    private void PreProcessing(EScene prevScene, string nextScenePath)
    {
        _uiRoot = null;
        _objectRoot = null;
        EScene nextScene = (EScene)SceneUtility.GetBuildIndexByScenePath(nextScenePath);
        OnSceneLoadStart.Publish(prevScene, nextScene);
        _curScene = nextScene;
    }
    // 씬 로드 후처리
    private void PostProcessing(EScene prevScene, string nextScenePath)
    {
        EScene nextScene = (EScene)SceneUtility.GetBuildIndexByScenePath(nextScenePath);
        // 루트 생성
        {
            Transform root = ObjectRoot;
        }
        {
            Transform root = EnableObjectRoot;
        }
        {
            Transform root = DisableObjectRoot;
        }
        {
            Transform root = UIRoot;
        }
        OnSceneLoadEnd.Publish(prevScene, nextScene);
        _curScene = nextScene;
    }

    // 씬 유효성 검증
    private static bool IsValidScene(int index)
    {
        // 존재할 수 없는 인덱스인지 검사
        if (index < 0 || index >= SceneManager.sceneCountInBuildSettings)
        {
            UDebug.Print($"존재하지 않는 씬 인덱스({index})를 호출했습니다.");
            return false;
        }
        return true;
    }
    private static bool IsValidScene(string name)
    {
        // 존재할 수 없는 인덱스인지 검사
        if (name.IsEmpty() || !Application.CanStreamedLevelBeLoaded(name))
        {
            UDebug.Print($"존재하지 않는 씬 이름({name})을 호출했습니다.");
            return false;
        }
        return true;
    }

    // 비동기 코루틴
    private IEnumerator DoLoadSceneAsync(
        string name, Action callback, Action<float> onProgress, float delay, LoadSceneMode loadSceneMode)
    {
        if (delay > 0f)
        {
            yield return UCoroutine.GetWait(delay);
        }
        // 유니티 기본 로드 함수 (비동기 대기)
        var asyncOperation = SceneManager.LoadSceneAsync(name, loadSceneMode);
        // 유니티 비동기 씬 로드 유틸리티
        yield return UCoroutine.WaitAsyncOperation(asyncOperation, onProgress);
        // 씬 로드 완료 → 콜백 호출
        callback?.Invoke();
        PostProcessing(_curScene, name);
    }
    #endregion
}
