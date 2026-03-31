/*using UnityEngine;

/// <summary>
/// 리소스 폴더의 특정 경로에서 프리펩을 로드 및 전역 적용
/// </summary>
public class GlobalPrefabLoader : BaseMono
{
    private bool _isInitialized = false;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void Initialize()
    {
        if (_isInitialized) return;

        LoadPrefabs();
        _isInitialized = true;
        UDebug.Print($"[GlobalPrefabLoader] 글로벌 프리팹 로드 및 DontDestroy 설정 완료.");
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 모든 프리펩 로드
    private void LoadPrefabs()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>(K.NAME_GLOBAL_PREFAB_ROOT);
        if (prefabs == null || prefabs.Length <= 0)
        {
            UDebug.Print($"경로({K.NAME_GLOBAL_PREFAB_ROOT})에서 로드할 프리팹이 존재하지 않습니다.");
            return;
        }
        // 
        // 2. 프리팹들을 하나씩 순회하며 런타임 인스턴스 생성
        int length = prefabs.Length;
        for (int i = 0; i < length; i++)
        {
            SpawnPrefabUnderGlobalRoot(prefabs[i]);
        }
    }

    /// <summary>
    /// 단일 프리팹을 스폰하고 글로벌 부모 하위로 정돈합니다.
    /// </summary>
    private void SpawnPrefabUnderGlobalRoot(GameObject prefab)
    {
        if (prefab == null) return;

        // UObject.Spawn (프로젝트 표준) 혹은 기본 Instantiate 사용
        // [핵심 동기화] 생성 시점에 캐싱해 둔 _parent를 넘겨 부모-자식 계층 구조를 즉시 확립
        GameObject instance = Instantiate(prefab, _parent);

        // 에디터 하이어라키 정돈을 위해 '(Clone)' 이름 제거
        instance.name = prefab.name;

        UDebug.Print($"[GlobalPrefabLoader] 글로벌 객체 스폰 완료: {instance.name}");
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Awake()
    {
        // 1. 하이어라키에서 이미 존재하는 '@GlobalPrefab' 루트를 찾습니다.
        _parent = GameObject.Find(K.NAME_GLOBAL_PREFAB_ROOT)?.transform;

        // 2. 만약 존재하지 않는다면 (최초 실행 시), 스스로 생성합니다.
        if (_parent == null)
        {
            // [치명적 팩트] UDebug.Print 이전에 호출되면 안 되므로 안전하게 Awake 후반부에 배치
            _parent = new GameObject(K.NAME_GLOBAL_PREFAB_ROOT).transform;

            // 3. 스스로 생성한 하이어라키 루트에 DontDestroyOnLoad를 걸어 씬 전환에도 유지시킵니다.
            // 자식들은 DontDestroy 속성이 자동으로 상속되므로 부모에만 걸어주면 됩니다.
            DontDestroyOnLoad(_parent.gameObject);

            UDebug.Print($"[GlobalPrefabLoader] 글로벌 하이어라키 루트(@GlobalPrefab) 생성 및 DontDestroyOnLoad 설정.");
        }
    }
    #endregion
}
*/
