using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [Header("UI References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI descText;

    private RectTransform rectTransform;
    private Canvas rootCanvas;

    void Awake()
    {
        Instance = this;
        rectTransform = tooltipPanel.GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (tooltipPanel.activeSelf)
            MoveToMouse();
    }

    public void Show(ItemData item)
    {
        nameText.text = item.itemName;
        typeText.text = $"[{item.itemType}]";
        descText.text = item.description;
        tooltipPanel.SetActive(true);
        MoveToMouse();
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }

    void MoveToMouse()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out pos
        );

        // 화면 밖으로 안 나가게 오프셋 조정
        float offsetX = 15f;
        float offsetY = -15f;

        Vector2 size = rectTransform.sizeDelta;
        float canvasW = rootCanvas.GetComponent<RectTransform>().rect.width;
        float canvasH = rootCanvas.GetComponent<RectTransform>().rect.height;

        if (pos.x + size.x + offsetX > canvasW / 2) offsetX = -size.x - 15f;
        if (pos.y + offsetY - size.y < -canvasH / 2) offsetY = size.y + 15f;

        rectTransform.anchoredPosition = new Vector2(pos.x + offsetX, pos.y + offsetY);
    }
}
