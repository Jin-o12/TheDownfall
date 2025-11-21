using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public enum StatusUIType
{
    hp,
    energy,
    temperature,
    wet,
    Gun,
    score,
    time
}

[System.Serializable]
public class playerUIConfig
{
    public StatusUIType type;
    public UnityEngine.UI.Image Image;
    public TMP_Text Text;
}

public class StatusUI
{
    public StatusUIType type;
    public UnityEngine.UI.Image Image    {get; private set;}
    public TMP_Text Text                 {get; private set;}

    public StatusUI(playerUIConfig config)
    {
        type = config.type;
        Image = config.Image;
        Text = config.Text;
    }

    public void ImageUpdate(UnityEngine.UI.Image image)
    {
        Image = image;
    }

    public void TextUpdate(string text)
    {
        Text.text = text;
    }
}

public class InGameUI : MonoBehaviour
{
    private UIController uiController;

    /* 플레이어 상태 UI */
    [Header("플레이어 상태 UI")]
    [SerializeField] private List<playerUIConfig> playerStatus = new List<playerUIConfig>();
    private Dictionary<StatusUIType, StatusUI> playerUIDictionary = new Dictionary<StatusUIType, StatusUI>();

    /* 게임 플레이 관련 수치 */
    private int ResultScore = 0;
    private float PlayTime = 0;
    private int normalKillCount = 0;
    private int UniqueKillCount = 0;

    /* Get Function */
    public int GetResultScore() { return ResultScore; }
    public float GetPlayTime() { return PlayTime; }
    public int GetnormalKillCount() { return normalKillCount; }
    public int GetUniqueKillCount() { return UniqueKillCount; }

    void Start()
    {
        if (uiController == null)
            uiController = FindFirstObjectByType<UIController>();
    }

    public void OnEnable()
    {
        UIInitialize();
        ConnectUI();
    }

    private void ConnectUI()
    {
        foreach (playerUIConfig config in playerStatus)
        {
            if (playerUIDictionary.ContainsKey(config.type))
            {
                Debug.LogWarning($"playerUIConfig {config.type}이 중복으로 존재합니다.");
                continue;
            }

            if (config.Image == null && config.Text == null)
            {
                Debug.LogWarning($"playerUIConfig {config.type}에 Image와 Text가 모두 할당되지 않았습니다. 인스펙터를 확인하세요.");
                continue; // 딕셔너리에 추가하지 않음
            }
        
            StatusUI newUI = new StatusUI(config);
            playerUIDictionary.Add(config.type, newUI);
        }
    }

    void Update()
    {
        if (uiController == null)
            return;
 
        IsPausePressed();
        UpdateTimer();
        UpdateTimeText();
        UpdateResultScore();
    }

    /* 초기화 */
    public void UIInitialize()
    { 
        PlayTime = 0f;
        normalKillCount = 0;
        UniqueKillCount = 0;
        ResultScore = 0;
    }

    /* 일시정지 키 입력 감지 및 열기 */
    public void IsPausePressed()
    {
        if (Input.GetKeyDown(uiController.keyBinding.GamePause))
        {
            uiController.PushUI(UIType.PauseMenu);
        }
    }

    /* 인벤토리 키 감지 및 열기*/
    public void IsInventoryPressed()
    {
        if (Input.GetKeyDown(uiController.keyBinding.GunInventory))
        {
            uiController.PushUI(UIType.Inventory);
        }
    }

    /* 체력 바 업데이트 */
    public void UpdateHP(int currentHp, int fullHp)
    {
        StatusUI hpUI = playerUIDictionary[StatusUIType.hp];
        // 텍스트
        if(hpUI.Text) hpUI.TextUpdate($"{currentHp}/{fullHp}");

        // 이미지
        float ratio = (float)currentHp / fullHp;
        hpUI.Image.fillAmount = Mathf.Clamp01(ratio);
    }
    
    /* 온도 업데이트 */
    public void UpdateTemperature(float current, float full)
    {
        StatusUI UpdateTemperUI = playerUIDictionary[StatusUIType.temperature];

        // 이미지
        float ratio = (float)current / full;
        if (UpdateTemperUI.Image != null)
            UpdateTemperUI.Image.fillAmount = Mathf.Clamp01(ratio);
    }

    /* 기력 업데이트 */
    public void UpdateEnergy(int current, int full)
    {
        StatusUI energyUI = playerUIDictionary[StatusUIType.energy];

        // 이미지
        float ratio = (float)current / full;
        if (energyUI.Image != null)
            energyUI.Image.fillAmount = Mathf.Clamp01(ratio);
    }

    /* 젖음 업데이트 */
    public void UpdateWatness(float current, float full)
    {
        StatusUI wetUI = playerUIDictionary[StatusUIType.wet];

        // 이미지
        float ratio = (float)current / full;
        if (wetUI.Image != null)
            wetUI.Image.fillAmount = Mathf.Clamp01(ratio);
    }
    
    /* 탄약 UI 업데이트 */
    public void UpdateBulletUI(int currentBulletCount, int carryBulletCount)
    {
        StatusUI gunUI = playerUIDictionary[StatusUIType.Gun];
        if (gunUI.Text) gunUI.Text.text = $"{Mathf.CeilToInt(currentBulletCount)}/{Mathf.CeilToInt(carryBulletCount)}";
    }

    /* 총기 이미지 업데이트 */
    private void UpdateGunImageUI(UnityEngine.UI.Image gunImage)
    {
        StatusUI gunUI = playerUIDictionary[StatusUIType.Gun];
        gunUI.ImageUpdate(gunImage);
    }

    /* 총점 계산기 */
    private void UpdateResultScore()
    {
        StatusUI scoreUI = playerUIDictionary[StatusUIType.score];
        ResultScore = normalKillCount * 100
                     + UniqueKillCount * 500
                     + Mathf.FloorToInt(PlayTime) * 10; // 소수점 내림 수학 함수

        if (scoreUI.Text) scoreUI.Text.text = $"{ResultScore}";
    }

    /* 플레이 타임 업데이트 */
    private void UpdateTimer()
    {
        PlayTime += Time.deltaTime;
    }

    private void UpdateTimeText()
    {
        StatusUI timeUI = playerUIDictionary[StatusUIType.time];

        TimeSpan timeSpan = TimeSpan.FromSeconds(PlayTime);
        string timeStr = timeSpan.ToString(@"mm\:ss\:ff");
        if (timeUI.Text) timeUI.Text.text = timeStr;
    }

    /* 일반 좀비 킬 카운트 업데이트 */
    public void UpdateNormalKillCount(int addkillCount)
    {
        normalKillCount += addkillCount;
    }

    /* 특수 좀비 킬 카운트 업데이트 */
    public void UpdateUniqueKillCount(int addkillCount)
    {
        UniqueKillCount += addkillCount;
    }
}
