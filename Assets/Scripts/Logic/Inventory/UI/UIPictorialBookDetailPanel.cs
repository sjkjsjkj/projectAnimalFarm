using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 도감 상세 정보 패널.
/// 
/// 역할
/// 1. 도감 슬롯을 클릭했을 때 상세 정보를 표시한다.
/// 2. ItemSO / SheetItemDatabase를 함께 참조해서 정보를 최대한 보강한다.
/// 3. 이름 / 설명 / 등급 / 판매금액 / 구매금액 / ID / 아이콘을 보여준다.
/// 4. 상세 패널이 열린 상태에서는 EscManager에 등록되어 ESC 입력 시
///    도감 전체가 아니라 상세 패널만 먼저 닫히도록 처리한다.
/// </summary>
public class UIPictorialBookDetailPanel : BaseMono, IEscClosable
{
    [Header("루트")]
    [SerializeField] private GameObject _rootObject;

    [Header("UI 참조")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _categoryText;
    [SerializeField] private TMP_Text _rarityText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _sellPriceText;
    [SerializeField] private TMP_Text _buyPriceText;
    [SerializeField] private TMP_Text _idText;

    [Header("상세 데이터 보강용")]
    [SerializeField] private SheetItemDatabase _sheetDatabase;

    [Header("옵션")]
    [SerializeField] private bool _hideOnAwake = true;
    [SerializeField] private bool _useLog = false;

    [Header("표시 문자열")]
    [SerializeField] private string _emptyValueText = "-";

    private string _currentItemId = string.Empty;
    private bool _isEscRegistered = false;

    protected override void Awake()
    {
        base.Awake();

        if (_rootObject == null)
        {
            _rootObject = gameObject;
        }

        if (_hideOnAwake)
        {
            Hide();
        }
    }

    /// <summary>
    /// 도감 엔트리를 기준으로 상세 패널 표시
    /// </summary>
    public void Show(PictorialBookEntry entry)
    {
        if (entry == null)
        {
            Hide();
            return;
        }

        PictorialBookDetailData detailData = BuildDetailData(entry);
        Apply(detailData);
        _currentItemId = NormalizeText(detailData.itemId);
        SetVisible(true);
        RegisterEscClose();
    }

    /// <summary>
    /// 외부 버튼에서 닫기용으로 바로 연결 가능
    /// </summary>
    public void Hide()
    {
        HideInternal(true);
    }

    /// <summary>
    /// EscManager가 호출하는 닫기.
    /// EscManager는 CloseUi 호출 전에 이미 스택에서 이 패널을 제거하므로
    /// 여기서는 추가 Exit 호출 없이 내부 상태만 정리한다.
    /// </summary>
    public void CloseUi()
    {
        HideInternal(false);
    }

    /// <summary>
    /// 현재 패널이 열려있는지 확인
    /// </summary>
    public bool IsShowing()
    {
        return _rootObject != null && _rootObject.activeSelf;
    }

    /// <summary>
    /// 현재 표시 중인 itemId 반환
    /// </summary>
    public string CurrentItemId => _currentItemId;

    private void HideInternal(bool unregisterEsc)
    {
        _currentItemId = string.Empty;

        if (unregisterEsc)
        {
            UnregisterEscClose();
        }
        else
        {
            _isEscRegistered = false;
        }

        SetVisible(false);
    }

    private void RegisterEscClose()
    {
        if (_isEscRegistered)
        {
            return;
        }

        if (EscManager.Ins == null)
        {
            return;
        }

        EscManager.Ins.Enter(this);
        _isEscRegistered = true;
    }

    private void UnregisterEscClose()
    {
        if (_isEscRegistered == false)
        {
            return;
        }

        if (EscManager.Ins != null)
        {
            EscManager.Ins.Exit(this);
        }

        _isEscRegistered = false;
    }

    private PictorialBookDetailData BuildDetailData(PictorialBookEntry entry)
    {
        PictorialBookDetailData data = new PictorialBookDetailData();

        if (entry != null)
        {
            data.itemId = NormalizeText(entry.itemId);
            data.name = NormalizeText(entry.displayName);
            data.category = NormalizeText(entry.category);
            data.icon = entry.icon;
        }

        ItemSO itemSO = TryGetItemSO(data.itemId);
        if (itemSO != null)
        {
            if (string.IsNullOrWhiteSpace(data.name))
            {
                data.name = NormalizeText(itemSO.Name);
            }

            if (string.IsNullOrWhiteSpace(data.description))
            {
                data.description = NormalizeText(itemSO.Description);
            }

            if (string.IsNullOrWhiteSpace(data.rarity))
            {
                data.rarity = itemSO.Rarity.ToString();
            }

            if (data.sellPrice < 0)
            {
                data.sellPrice = itemSO.SellPrice;
            }

            if (data.buyPrice < 0)
            {
                data.buyPrice = itemSO.BuyPrice;
            }

            if (data.icon == null)
            {
                data.icon = itemSO.Image;
            }
        }

        SheetItemRow row = TryGetSheetRow(data.itemId);
        if (row != null)
        {
            if (string.IsNullOrWhiteSpace(row.name) == false)
            {
                data.name = NormalizeText(row.name);
            }

            if (string.IsNullOrWhiteSpace(row.category) == false)
            {
                data.category = NormalizeText(row.category);
            }

            if (string.IsNullOrWhiteSpace(row.rarity) == false)
            {
                data.rarity = NormalizeText(row.rarity);
            }

            if (string.IsNullOrWhiteSpace(row.description) == false)
            {
                data.description = NormalizeText(row.description);
            }

            if (row.sellPrice >= 0)
            {
                data.sellPrice = row.sellPrice;
            }

            if (row.buyPrice >= 0)
            {
                data.buyPrice = row.buyPrice;
            }
        }

        return data;
    }

    private void Apply(PictorialBookDetailData data)
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = data.icon;
            _iconImage.enabled = data.icon != null;
            _iconImage.color = Color.white;
        }

