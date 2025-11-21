using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ZombeMovement : MonoBehaviour
{
    [Header("플레이어 정보")]
    public PlayerManagement playerAction;     // playerState 스크립트
    public Transform playerTf;          // Transform
    public LayerMask playerLayer;       // 레이어
    public ZombeSpawnController zombeSpawnController;

    [Header("좀비 정보")]
    public float walkSpeed;
    public Animator animator;
    public float attackRange;

    [Header("좀비 상태")]
    public bool isDead = false;
    public int health;
    public int damage;

    [Header("플레이어 공격")]
    public float rotateSpeed = 720f;    // 공격 시 방향 회전 속도
    private bool _isAttacking = false;
    public float attackCooldown;
    private float attackCooldowmTimer;

    NavMeshAgent enemyAge;

    enum ZombeState
    {
        Roam,
        Chase,
        Attack
    }

    private ZombeState zombeState;

    void Awake()
    {
        enemyAge = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (playerTf != null)
        {
            playerAction = playerTf.GetComponent<PlayerManagement>();
        }
        if (playerAction == null)
        {
            Debug.LogError("PlayerAction 컴포넌트를 플레이어에게서 찾을 수 없습니다");
        }

        enemyAge.speed = walkSpeed;
        enemyAge.stoppingDistance = attackRange * 0.9f; // 여유 거리에서 정지

        zombeState = ZombeState.Roam;
        attackCooldowmTimer = 0f;
    }

    void Update()
    {
        //MoveToPlayer();
        PlayerAttack();
    }

    /* 플레이어 자동 추적 및 이동 */
    void MoveToPlayer()
    {
        enemyAge.SetDestination(playerTf.position);
        // 실제 좀비 이동과 애니메이션 속도 일치시키기 위해 에이전트 속도 사용
        float actualSpeed = enemyAge.velocity.magnitude;
        animator.SetFloat("Speed", actualSpeed);
    }

    void PlayerAttack()
    {
        Vector3 playerPos = playerTf.position;
        Vector3 zombePos = transform.position;

        // Y축 값을 0으로 강제해서 2D 평면 위치로 만듦
        playerPos.y = 0f;
        zombePos.y = 0f;

        // 2D 평면상의 거리를 계산
        float distance = Vector3.Distance(zombePos, playerPos);

        if (distance <= enemyAge.stoppingDistance)  // 공격 상태
        {
            enemyAge.isStopped = true;
            animator.SetFloat("Speed", 0f);
            FaceTarget(playerTf.position);

            Debug.Log("플레이어가 공격 범위 안에 있음");
            if (!_isAttacking && attackCooldown <= attackCooldowmTimer)
            {
                Debug.Log("플레이어 공격");
                animator.SetTrigger("Attack");
                _isAttacking = true;
            }
        }
        else    // 추격 상태
        {
            enemyAge.isStopped = false;

            enemyAge.SetDestination(playerTf.position);

            float actualSpeed = enemyAge.velocity.magnitude;
            animator.SetFloat("Speed", actualSpeed);
        }

        attackCooldowm();
    }

    private void FaceTarget(Vector3 lookPos)
    {
        Vector3 dir = (lookPos - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        var targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }

    public void OnAttackHit()
    {
        if (playerAction == null) return;

        float distance = Vector3.Distance(transform.position, playerTf.position);
        if (distance <= attackRange + 0.7f) // 약간의 여유 범위
        {
            playerAction.TakeDamage(damage);
            Debug.Log($"Zombie attacked player for {damage} damage.");
        }
    }

    /* 공격 모션 끝 */
    public void OnAttackEnd()
    {
        _isAttacking = false;
        attackCooldowmTimer = 0f;
    }

    public void attackCooldowm()
    {
        attackCooldowmTimer += Time.deltaTime;
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        health -= dmg;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetBool("Die", true);
        enemyAge.isStopped = true;
        GetComponent<Collider>().enabled = false;
        enabled = false;
        Destroy(gameObject, 5f);

        // 점수 및 킬 카운트 업데이트
        InGameUI inGameUI = FindFirstObjectByType<InGameUI>();
        inGameUI.UpdateNormalKillCount(1);

        // 좀비 스폰 컨트롤러에 좀비 파괴 알림
        if (zombeSpawnController != null)
        {
            zombeSpawnController.DestroyZombe();
        }
    }
}
