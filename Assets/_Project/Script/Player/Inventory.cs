using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("인벤토리 UI")]
    [SerializeField] GameObject inventoryCanvas;
    [SerializeField] GameObject itemBoxPrefab;

    [Header("Layout Group")]
    [SerializeField]
    private RectTransform containerLayout;

    [Header("스크립트")]
    public UIController uiController;

    // 생성된 아이템들을 관리하기 위한 리스트
    private int boxNum = 20;
    private List<GameObject> spawnedItems = new List<GameObject>();

    void Start()
    {
        SpawnItemBoxes();
    }

    void Update()
    {
        CloseInventory();
    }

    /* 아이템 상자 생성 */
    public void SpawnItemBoxes()
    {
        for (int i = 0; i < boxNum; i++)
        {
            GameObject newItemBox = Instantiate(itemBoxPrefab, containerLayout);
            
            spawnedItems.Add(newItemBox);
        }
    }

    // public void ClearItems()
    // {
        
    //     for (int i = spawnedItems.Count - 1; i >= 0; i--)
    //     {
    //         if (spawnedItems[i] != null)
    //         {
    //             Destroy(spawnedItems[i]);
    //         }
    //     }
    //     spawnedItems.Clear();
    // }

    void CloseInventory()
    {
        if (Input.GetKeyDown(uiController.keyBinding.GamePause))
            uiController.PopUI();
    }
}
