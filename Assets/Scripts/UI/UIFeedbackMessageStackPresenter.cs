using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 피드백 메시지 요청 이벤트를 구독하여
/// 메시지 아이템 프리팹을 스택 루트에 생성하고 관리하는 프리젠터입니다.
/// </summary>
public class UIFeedbackMessageStackPresenter : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("피드백 메시지 스택 참조")]
    [SerializeField] private RectTransform _stackRoot;

    [Header("생성할 메시지 아이템 프리팹 참조")]
    [SerializeField] private UIFeedbackMessageItemView _messageItemPrefab;

    [Header("표시 연출 설정")]
    [SerializeField] private float _fadeInDuration = 0.15f;
    [SerializeField] private float _fadeOutDuration = 0.2f;

    [Header("최대 메시지 수")]
    [SerializeField] private int _maxMessageCount = 5;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    /// <summary>
    /// 현재 화면에 살아 있는 메시지 아이템 목록입니다.
    /// </summary>
    private readonly List<UIFeedbackMessageItemView> _activeItems = new List<UIFeedbackMessageItemView>();
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 피드백 메시지 요청 이벤트를 받아 메시지 아이템을 생성합니다.
    /// </summary>
    /// <param name="channel">피드백 메시지 요청 데이터</param>
    private void HandleFeedbackMessageRequested(OnFeedbackMessageRequested channel)
    {
        if (!ValidateReferences())
        {
            return;
        }

        RemoveOverflowItems();

        UIFeedbackMessageItemView itemView = Instantiate(_messageItemPrefab, _stackRoot);

        if (itemView == null)
        {
            return;
        }

        itemView.transform.SetAsLastSibling();
        itemView.transform.localScale = Vector3.one;
        itemView.Initialize(channel.message, channel.messageType);

        _activeItems.Add(itemView);
        StartCoroutine(CoShowAndRemove(itemView, channel.duration));
    }

    /// <summary>
    /// 최대 메시지 수를 초과한 경우 가장 오래된 메시지부터 제거합니다.
    /// </summary>
    private void RemoveOverflowItems()
    {
        while (_activeItems.Count >= _maxMessageCount)
        {
            UIFeedbackMessageItemView oldestItem = _activeItems[0];
            _activeItems.RemoveAt(0);

            if (oldestItem != null)
            {
                Destroy(oldestItem.gameObject);
            }
        }
    }

    /// <summary>
    /// 메시지를 페이드 인, 유지, 페이드 아웃 후 제거하는 코루틴입니다.
    /// </summary>
    /// <param name="itemView">대상 메시지 아이템</param>
    /// <param name="duration">표시 유지 시간</param>
    private IEnumerator CoShowAndRemove(UIFeedbackMessageItemView itemView, float duration)
    {
        if (itemView == null)
        {
            yield break;
        }

        yield return CoFadeAlpha(itemView, 0f, 1f, _fadeInDuration);

        float remainTime = Mathf.Max(0f, duration);

        while (remainTime > 0f)
        {
            remainTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        yield return CoFadeAlpha(itemView, 1f, 0f, _fadeOutDuration);

        _activeItems.Remove(itemView);

        if (itemView != null)
        {
            Destroy(itemView.gameObject);
        }
    }

    /// <summary>
    /// 메시지 아이템 알파를 일정 시간 동안 보간합니다.
    /// </summary>
    /// <param name="itemView">대상 메시지 아이템</param>
    /// <param name="startAlpha">시작 알파</param>
    /// <param name="endAlpha">목표 알파</param>
    /// <param name="duration">보간 시간</param>
    private IEnumerator CoFadeAlpha(UIFeedbackMessageItemView itemView, float startAlpha, float endAlpha, float duration)
    {
        if (itemView == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            itemView.SetAlpha(endAlpha);
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            itemView.SetAlpha(alpha);
            yield return null;
        }

        itemView.SetAlpha(endAlpha);
    }

    /// <summary>
    /// 필수 참조가 정상 연결되었는지 검사합니다.
    /// </summary>
    private bool ValidateReferences()
    {
        if (_stackRoot == null)
        {
            UDebug.Print("UIFeedbackMessageStackPresenter에 StackRoot가 연결되지 않았습니다.", LogType.Assert);
            return false;
        }

        if (_messageItemPrefab == null)
        {
            UDebug.Print("UIFeedbackMessageStackPresenter에 MessageItemPrefab이 연결되지 않았습니다.", LogType.Assert);
            return false;
        }

        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        if (!ValidateReferences())
        {
            return;
        }

        EventBus<OnFeedbackMessageRequested>.Subscribe(HandleFeedbackMessageRequested);
    }

    private void OnDisable()
    {
        EventBus<OnFeedbackMessageRequested>.Unsubscribe(HandleFeedbackMessageRequested);
    }
    #endregion
}
