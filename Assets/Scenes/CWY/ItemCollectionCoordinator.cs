using UnityEngine;

/// <summary>
/// 아이템 획득 시 인벤토리와 도감을 동시에 갱신하는 중간 관리자
/// 다른 시스템은 이 클래스만 호출하면 된다.
/// </summary>
public class ItemCollectionCoordinator : MonoBehaviour
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [SerializeField] private CContentTestInventory _inventory;
    [SerializeField] private PictorialBookSystem _pictorialBook;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    public bool TryCollectItem(string itemId, int amount)
    {
        if (_inventory == null)
        {
            Debug.LogError("[ItemCollectionCoordinator] Inventory가 연결되지 않았습니다.");
            return false;
        }

        if (_pictorialBook == null)
        {
            Debug.LogError("[ItemCollectionCoordinator] PictorialBook이 연결되지 않았습니다.");
            return false;
        }

        bool added = _inventory.TryAddItem(itemId, amount);

        if (!added)
        {
            return false;
        }

        // 처음 얻었으면 도감 자동 해금
        _pictorialBook.TryDiscover(itemId);

        return true;
    }
    #endregion
}
