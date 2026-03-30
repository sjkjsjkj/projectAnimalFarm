using UnityEngine;

/// <summary>
/// 본 프로젝트의 표준 모노비헤이비어입니다.
/// </summary>
public abstract class BaseMono : MonoBehaviour
{
    [HideInInspector]
    [SerializeField] private string _uniqueId; // UUID

    public string UniqueId => _uniqueId;

    // 스크립트 로드 or 인스펙터 값 변경 시 호출
    protected virtual void OnValidate()
    {
        if (_uniqueId.IsEmpty())
        {
            NewGuid();
        }
    }

    // 복사 시 UUID 복제 방지
    protected virtual void Reset()
    {
        NewGuid();
    }

    // 고유 UUID 부여
    private void NewGuid()
    {
        _uniqueId = System.Guid.NewGuid().ToString();
    }
}
