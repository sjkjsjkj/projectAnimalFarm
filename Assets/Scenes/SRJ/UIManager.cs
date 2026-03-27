using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject inventoryUI;
    public GameObject storageUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            storageUI.SetActive(!storageUI.activeSelf);
        }
    }
}
