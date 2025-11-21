using System.Collections;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private UIController uiController;

    void Start()
    {
        uiController = FindFirstObjectByType<UIController>();
    }

    public void OnEnable()
    {
        if (uiController == null)
            return;

        Settings();
    }

    /* 설정 메뉴 */
    public void Settings()
    {
        uiController.PushUI(UIType.Settings);
    }

    /* 게임 재개 */
    public void Resume()
    {
        Debug.Log("게임 재게");
        Time.timeScale = 1f;
    }

    /* 메인메뉴로 이동 선택시 확인 팝업 */
    public void PlayMainmenuNoticePanel()
    {
        uiController.PushUI(UIType.SaveCheckNotice);
    }

    /* 메인메뉴로 이동 */
    public void GoMainmenu()
    {
        uiController.PushUI(UIType.SaveLoadingNotice);
        
        // 데이터 저장 과정 코드 추가할 것 //
        LoadMainmenuCoroutine();
        // 당장은 코루틴으로 대체 //

        uiController.PushUI(UIType.MainMenu);
    }
    
    // 테스트용: 세이브 기능 추가되면 삭제
    IEnumerator LoadMainmenuCoroutine()
    {
        yield return new WaitForSeconds(2f);
    }

    /* 메인메뉴로 이동 취소 */
    public void CancelMainmenu()
    {
        uiController.PopUI();
    }
}
