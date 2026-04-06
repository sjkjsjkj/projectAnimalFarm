using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("기존 타일 기반 낚시 판정")]
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
            if (_logEnabled) Debug.Log("[FishingController] 실패: 이미 낚시 중");
            return false;
        }

        if (collector == null)
        {
            if (_logEnabled) Debug.Log("[FishingController] 실패: collector null");
            return false;
        }

        if (collector.IsBusy)
        {
            if (_logEnabled) Debug.Log("[FishingController] 실패: collector Busy 상태");
            return false;
        }

        if (Time.time < _cooldownEndTime)
        {
            if (_logEnabled) Debug.Log("[FishingController] 실패: 쿨타임 중");
            return false;
        }

        if (_database == null)
        {
            if (_logEnabled) Debug.Log("[FishingController] 실패: SheetItemDatabase 연결 안 됨");
            return false;
        }

        bool canFish = TryGetFishingAreaType(collector, out EFishingAreaType areaType, out Vector2 checkPos);

        if (_logEnabled)
        {
            Debug.Log("[FishingController] CanManualFish 판정 = " + canFish +
                      " / areaType = " + areaType +
                      " / checkPos = " + checkPos);
        }

        return canFish;
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
                return "[" + key + "] " + _freshWaterPromptText;

            case EFishingAreaType.SeaWater:
                return "[" + key + "] " + _seaWaterPromptText;

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
            if (_logEnabled) Debug.Log("[FishingController] 실패: collector.CanStartInteraction() == false");
            return false;
        }

        if (!TryGetFishingAreaType(collector, out EFishingAreaType areaType, out Vector2 checkPos))
        {
            if (_logEnabled) Debug.Log("[FishingController] 실패: TryGetFishingAreaType == false");
            return false;
        }

        if (_logEnabled)
        {
            Debug.Log("[FishingController] 낚시 시작 / areaType = " + areaType + " / checkPos = " + checkPos);
        }

        StartCoroutine(CoFishing(collector, areaType, checkPos));
        return true;
    }

    public bool CanManualFishFromSpot(CPlayerCollector2D collector, bool useSeaFishing)
    {
        if (_isFishing)
        {
            if (_logEnabled) Debug.Log("[FishingController] 실패: 이미 낚시 중");
            return false;
        }

        if (collector == null)
        {
            if (_logEnabled) Debug.Log("[FishingController] 실패: collector null");
            return false;
        }

        if (collector.IsBusy)
        {
            if (_logEnabled) Debug.Log("[FishingController] 실패: collector Busy 상태");
            return false;
        }

        if (Time.time < _cooldownEndTime)
        {
            if (_logEnabled) Debug.Log("[FishingController] 실패: 쿨타임 중");
            return false;
        }

        if (_database == null)
        {
            if (_logEnabled) Debug.Log("[FishingController] 실패: SheetItemDatabase 연결 안 됨");
            return false;
        }

        EFishingAreaType areaType = useSeaFishing ? EFishingAreaType.SeaWater : EFishingAreaType.FreshWater;
        List<SheetItemRow> candidates = GetFishCandidates(areaType);

        if (_logEnabled)
        {
            Debug.Log("[FishingController] FishingSpot 기준 낚시 가능 후보 수 = " + candidates.Count +
                      " / areaType = " + areaType);
        }

        return candidates.Count > 0;
    }

    public bool TryFishFromSpot(CPlayerCollector2D collector, bool useSeaFishing)
    {
        if (!CanManualFishFromSpot(collector, useSeaFishing))
        {
            return false;
        }

        if (!collector.CanStartInteraction())
        {
            if (_logEnabled) Debug.Log("[FishingController] 실패: collector.CanStartInteraction() == false");
            return false;
        }

        EFishingAreaType areaType = useSeaFishing ? EFishingAreaType.SeaWater : EFishingAreaType.FreshWater;

        if (_logEnabled)
        {
            Debug.Log("[FishingController] FishingSpot 기준 낚시 시작 / areaType = " + areaType);
        }

        StartCoroutine(CoFishing(collector, areaType, collector.transform.position));
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
                Debug.LogWarning("[FishingController] 조건에 맞는 물고기 없음 / areaType = " + areaType);
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
                    Debug.Log("[FishingController] 낚시 성공: " + fishRow.name + " (" + fishRow.id + ") x" + _amount);
                }
            }
            else if (_logEnabled)
            {
                Debug.LogWarning("[FishingController] 낚시 보상 지급 실패: " + fishRow.id);
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
            if (_logEnabled) Debug.Log("[FishingController] TryGetFishingAreaType 실패: collector null");
            return false;
        }

        if (TileManager.Ins == null)
        {
            if (_logEnabled) Debug.Log("[FishingController] TryGetFishingAreaType 실패: TileManager.Ins null");
            return false;
        }

        if (TileManager.Ins.Tile == null)
        {
            if (_logEnabled) Debug.Log("[FishingController] TryGetFishingAreaType 실패: TileManager.Ins.Tile null");
            return false;
        }

        TileMap map = TileManager.Ins.Tile;
        Vector2 facingDir = GetCardinalFacingDirection();
        checkWorldPos = (Vector2)collector.transform.position + _checkOffset + facingDir * _checkDistance;

        bool fresh = map.IsFishingable(checkWorldPos);
        bool sea = _allowSeaFishing && map.IsSeaFishingable(checkWorldPos);

        if (_logEnabled)
        {
            Debug.Log("[FishingController] 판정 위치 = " + checkWorldPos +
                      " / facingDir = " + facingDir +
                      " / fresh = " + fresh +
                      " / sea = " + sea +
                      " / playerPos = " + collector.transform.position);
        }

        if (fresh)
        {
            areaType = EFishingAreaType.FreshWater;
            return true;
        }

        if (sea)
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

        if (_logEnabled)
        {
            Debug.Log("[FishingController] 후보 물고기 수 = " + candidates.Count + " / areaType = " + areaType);
        }

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
