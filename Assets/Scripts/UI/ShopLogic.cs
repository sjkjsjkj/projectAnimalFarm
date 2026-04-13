using UnityEngine;

/// <summary>
/// 상점의 거래 판정 틀을 담당합니다.
/// 현재는 실제 인벤토리, 골드, 아이템 데이터 연결 전이므로 테스트용 로직을 사용합니다.
/// </summary>
public class ShopLogic
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 접근자 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private const int MIN_SLOT_COUNT = 1;

    /// <summary>
    /// 현재 상점이 관리하는 슬롯 수입니다.
    /// </summary>
    private readonly int _slotCount;

    /// <summary>
    /// 테스트용 로직 사용 여부입니다.
    /// </summary>
    private readonly bool _useTestLogic;
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 슬롯 번호가 유효한지 검사합니다.
    /// </summary>
    /// <param name="slotIndex">검사할 슬롯 번호</param>
    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < _slotCount;
    }
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 상점 로직을 생성합니다.
    /// </summary>
    /// <param name="slotCount">관리할 슬롯 수</param>
    /// <param name="useTestLogic">테스트용 로직 사용 여부</param>
    public ShopLogic(int slotCount, bool useTestLogic)
    {
        _slotCount = Mathf.Max(MIN_SLOT_COUNT, slotCount);
        _useTestLogic = useTestLogic;
    }

    /// <summary>
    /// 구매 가능 여부를 검사하고 결과를 반환합니다.
    /// </summary>
    /// <param name="slotIndex">슬롯 번호</param>
    /// <param name="message">피드백 메시지</param>
    /// <returns>성공 여부</returns>
    public bool TryBuy(int slotIndex, out string message)
    {
        if (IsValidSlotIndex(slotIndex) == false)
        {
            message = "잘못된 구매 슬롯입니다.";
            return false;
        }

        if (_useTestLogic)
        {
            if (slotIndex % 2 == 0)
            {
                message = $"테스트 구매 성공 : Slot {slotIndex + 1}";
                return true;
            }

            message = $"테스트 구매 실패 : Slot {slotIndex + 1}";
            return false;
        }

        message = $"구매 요청 성공 : Slot {slotIndex + 1}";
        return true;
    }

    /// <summary>
    /// 판매 가능 여부를 검사하고 결과를 반환합니다.
    /// </summary>
    /// <param name="slotIndex">슬롯 번호</param>
    /// <param name="message">피드백 메시지</param>
    /// <returns>성공 여부</returns>
    public bool TrySell(int slotIndex, out string message)
    {
        if (IsValidSlotIndex(slotIndex) == false)
        {
            message = "잘못된 판매 슬롯입니다.";
            return false;
        }

        if (_useTestLogic)
        {
            if (slotIndex % 2 == 1)
            {
                message = $"테스트 판매 성공 : Slot {slotIndex + 1}";
                return true;
            }

            message = $"테스트 판매 실패 : Slot {slotIndex + 1}";
            return false;
        }

        message = $"판매 요청 성공 : Slot {slotIndex + 1}";
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    #endregion
}
