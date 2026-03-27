using UnityEngine;

/// <summary>
/// 테스트용 스크립트
/// 특정 키를 누르면 아이템을 획득하게 해서 인벤토리 / 도감 연동 확인
/// </summary>
public class ItemGrantTester : MonoBehaviour
{
    [SerializeField] private ItemCollectionCoordinator _coordinator;

    private void Update()
    {
        // 1키: 사과 획득
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _coordinator.TryCollectItem("Apple_0", 1);
        }

        // 2키: 붕어 획득
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _coordinator.TryCollectItem("River_0", 1);
        }

        // 3키: 검은 젓소 획득
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _coordinator.TryCollectItem("Baby Cow Black_0", 1);
        }
    }
}
