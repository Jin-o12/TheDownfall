using UnityEngine;
using TMPro;
using System.Collections;
using NUnit.Framework;

public class PlayerManagement : MonoBehaviour
{
    public enum SurfaceType { Default, Water, Wood, Dirt }

    [Header("플레이어 발소리 재생시 지면 상태")]
    public SurfaceType nowSurfaceType;

    [Header("플레이어 상태")]
    [SerializeField] int fullHp;
    [SerializeField] int currentHp;
    /* 스테미나 */
    [SerializeField] int fullStamina;
    [SerializeField] int currentStamina;
    [SerializeField] int useStamina;
    [SerializeField] int recoveryStamina;
    /* 젖음 정도 */
    [SerializeField] float fullWetness;
    [SerializeField] float currentWetness;
    [SerializeField] int addWetness;
    [SerializeField] int removeWetness;
    /* 온도 */
    [SerializeField] float fullTemperature;
    [SerializeField] float currentTemperature;
    
    [Header("외부 스크립트 참조")]
    [SerializeField] public UIController uiController;
    [SerializeField] public InGameUI inGameUI;
    [SerializeField] public WeatherController weatherController;
    [SerializeField] public PlayerMovement playerMovement;


    private bool canHit = true;

    void Start()
    {
        uiController = FindFirstObjectByType<UIController>();
        inGameUI = FindFirstObjectByType<InGameUI>();
        weatherController = FindFirstObjectByType<WeatherController>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();

        currentHp = fullHp;
        currentStamina = fullStamina;
        currentWetness = 20;
        currentTemperature = 36.5f;
    }

    void OnEnable()
    {
        if (inGameUI == null)
            inGameUI = FindFirstObjectByType<InGameUI>();

        if (inGameUI == null)
        {
            Debug.LogWarning("PlayerManagement: InGameUI not found on OnEnable. UI updates will be skipped until assigned.");
        }

        nowSurfaceType = SurfaceType.Water;
        fullHp = 100;
        currentHp = fullHp;
        Debug.Log("Set Player HP"+currentHp);

        StartCoroutine(ChangeStemina(1.0f));
        StartCoroutine(ChangeWetness(1.0f));
    }

    void Update()
    {
        if (inGameUI != null)
        {
            InitPlayerUI();
        }
        else
        {
            inGameUI = FindFirstObjectByType<InGameUI>();
            if (inGameUI != null)
                InitPlayerUI();
        }
        IsDead();
        SetPlayrTemperature();
    }

    private void InitPlayerUI()
    {
        inGameUI.UpdateHP(currentHp, fullHp);
        inGameUI.UpdateEnergy(currentStamina, fullStamina);
        inGameUI.UpdateTemperature(currentTemperature, fullTemperature);
        inGameUI.UpdateWatness(currentWetness, fullWetness);
    }

    private void OnTriggerStay(Collider collider)
    {
        var obj = collider.gameObject;

        if (obj.CompareTag("Inside"))
        {
            nowSurfaceType = SurfaceType.Default;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        nowSurfaceType = SurfaceType.Water;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.gameObject;

        if (obj.CompareTag("Enemy"))
        {
            TakeDamage(10);
            StartCoroutine(HitCooldown());
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHp -= dmg;
    }

    IEnumerator HitCooldown()
    {
        canHit = false;
        yield return new WaitForSeconds(1.0f);
        canHit = true;
    }

    private void IsDead()
    {
        if (currentHp <= 0)
        {
            if (uiController != null)
                uiController.PushUI(UIType.GameOver);
            else
                Debug.LogWarning("PlayerManagement: uiController is null when player died.");
        }
    }

    /* 플레이어 온도 변경 */
    public void SetPlayrTemperature()
    {
        float addTemp = 0f;

        if(0 <= currentWetness && currentWetness <=10)
            addTemp = 0.5f;
        else if(currentWetness>30)
            addTemp = -0.5f;
        else if(currentWetness>50)
            addTemp = -1f;

        float nextTemp = currentTemperature + addTemp;
        if(nextTemp > fullTemperature)
            currentTemperature = fullTemperature;
        else if(nextTemp < 0f)
            currentTemperature = 0f;
        else
            currentTemperature = nextTemp;

        inGameUI.UpdateTemperature(currentTemperature, fullTemperature);
    }

    /* 플레이어 스테미나 변경 */
    IEnumerator ChangeStemina(float sec)
    {
        int addStamina = playerMovement.GetIsRun() ? useStamina : recoveryStamina;
        
        int result = currentStamina + addStamina;
        if(result > fullStamina)
            currentStamina = fullStamina;
        else if(result < 0)
            currentStamina = 0;
        else
            currentStamina += result;

        inGameUI.UpdateEnergy(currentStamina, fullStamina);

        yield return new WaitForSeconds(sec);
    }

    public IEnumerator UseStemina()
    {
        int result = currentStamina + useStamina;
        if(result < 0)
            currentStamina = 0;
        else
            currentStamina += result;

        inGameUI.UpdateEnergy(currentStamina, fullStamina);

        yield return null;
    }

    /* 플레이어 젖음 정도 변경 */
    IEnumerator ChangeWetness(float sec)
    {
        float changeWetness = weatherController.GetIsRainy() ? addWetness : removeWetness;
        
        float result = currentWetness + changeWetness;
        if(result > fullStamina)
            currentWetness = fullWetness;
        else if(result < 0)
            currentWetness = 0;
        else
            currentWetness += result;

        inGameUI.UpdateWatness(currentWetness, fullWetness);

        yield return new WaitForSeconds(sec);
    }
}