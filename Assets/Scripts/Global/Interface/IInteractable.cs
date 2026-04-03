using UnityEngine;

/// <summary>
/// 인터랙트가 가능한 월드 객체들이 가져야 하는 필수 인터페이스
/// </summary>
public interface IInteractable
{
    bool Interact(GameObject player); // 상호작용 실패 시 False 반환
}
