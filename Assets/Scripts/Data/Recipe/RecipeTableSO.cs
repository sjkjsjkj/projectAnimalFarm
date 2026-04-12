using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "RecipeTableSO_", menuName = "ScriptableObjects/Table/RecipeTable", order = 1)]
public class RecipeTableSO : TableSO<RecipeSO>
{
 
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public List<RecipeSO> ElementalList => _elements;


    public IReadOnlyList<RecipeSO> GetRecipeTable()
    {
        return _elements;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void OnValidate()
    {
        base.OnValidate();
    }
    #endregion
}
