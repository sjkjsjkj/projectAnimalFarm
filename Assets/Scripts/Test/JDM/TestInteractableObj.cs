using UnityEngine;

public class TestInteractableObj : BaseMono, IInteractable
{
    private bool _use = false;

    public bool CanInteract(GameObject player)
    {
        if (_use)
        {
            UDebug.Print("이미 사용하셨어요.");
            return false; 
        }
        // if(player.TryGetComponent(out ) ) 지금은 가져올만한게 없다.
        var provider = DataManager.Ins.Player; // 대신 플레이어 데이터는 싱글톤에서 접근 가능
        if(provider.CurHunger < 10f)
        {
            UDebug.Print("배고파요..");
            return false;
        }
        return true;
    }

    public string GetMessage()
    {
        UDebug.Print("테스트 오브젝트를 건드리자 도망가버렸습니다.");
        return "테스트 오브젝트를 건드리자 도망가버렸습니다.";
    }

    public void Interact(GameObject player)
    {
        _use = true;
        Vector3 pos = transform.position;
        pos.x += Random.Range(-2, 2);
        pos.y += Random.Range(-2, 2);
        transform.position = pos;
        // 배고픔도 감소
        var provider = DataManager.Ins.Player;
        provider.ConsumeHunger(10f); // 조건 검사는 CanInteract에 있으니 괜찮다.
        // 혹시나 플레이어의 배고픔 값이 10 미만이라더라도 provider에 방어 코드가 또 존재
    }
}
