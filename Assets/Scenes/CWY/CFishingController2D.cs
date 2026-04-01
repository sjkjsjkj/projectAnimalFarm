using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 기준 낚시 처리 전용 컨트롤러
/// - 플레이어 앞 타일이 낚시 가능한 타일인지 검사
/// - 시트 DB의 Fish 카테고리 아이템 중 조건에 맞는 물고기를 랜덤 지급
/// - ItemCollectionCoordinator는 CPlayerCollector2D를 통해 호출
/// 
/// 의도
/// - 채집은 오브젝트 기반
/// - 낚시는 타일 기반
/// 
/// 이렇게 나눠서 팀원 코드 구조를 최대한 유지한다.
/// </summary>
public class CFishingController2D : BaseMono
{
    private enum EFishingAreaType
    {
        None = 0,
        FreshWater,
        SeaWater
    }

    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("대상 DB")]
    [SerializeField] private SheetItemDatabase _database;

    [Header("낚시 판정")]
    [Tooltip("플레이어 위치에서 바라보는 방향으로 얼마만큼 앞 타일을 검사할지")]
    [SerializeField] private float _checkDistance = 0.9f;

    [Tooltip("플레이어 위치 기준 추가 오프셋")]
    [SerializeField] private Vector2 _checkOffset = Vector2.zero;

    [Tooltip("바다 낚시도 허용할지")]
    [SerializeField] private bool _allowSeaFishing = true;

    [Header("낚시 연출")]
    [SerializeField] private float _fishingDelay = 1.5f;
    [SerializeField] private float _cooldownAfterFishing = 0.2f;

    [Header("지급 설정")]
    [SerializeField] private int _amount = 1;
    [SerializeField] private int _fishingSkillExp = 0;

    [Header("안내 문구")]
    [SerializeField] private string _freshWaterPromptText = "낚시하기";
    [SerializeField] private string _seaWaterPromptText = "바다 낚시하기";

    [Header("희귀도 가중치")]
    [SerializeField] private float _basicWeight = 70f;
    [SerializeField] private float _primeWeight = 25f;
    [SerializeField] private float _rareWeight = 10f;
    [SerializeField] private float _legendaryWeight = 2f;
    [SerializeField] private float _unknownWeight = 5f;

    [Header("로그 출력")]
    [SerializeField] private bool _logEnabled = true;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isFishing = false;
    private float _cooldownEndTime = 0f;
    #endregion

    #region ─────────────────────────▶ 외부 메서드 ◀─────────────────────────
    /// <summary>
    /// 현재 낚시 가능 여부 검사
    /// </summary>
    public bool CanManualFish(CPlayerCollector2D collector)
    {
        if (_isFishing)
        {
            return false;
        }

        if (collector == null)
        {
            return false;
        }

        if (collector.IsBusy)
        {
            return false;
        }

        if (Time.time < _cooldownEndTime)
        {
            return false;
        }

        if (_database == null)
        {
            return false;
        }

        return TryGetFishingAreaType(collector, out _, out _);
    }

    /// <summary>
    /// 안내 문구 생성
    /// </summary>
    public string GetInteractionMessage(KeyCode key, CPlayerCollector2D collector)
    {
        if (!TryGetFishingAreaType(collector, out EFishingAreaType areaType, out _))
        {
            return string.Empty;
        }

        string text = areaType == EFishingAreaType.SeaWater
            ? _seaWaterPromptText
            : _freshWaterPromptText;

        return $"[{key}] {text}";
    }

    /// <summary>
    /// 낚시 시작 시도
    /// </summary>
    public bool TryFish(CPlayerCollector2D collector)
    {
        if (!CanManualFish(collector))
        {
            return false;
        }

        if (!collector.CanStartInteraction())
        {
            return false;
        }

        if (!TryGetFishingAreaType(collector, out EFishingAreaType areaType, out Vector2 checkPos))
        {
            return false;
        }

        StartCoroutine(CoFishing(collector, areaType, checkPos));
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    /// <summary>
    /// 실제 낚시 처리
    /// - 플레이어 Busy
    /// - 낚시 모션
    /// - 대기
    /// - 랜덤 물고기 지급
    /// - 쿨타임
    /// </summary>
    private IEnumerator CoFishing(CPlayerCollector2D collector, EFishingAreaType areaType, Vector2 checkPos)
    {
        _isFishing = true;

        if (collector != null)
        {
            collector.SetInteractionBusy(true);
            collector.PlayFishingAnimation();
        }

        if (_fishingDelay > 0f)
        {
            yield return new WaitForSeconds(_fishingDelay);
        }

        SheetItemRow fishRow = GetRandomFishRow(areaType);

        if (fishRow == null)
        {
            if (_logEnabled)
            {
                Debug.LogWarning($"[CFishingController2D] 조건에 맞는 물고기를 찾지 못했습니다. areaType={areaType}, pos={checkPos}");
            }
        }
        else
        {
            bool received = collector != null && collector.TryReceiveItem(fishRow.Id, _amount);

            if (received)
            {
                if (_fishingSkillExp > 0 && collector != null)
                {
                    collector.TryAddLifeSkillExp(ELifeSkill.Fishing, _fishingSkillExp);
                }

                if (_logEnabled)
                {
                    Debug.Log($"[CFishingController2D] 낚시 성공: {fishRow.Name} ({fishRow.Id}) x{_amount}");
                }
            }
            else if (_logEnabled)
            {
                Debug.LogWarning($"[CFishingController2D] 낚시 보상 지급 실패: {fishRow.Id}");
            }
        }

        _cooldownEndTime = Time.time + Mathf.Max(0f, _cooldownAfterFishing);

        if (collector != null)
        {
            collector.SetInteractionBusy(false);
        }

        _isFishing = false;
    }

    /// <summary>
    /// 현재 플레이어 기준으로 낚시 가능한 지역 타입 판정
    /// - 일반 물 낚시
    /// - 바다 낚시
    /// </summary>
    private bool TryGetFishingAreaType(CPlayerCollector2D collector, out EFishingAreaType areaType, out Vector2 checkWorldPos)
    {
        areaType = EFishingAreaType.None;
        checkWorldPos = Vector2.zero;

        if (collector == null)
        {
            return false;
        }

        if (TileManager.Ins == null || TileManager.Ins.Tile == null)
        {
            return false;
        }

        TileMap map = TileManager.Ins.Tile;

        Vector2 facingDir = GetCardinalFacingDirection();
        checkWorldPos = (Vector2)collector.transform.position + _checkOffset + facingDir * _checkDistance;

        if (map.IsFishingable(checkWorldPos))
        {
            areaType = EFishingAreaType.FreshWater;
            return true;
        }

        if (_allowSeaFishing && map.IsSeaFishingable(checkWorldPos))
        {
            areaType = EFishingAreaType.SeaWater;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 플레이어가 현재 바라보는 방향을 4방향 기준으로 정리해서 반환
    /// 대각 입력이 들어와도 최종 검사 타일은 상하좌우 중 하나만 보게 만듦
    /// </summary>
    private Vector2 GetCardinalFacingDirection()
    {
        Vector2 dir = Vector2.down;

        if (DataManager.Ins != null && DataManager.Ins.Player != null)
        {
            dir = DataManager.Ins.Player.Direction;
        }

        if (dir.sqrMagnitude <= 0.0001f)
        {
            return Vector2.down;
        }

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x >= 0f ? Vector2.right : Vector2.left;
        }

        return dir.y >= 0f ? Vector2.up : Vector2.down;
    }

    /// <summary>
    /// 조건에 맞는 물고기 후보 목록 생성
    /// </summary>
    private List<SheetItemRow> GetFishCandidates(EFishingAreaType areaType)
    {
        List<SheetItemRow> result = new List<SheetItemRow>();

        if (_database == null)
        {
            return result;
        }

        IReadOnlyList<SheetItemRow> allItems = _database.AllItems;

        for (int i = 0; i < allItems.Count; i++)
        {
            SheetItemRow row = allItems[i];

            if (row == null)
            {
                continue;
            }

            if (row.Category != "Fish")
            {
                continue;
            }

            switch (areaType)
            {
                case EFishingAreaType.FreshWater:
                    if (!row.IsWaterFish)
                    {
                        continue;
                    }
                    break;

                case EFishingAreaType.SeaWater:
                    if (!row.IsDeepWaterFish)
                    {
                        continue;
                    }
                    break;

                default:
                    continue;
            }

            result.Add(row);
        }

        return result;
    }

    /// <summary>
    /// 희귀도 문자열에 따라 가중치 반환
    /// 시트의 rarity 값이 늘어나도 기본값으로 버틸 수 있게 처리
    /// </summary>
    private float GetWeightByRarity(string rarity)
    {
        if (string.IsNullOrWhiteSpace(rarity))
        {
            return _unknownWeight;
        }

        string key = rarity.Trim().ToLowerInvariant();

        switch (key)
        {
            case "basic":
                return _basicWeight;

            case "prime":
                return _primeWeight;

            case "rare":
                return _rareWeight;

            case "legend":
            case "legendary":
                return _legendaryWeight;

            default:
                return _unknownWeight;
        }
    }

    /// <summary>
    /// 조건에 맞는 물고기 하나를 가중치 랜덤으로 뽑는다.
    /// </summary>
    private SheetItemRow GetRandomFishRow(EFishingAreaType areaType)
    {
        List<SheetItemRow> candidates = GetFishCandidates(areaType);

        if (candidates.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            totalWeight += Mathf.Max(0.01f, GetWeightByRarity(candidates[i].Rarity));
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += Mathf.Max(0.01f, GetWeightByRarity(candidates[i].Rarity));

            if (randomValue <= cumulative)
            {
                return candidates[i];
            }
        }

        return candidates[candidates.Count - 1];
    }
    #endregion
}
