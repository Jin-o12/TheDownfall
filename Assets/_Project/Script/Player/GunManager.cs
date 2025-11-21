using UnityEngine;
using TMPro;

public class GunManager : MonoBehaviour
{
    [Header("Gun Info")]
    public string gunName;                  // 총 이름
    public float range;                     // 사거리
    public float accuracy;                  // 명중률
    public float fireRate;                  // 연사 속도
    public float reloadTime;                // 재장전 속도

    public int damage;

    [Header("Bullet")]
    public int reloadBulletCount;           // 한 번에 장전하는 총알 갯수
    public int currentBulletCount;          // 탄집에 남은 총알 갯수
    public int maxBulletCount;              // 최대 소유 가능 총알 갯수
    public int carryBulletCount;            // 현재 소유 총알 갯수

    [Header("Gun State")]
    public float retroActionForce;          // 반동 세기
    public float retroActionFineSightForce; // 정조준시 반동 세기

    [Header("References")]
    public Vector3 fireSightOriginPos;      // 정조준 위치
    public Animator anim;                   // 애니메이터
    public ParticleSystem muzzleFlash;      // 총구 화염 이펙트

    [Header("Audio")]
    public AudioClip fireSound;             // 총격음
    public AudioClip reloadSound;           // 재장전음
    public AudioSource fireSoundAudio;      // 총격음 오디오 소스
    [Range(0, 1)] public float fireVolume = 0.5f;

    [Header("외부 스크립트 참조")]
    public UIController uiController;
    public InGameUI inGameUI;

    void Awake()
    {
        enabled = false;
    }
    
    void OnEnable()
    {
        UpdateBulletUI();
    }

    void Update()
    {
        UpdateBulletUI();
    }

    public Vector3 GetLocalPosition()
    {
        return transform.localPosition;
    }

    private void UpdateBulletUI()
    {
        inGameUI.UpdateBulletUI(currentBulletCount, carryBulletCount);
    }
}
