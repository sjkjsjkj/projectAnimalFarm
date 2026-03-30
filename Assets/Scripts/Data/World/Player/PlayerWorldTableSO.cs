using UnityEngine;

/// <summary>
/// 플레이어 SO를 관리하는 테이블 클래스
/// 데이터가 하나뿐이라 테이블일 필요는 없지만 통일성을 주기 위함
/// </summary>
[CreateAssetMenu(fileName = "PlayerWorldTableSO_", menuName = "ScriptableObjects/Table/PlayerWorld", order = 1)]
public class PlayerWorldTableSO : TableSO<PlayerWorldSO>
{
    
}
