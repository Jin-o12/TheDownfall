using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class GameoverMenu : MonoBehaviour
{
    UIController uiController;

    TMP_Text gameResultLeft;
    TMP_Text gameResultRight;
    TMP_Text gameResultCenter;

    void Start()
    {
        uiController = FindFirstObjectByType<UIController>();

        gameResultLeft = uiController.gameResultLeft;
        gameResultRight = uiController.gameResultRight;
        gameResultCenter = uiController.gameResultCenter;
    }

    public void StartGameover()
    {
        UpdateResultText();
    }

    private void UpdateResultText()
    {
        InGameUI inGameUI = FindFirstObjectByType<InGameUI>();

        TimeSpan timeSpan = TimeSpan.FromSeconds(inGameUI.GetPlayTime());

        string timeStr = timeSpan.ToString(@"mm\:ss");
        int normalKill = inGameUI.GetnormalKillCount();
        int uniqueKill = inGameUI.GetUniqueKillCount();
        int result = inGameUI.GetResultScore();

        if (gameResultLeft) gameResultLeft.text = $"살아남은 시간:   {timeStr}\n" +
                                                  $"죽인 일반 좀비 수:   {normalKill}명\n" +
                                                  $"죽인 특수 좀비 수:   {uniqueKill}명\n\n\n" +
                                                  $"총 점수:   {result}\n";

        if (gameResultCenter) gameResultCenter.text = $"* {10}  =\n" +
                                                    $"* {100} =\n" +
                                                    $"* {500} =\n";                                          

        if (gameResultRight) gameResultRight.text = $"{Mathf.FloorToInt(inGameUI.GetPlayTime()) * 10}점\n" +
                                                    $"{normalKill * 100}점\n" +
                                                    $"{uniqueKill * 500}점\n";
    }

    public void RestartGame()
    {
        uiController.PushUI(UIType.InGame);
    }

    public void GoMainmenu()
    {
        uiController.PushUI(UIType.MainMenu);
    }
}