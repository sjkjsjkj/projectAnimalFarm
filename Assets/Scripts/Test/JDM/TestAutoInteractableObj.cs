using UnityEngine;

public class TestAutoInteractableObj : BaseMono, IAutoInteractable
{
    public bool CanInteract(GameObject player)
    {
        // 통과시켜드립니다.
        return true;
    }

    public string GetMessage()
    {
        UDebug.Print("도망가!");
        return "도망가!";
    }

    public void Interact(GameObject player)
    {
        Vector3 pos = transform.position;
        pos.x += Random.Range(-0.2f, 0.2f);
        pos.y += Random.Range(-0.2f, 0.2f);
        transform.position = pos;
    }
}
