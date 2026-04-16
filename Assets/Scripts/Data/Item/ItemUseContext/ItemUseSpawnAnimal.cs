using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "AnimalSpawn_", menuName = "ScriptableObjects/ItemUseContext/AnimalSpawn", order = 1)]
public class ItemUseSpawnAnimal : ItemUseContextSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] protected string _spawnAnimalId = "";
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string SpawnAnimalId => _spawnAnimalId;

    public override bool TryUse()
    {
        if(BreedingArea.Ins==null)
        {
            return false;
        }
        BreedingArea.Ins.SpawnAnimal(_spawnAnimalId);
        
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    
    #endregion
}
