using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 기준 낚시 처리 전용 컨트롤러
/// 
/// 주의
/// - SheetItemRow는 소문자 필드(id, name, category, rarity, isWaterFish, isDeepWaterFish)를 사용한다.
/// - 기존 대문자 프로퍼티(Id, Name, Category...)를 사용하면 컴파일 에러가 난다.
/// </summary>
public class CFishingController2D : BaseMono
{
    private enum EFishingAreaType
    {
        None = 0,
        FreshWater,
        SeaWater
    }

    [Header("낚시 대상 DB")]
    [SerializeField] private SheetItemDatabase _database;

    [Header("낚시 판정")]
    [SerializeField] private float _checkDistance = 0.9f;
    [SerializeField] private Vector2 _checkOffset = Vector2.zero;
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

    [Header("로그")]
    [SerializeField] private bool _logEnabled = true;

    private bool _isFishing = false;
    private float _cooldownEndTime = 0f;

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

    public string GetInteractionMessage(KeyCode key, CPlayerCollector2D collector)
    {
        if (collector == null)
        {
            return string.Empty;
        }

        if (!TryGetFishingAreaType(collector, out EFishingAreaType areaType, out _))
        {
            return string.Empty;
        }

        switch (areaType)
        {
            case EFishingAreaType.FreshWater:
                return $"[{key}] {_freshWaterPromptText}";
            case EFishingAreaType.SeaWater:
                return $"[{key}] {_seaWaterPromptText}";
            default:
                return string.Empty;
        }
    }

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
            bool received = collector != null && collector.TryReceiveItem(fishRow.id, _amount);

            if (received)
            {
                if (_fishingSkillExp > 0 && collector != null)
                {
                    collector.TryAddLifeSkillExp(ELifeSkill.Fishing, _fishingSkillExp);
                }

                if (_logEnabled)
                {
                    Debug.Log($"[CFishingController2D] 낚시 성공: {fishRow.name} ({fishRow.id}) x{_amount}");
                }
            }
            else if (_logEnabled)
            {
                Debug.LogWarning($"[CFishingController2D] 낚시 보상 지급 실패: {fishRow.id}");
            }
        }

        _cooldownEndTime = Time.time + Mathf.Max(0f, _cooldownAfterFishing);

        if (collector != null)
        {
            collector.SetInteractionBusy(false);
        }

        _isFishing = false;
    }

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

            if (row.category != "Fish")
            {
                continue;
            }

            switch (areaType)
            {
                case EFishingAreaType.FreshWater:
                    if (!row.isWaterFish)
                    {
                        continue;
                    }
                    break;

                case EFishingAreaType.SeaWater:
                    if (!row.isDeepWaterFish)
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
            totalWeight += Mathf.Max(0.01f, GetWeightByRarity(candidates[i].rarity));
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += Mathf.Max(0.01f, GetWeightByRarity(candidates[i].rarity));

            if (randomValue <= cumulative)
            {
                return candidates[i];
            }
        }

        return candidates[candidates.Count - 1];
    }
}
