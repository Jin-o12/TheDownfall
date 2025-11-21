using System.Collections;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System.Collections.Generic;
using TMPro;


public class GunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] KeyBinding keyBinding;                     // 키 설정 스크립트
    [SerializeField] PlayerMovement playerMovement;             // 플레이어 움직임 스크립트
    [SerializeField] GameObject PauseUI;

    [Header("Gun Shot Settings")]
    [SerializeField] private GunManager currentGun;             // 현재 장착된 총
    private float currentFireRate;                              // 현재 연사 속도 타이머
    private bool isReloading = false;                           // 재장전 중인지 여부
    private bool isFineSight = false;                           // 정조준 중인지 여부
    private Vector3 originPos;                                  // 정조준 전 총 원래 위치
    public float recoilYOffset = 0.5f;                          // 반동 시 y축 오프셋

    [Header("Hit Settings")]
    private RaycastHit hitInfo;                                 // 레이캐스트 충돌 정보
    private Camera theCamera;                                   // 카메라 컴포넌트
    private GameObject hitTarget;                               // 맞은 오브젝트
    public GameObject hitEffect;                                // 맞은 오브젝트에 가하는 힘
    public LayerMask enemyLayer;

    [Header("Sound Settings")]
    public AudioSource EmptyGunAudioSource;                     // 총알 없을 때 소리
    public AudioClip EmptyGunSound;                             // 총알 없을 때 소리 클립

    [Header("UI 설정")]
    public GameObject bulletIconPrefab;                             // 에디터에서 할당할 총알 아이콘 프리팹
    public Transform ammoContainer;                                 // 총알 아이콘들이 생성될 부모 컨테이너
    private List<GameObject> bulletIcons = new List<GameObject>();  // 생성된 총알 아이콘들을 관리할 리스트
    //public UnityEngine.UI.Image ammoBar;
    public TMP_Text ammoBarText;


    void Awake()
    {
        originPos = currentGun.GetLocalPosition();
        theCamera = Camera.main;
        /* 현재 총에 대한 정해진 자리로 배치해주는 코드, 이후 총 스왑 기능과 함께 추가할 것*/
        // if (currentGun != null)
        //     originPos = currentGun.transform.localPosition;
        // else
        //     Debug.LogWarning("No Gun Equipped");
    }

    void Start()
    { 
        SetupAmmoUI();
    }

    void Update()
    {
        if (currentGun == null)
        {
            Debug.LogWarning("No Gun Equipped");
            return;
        }

        if (!PauseUI.activeInHierarchy)
        {
            GunFireRateCalc();
            TryFire();
            TryReload();
            TryFineSight();
            UpdateBulletUI();
        }
    }

    /* Set 함수 */
    public void SetIsReloading(bool value)
    {
        isReloading = value;
    }

    /* 연사 속도 계산 */
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime;
    }

    /* 발사 시도 */
    private void TryFire()
    {
        if (Input.GetKeyDown(keyBinding.GunFire) && currentFireRate <= 0 && !isReloading)
        {
            Fire();
        }
    }

    /* 재장전 시도 */
    private void TryReload()
    {
        if (Input.GetKey(keyBinding.GunReload) && isReloading && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            StartCoroutine(ReloadCountine());
        }
    }

    /* 총알 발사 전 계산*/
    private void Fire()
    {
        if (!isReloading
            && currentFireRate <= 0
            && !playerMovement.GetIsRun()
            )
        {
            if (currentGun.currentBulletCount <= 0)
            {
                StartCoroutine(ReloadCountine());
            }
            else
                Shoot();
        }
    }

    /* 총알 발사 후 계산 */
    private void Shoot()
    {
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate;  // 연사 속도 재계산
        currentGun.muzzleFlash.Play();
        Hit();
        PlaySE(currentGun.fireSound);

        StopAllCoroutines();
        StartCoroutine(RetroActionCoroutine());
    }

    private void Hit()
    {
        if (Physics.Raycast(theCamera.transform.position, theCamera.transform.forward, out hitInfo, currentGun.range))
        {
            // 피격한 오브젝트의 태그가 Enemy일 경우 실행
            //Debug.Log($"Hit: {hitInfo.transform.name}");
            if (hitInfo.transform != null && hitInfo.transform.tag == "Enemy")
            {
                GameObject effect = Instantiate(hitEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(effect, ps.main.duration);
                }
                else
                {
                    Destroy(effect, 2f); // 예비: 파티클 없으면 2초 후 삭제
                }
                hitInfo.transform.GetComponent<ZombeMovement>()?.TakeDamage(currentGun.damage);
            }
        }
    }

    /* 정조준 시도 */
    private void TryFineSight()
    {
        if (Input.GetKeyDown(keyBinding.GunFineSight))
        {
            FineSight();
        }
        if (Input.GetKeyUp(keyBinding.GunFineSight))
        {
            FineSight();
        }
    }

    /* 정조준 */
    private void FineSight()
    {
        isFineSight = !isFineSight;
        currentGun.anim.SetBool("AimPose", isFineSight);
    }

    /* 반동 코루틴 */
    IEnumerator RetroActionCoroutine()
    {
        Vector3 recoilBack = new Vector3(originPos.x, originPos.y, originPos.z + currentGun.retroActionForce);
        Vector3 retroActionRecoilBack = new Vector3(originPos.x, originPos.y, originPos.z + currentGun.retroActionFineSightForce);

        currentGun.anim.enabled = false;
        Vector3 nowRecoilBack = !isFineSight ? recoilBack : retroActionRecoilBack;
        currentGun.transform.localPosition = originPos;

        // 반동
        while (currentGun.transform.localPosition.z >= nowRecoilBack.z + 0.02f)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, nowRecoilBack, 0.4f);
            yield return null;
        }
        // 원위치
        while (Vector3.Distance(currentGun.transform.localPosition, originPos) > 0.01)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
            yield return null;
        }
        currentGun.anim.enabled = true;
    }

    /* 재장전 */
    IEnumerator ReloadCountine()
    {
        if (currentGun.carryBulletCount <= 0)
        {
            EmptyGunAudioSource.PlayOneShot(EmptyGunSound);
        }
        else
        {
            isReloading = true;
            currentGun.anim.SetTrigger("Reload");
            PlaySE(currentGun.reloadSound);

            yield return new WaitForSeconds(currentGun.reloadTime);

            // 현재 남은 장틴수를 고려하여 재장전
            int neededBullets = currentGun.reloadBulletCount - currentGun.currentBulletCount;
            if (currentGun.carryBulletCount >= neededBullets)
            {
                currentGun.currentBulletCount += neededBullets;
                currentGun.carryBulletCount -= neededBullets;
            }
            else
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;
            }
        }
    }

    /* 사운드 재생 */
    private void PlaySE(AudioClip _clip)
    {
        AudioSource.PlayClipAtPoint(
                _clip,
                transform.TransformPoint(currentGun.fireSoundAudio.transform.localPosition),  // 오디오 소스 위치
                currentGun.fireVolume
        );
    }

    void SetupAmmoUI()
    {
        ammoBarText.text = $"{currentGun.currentBulletCount} / {currentGun.carryBulletCount}";

        // UI 관련 설정
        // for (int i = 0; i < currentGun.maxBulletCount; i++)
        // {
        //     GameObject icon = Instantiate(bulletIconPrefab, ammoContainer);
        //     bulletIcons.Add(icon);
        // }
    }

    private void UpdateBulletUI()
    {
        ammoBarText.text = $"{currentGun.currentBulletCount} / {currentGun.carryBulletCount}";
        // // 기존 아이콘 제거
        // foreach (Transform child in ammoContainer)
        // {
        //     Destroy(child.gameObject);
        // }

        // // 현재 소유한 총알 수만큼 아이콘 생성
        // for (int i = 0; i < currentGun.carryBulletCount; i++)
        // {
        //     Instantiate(bulletIconPrefab, ammoContainer);
        // }
    }
}