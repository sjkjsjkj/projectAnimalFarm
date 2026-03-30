using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryContextMenu : MonoBehaviour
{
    public Button useButton;
    public Button trashButton;
    public Button cancelButton;

    private InventorySlot targetSlot;
    private bool isReady = false; // 이번 프레임 클릭 무시용

    public void Open(InventorySlot slot, Vector2 screenPos)
    {
        targetSlot = slot;

        RectTransform rt = GetComponent<RectTransform>();
        rt.position = screenPos;

        useButton.onClick.AddListener(OnUse);
        trashButton.onClick.AddListener(OnTrash);
        cancelButton.onClick.AddListener(OnCancel);
    }

    void Update()
    {
        // 열린 첫 프레임은 무시, 다음 프레임부터 outside 클릭 감지
        if (!isReady)
        {
            isReady = true;
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // 버튼 위 클릭이면 무시 (버튼이 처리하게 둠)
            if (EventSystem.current.IsPointerOverGameObject()) return;
            Destroy(gameObject);
        }
    }

    void OnUse()
    {
        Debug.Log($"{targetSlot.CurrentItem.itemName} 사용!");
        InventoryManager.Instance.RemoveItem(targetSlot);
        Destroy(gameObject);
    }

    void OnTrash()
    {
        Debug.Log($"{targetSlot.CurrentItem.itemName} 버림!");
        InventoryManager.Instance.RemoveItem(targetSlot);
        Destroy(gameObject);
    }

    void OnCancel()
    {
        Destroy(gameObject);
    }
}
