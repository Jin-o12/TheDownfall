using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float walkSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float crouchSpeed;
    float applySpeed;
    float moveDir;
    float maxSpeed;
    [SerializeField] float jumpForce;

    [Header("State Settings")]
    bool isRun = false;
    bool isCrouch = false;
    bool isGround = true;

    [Header("crouch Settings")]
    [SerializeField] float crouchPosY;
    float originPosY;
    float applyCrouchPosY;
    float originHeight;     // 앉았을 때 콜라이더 크기 조정
    [SerializeField] float crouchHeight;

    [Header("Key Settings")]
    [SerializeField] KeyBinding keyBinding;

    [Header("Look Settings")]
    // 민감도
    [SerializeField] float lookSeneitivity;

    // 카메라 한계
    [SerializeField] float cameraRotationLimit;
    float currentCameraRotationX = 0;

    // 필요한 컴포넌트
    [SerializeField] Camera theCamera;
    Rigidbody myRigid;
    CapsuleCollider capsuleCollider;
    Coroutine crouchCoroutine;
    Coroutine crouchColliderCoroutine;
    [SerializeField] Animator animator;
    [SerializeField] PlayerManagement playerManagement;

    [Header("Audio Files")]
    public AudioClip _jumpSound;      // 점프 소리
    public AudioClip _hitSound;       // 피격 소리
    public AudioClip _ReroadSound;    // 사망 소리

    public AudioSource audioSource;

    [HideInInspector]public Vector3 cameraPosition;

    /* 발소리 구현 추가 코드 */
    // 발걸음 오디오 설정
    [Header("Footstep Audio")]
    public AudioClip[] FootstepAudioClips;
    public AudioClip[] WaterstepAudioClips;
    [Range(0, 1)] public float FootstepVolume = 0.8f;
    public float minFootstepIntervalSec;    // 발소리 재생 최소시간
    public float minSpeedForSteps;          // 발소리 사이 최소 시간 (겹침 방지)
    public float walkStepRange;
    public float runStepRange;
    public float minIntervalWalk;           // 걷기 최소 간격 상한
    public float minIntervalRun;            // 달리기 최소 간격 상한

    // 플레이어 이동
    private Vector3 _lastPos;
    private float _distanceAccum;   //걸어간 길이
    private float _lastStepTime;    //마지막 발소리가 재생 된 (절대적)시간
    public CapsuleCollider _controller;

    // Get Function
    public float GetMoveDir() { return moveDir; }
    public float GetMaxSpeed() { return maxSpeed; }
    public void SetMaxSpeed(float maxSpeed) { this.maxSpeed = maxSpeed; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myRigid = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;
        originHeight = capsuleCollider.height;
    }

    void Awake()
    {
        // 플레이어 이동 거리 수치 초기화
        _lastPos = transform.position;
        _distanceAccum = 0.0f;
    }

    public bool GetIsCrouch()
    {
        return isCrouch;
    }

    public bool GetIsRun()
    {
        return isRun;
    }

    public bool GetIsJumping()
    {
        return !isGround;
    }

    // Update is called once per frame
    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        CharacterRotation();
        CameraRotation();
        
        //PlayFootstepSound(); 맵 변경으로 에러가 심함, 잠시 보류
    }

    void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }

    void TryJump()
    {
        if (Input.GetKeyDown(keyBinding.playerJump) && isGround)
        {
            Jump();
        }
    }

    void Jump()
    {
        if (isCrouch) Crouch();
        myRigid.linearVelocity = transform.up * jumpForce;

        if (_jumpSound)
        {
            audioSource.clip = _jumpSound;
            audioSource.Play();
        }
        else
        {
            print("Missig jump sound.");
        }
}

    void TryCrouch()
    {
        if (Input.GetKeyDown(keyBinding.playerCrouch))
        {
            Crouch();
        }
        else if (Input.GetKeyUp(keyBinding.playerCrouch))
        {
            if (isCrouch)
            {
                Crouch();
            }
        }

    }

    void Crouch()
    {
        isCrouch = !isCrouch;

        if (crouchCoroutine != null)
        {
            StopCoroutine(crouchCoroutine);
        }

        if (crouchColliderCoroutine != null)
        {
            StopCoroutine(crouchColliderCoroutine);
        }

        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        crouchCoroutine = StartCoroutine(CrouchCoroutine());
        crouchColliderCoroutine = StartCoroutine(CrouchColliderCoroutine());
    }

    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15) break;
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0);
    }

    IEnumerator CrouchColliderCoroutine()
    {
        float targetHeight = isCrouch ? crouchHeight : originHeight;
        Vector3 targetCenter = isCrouch ? new Vector3(0, -0.5f, 0) : new Vector3(0, -0.1f, 0);

        while (Mathf.Abs(capsuleCollider.height - targetHeight) > 0.01f || Vector3.Distance(capsuleCollider.center, targetCenter) > 0.01f)
        {
            capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, targetHeight, 0.3f);
            capsuleCollider.center = Vector3.Lerp(capsuleCollider.center, targetCenter, 0.3f);
            yield return null;
        }

        capsuleCollider.height = targetHeight;
        capsuleCollider.center = targetCenter;
    }

    void TryRun()
    {
        if (Input.GetKeyDown(keyBinding.playerSprint) && moveDir > 0)
        {
            Running();
        }
        else if (Input.GetKeyUp(keyBinding.playerSprint) || moveDir == 0)
        {
            RunningCancel();
        }
    }

    void Running()
    {
        if (isCrouch) Crouch();
        isRun = true;
        applySpeed = runSpeed;
        playerManagement.StartCoroutine(playerManagement.UseStemina());
        animator.SetBool("Run", true);
    }

    void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
        animator.SetBool("Run", false);
    }

    void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        moveDir = _velocity.magnitude;

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

        if (moveDir > 0)
        {
            animator.SetBool("Walk", true);
        }
        else
        {
            animator.SetBool("Walk", false);
        }
    }

    void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSeneitivity;

        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }

    void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRoationX = _xRotation * lookSeneitivity;
        currentCameraRotationX -= _cameraRoationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    /* 플레이어 지면(을 따라 설치된 콜라이더 충돌)에 따라 발걸음 소리 재생 */
    void PlayFootstepSound()
    {
        // 수평 이동량 계산
        Vector3 nowPos = transform.position;
        Vector3 delta = nowPos - _lastPos;
        delta.y = 0f;
        float dist = delta.magnitude;
        float speed = dist / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastPos = nowPos;

        // 바닥을 밟고있지 않거나 최소 걸음수를 만족하지 않으면 재생하지 않음
        if (!isGround || speed < minSpeedForSteps) return;

        float stepDistance = isRun ? runStepRange : walkStepRange;

        // 이번 프레임에 수평으로 이동한 거리 합산 및 한걸음 체크
        _distanceAccum += dist;
        if (_distanceAccum < stepDistance) return;

        // 걷기-달리기 부드러운 전환
        float t = Mathf.InverseLerp(walkSpeed, runSpeed, speed);
        float minInterval = Mathf.Lerp(minIntervalWalk, minIntervalRun, t);
        if (Time.time - _lastStepTime < minInterval) return;

        _distanceAccum = 0.0f;
        _lastStepTime = Time.time;

        // 발소리 종류 지정
        AudioClip[] stepAudio = null;
        switch (playerManagement.nowSurfaceType)
        {
            case PlayerManagement.SurfaceType.Default:
                stepAudio = FootstepAudioClips;
                break;
            case PlayerManagement.SurfaceType.Water:
                stepAudio = WaterstepAudioClips;
                break;
        }

        // 오디오 클립 재생
        if (stepAudio != null)
        {
            var index = Random.Range(0, stepAudio.Length);
            AudioSource.PlayClipAtPoint(
                stepAudio[index],
                transform.TransformPoint(audioSource.transform.localPosition),  // 오디오 소스 위치
                FootstepVolume
            );
        }
    }
}
