using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameLoader : MonoBehaviour
{
    [Header("로딩 UI")]
    [SerializeField] GameObject loadingCanvas;                  // 로딩 캔버스
    [SerializeField] Slider loadingSlider;                      // 로딩바

    [Header("초기화 데이터")]
    [SerializeField] PlayerManagement playerManagement;         // 플레이어 데이터
    [SerializeField] InGameUI inGameUI;                         // 인게임 수치
    [SerializeField] GunManager gunManager;

    [Header("생성 혹은 활성화 할 오브젝트")]
    [SerializeField] GameObject[] activeObjects;                // 활성화 할 오브젝트들
    [SerializeField] ZombeSpawnController zombeSpawnController; // 좀비 스폰 스크립트

    [Header("외부 참조 스크립트")]
    private UIController uiController;

    void Awake()
    {
        // 처음에 비활성화
        if (zombeSpawnController != null)
            zombeSpawnController.gameObject.SetActive(false);

        foreach(var obj in activeObjects)
        {
            if(obj != null)
                obj.SetActive(false);
        }
        loadingSlider.value = 0f;
    }

    void Start()
    {
        uiController = FindFirstObjectByType<UIController>();
    }

    /* 로딩 시작 */
    public void InitLoadGame()
    {
        StartCoroutine(LoadGameDataCoroutine());
        loadingCanvas.SetActive(true);
    }

    IEnumerator LoadGameDataCoroutine()
    {
        // 1: 데이터 초기화
        InitializeGameData();
        loadingSlider.value = 0.25f;
        yield return null;

        // 2. 지형 활성화
        yield return StartCoroutine(ActivateTerrainObjects());
        loadingSlider.value = 0.75f;

        // 3. 좀비 스포너 활성화
        if (zombeSpawnController != null)
            zombeSpawnController.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);
        loadingSlider.value = 1.0f;

        Debug.Log("로딩 완료");
        yield return new WaitForSeconds(0.5f);  // 로딩 완료 직후 잠시 대기 (100프로 보여줌)
        loadingCanvas.SetActive(false);

        // Awake에서 비활성화 된 코드들을 직접 활성화
        if (inGameUI != null)
        {
            zombeSpawnController.enabled = true;
            inGameUI.enabled = true;
            gunManager.enabled = true;
        }
        loadingCanvas.SetActive(false);
        uiController.PushUI(UIType.InGame);
    }

    private void InitializeGameData()
    {

    }

    IEnumerator ActivateTerrainObjects()
    {

        foreach (GameObject obj in activeObjects)
        {
            if (obj == null) continue;
            
            obj.SetActive(true);

            var children = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in obj.transform)
            {
                children.Add(child);
            }

            int totalCount = children.Count;
            if (totalCount == 0) yield break;

            for (int i = 0; i < totalCount; i++)
            {
                // 자식 오브젝트 활성화
                children[i].gameObject.SetActive(true);

                // 20개 켤 때마다 1프레임 쉬기
                if (i % 20 == 0)
                {
                    float progress = (float)i / totalCount;
                    loadingSlider.value = 0.25f + (progress * 0.5f);

                    yield return null;  // 다음 프레임까지 대기
                }
            }
        }
    }
}