        if (_nameText != null)
        {
            _nameText.text = GetValueOrFallback(data.name);
        }

        if (_categoryText != null)
        {
            _categoryText.text = GetValueOrFallback(data.category);
        }

        if (_rarityText != null)
        {
            _rarityText.text = GetValueOrFallback(data.rarity);
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = GetValueOrFallback(data.description);
        }

        if (_sellPriceText != null)
        {
            _sellPriceText.text = data.sellPrice >= 0 ? data.sellPrice.ToString() : _emptyValueText;
        }

        if (_buyPriceText != null)
        {
            _buyPriceText.text = data.buyPrice >= 0 ? data.buyPrice.ToString() : _emptyValueText;
        }

        if (_idText != null)
        {
            _idText.text = GetValueOrFallback(data.itemId);
        }

        if (_useLog)
        {
            Debug.Log($"[UIPictorialBookDetailPanel] 상세 정보 표시: {data.itemId}");
        }
    }

    private ItemSO TryGetItemSO(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        try
        {
            if (DatabaseManager.Ins == null)
            {
                return null;
            }

            return DatabaseManager.Ins.Item(itemId);
        }
        catch (Exception ex)
        {
            if (_useLog)
            {
                Debug.LogWarning($"[UIPictorialBookDetailPanel] ItemSO 조회 중 예외 발생: {ex.Message}");
            }

            return null;
        }
    }

    private SheetItemRow TryGetSheetRow(string itemId)
    {
        if (_sheetDatabase == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        return _sheetDatabase.GetRowOrNull(itemId);
    }

    private void SetVisible(bool visible)
    {
        if (_rootObject != null)
        {
            _rootObject.SetActive(visible);
            return;
        }

        gameObject.SetActive(visible);
    }

    private string GetValueOrFallback(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? _emptyValueText : value.Trim();
    }

    private string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim();
    }

    private void Start()
    {
        StartCoroutine(CoFindSheetItemDatabase());
    }

    private IEnumerator CoFindSheetItemDatabase()
    {
        while (_sheetDatabase == null)
        {
            _sheetDatabase = FindAnyObjectByType<SheetItemDatabase>();
            yield return null;
        }
    }

    private void OnDisable()
    {
        UnregisterEscClose();
    }

    [Serializable]
    private class PictorialBookDetailData
    {
        public string itemId;
        public string name;
        public string category;
        public string rarity;
        public string description;
        public int sellPrice = -1;
        public int buyPrice = -1;
        public Sprite icon;
    }
}
