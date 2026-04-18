using System;
using UnityEngine;

/// <summary>
/// 인벤토리 UI 테스트용 아이템 데이터입니다.
/// 
/// [TEST ONLY]
/// - 실제 ItemSO가 아직 없을 때 테스트하기 위한 임시 구조체입니다.
/// - 나중에 실제 아이템 데이터 시스템이 연결되면 삭제해도 됩니다.
/// </summary>
[Serializable]
public struct InventoryDebugItemData
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("테스트 아이템 정보")]
    [SerializeField] private string _name;
    [SerializeField] private Sprite _icon;
    [SerializeField] private int _maxStack;
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    public string Name => _name;
    public Sprite Icon => _icon;
    public int MaxStack => Mathf.Max(1, _maxStack);
    public bool IsValid => string.IsNullOrEmpty(_name) == false && _icon != null;
    #endregion
}
