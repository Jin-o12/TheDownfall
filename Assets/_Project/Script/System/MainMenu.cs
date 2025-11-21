using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private UIController uiController;
    private GameLoader gameLoader;

    void Start()
    {
        uiController = FindFirstObjectByType<UIController>();
        gameLoader = FindFirstObjectByType<GameLoader>();
    }

    public void StartNewGame()
    {
        LoadGame();
        Debug.Log("Start Game");
    }

    public void StartSaveGame()
    {
        /// 저장 데이터 구현 후, 저장 데이터 불러오는 코드 작성 할 것 ///
        gameLoader.InitLoadGame();
        //uiController.PushUI(UIType.InGame);
        Debug.Log("Start Game");
    }

    public void LoadGame()
    {
        gameLoader.InitLoadGame();
        //uiController.PushUI(UIType.InGame);
        Debug.Log("Start Game");
    }

    public void OpenRecords()
    {
        uiController.PushUI(UIType.Record);
    }

    public void Settings()
    {
        uiController.PushUI(UIType.Settings);
    }
    
    public void QuitGame()
    {
    #if UNITY_EDITOR
            // 유니티 에디터
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            // 빌드 실행
            Application.Quit();
    #endif
    }
}
