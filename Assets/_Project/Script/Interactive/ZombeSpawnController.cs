using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ZombeSpawnController : MonoBehaviour
{
    [Header("Zombe Setting")]
    [SerializeField] public GameObject zombePrefap;
    [SerializeField] public float walkSpeed;                // 걷기 속도
    [SerializeField] public int damage;                     // 공격력
    [SerializeField] public int health;                     // 체력
    [SerializeField] public float attackCooldown;    // 공격 딜레이
    [SerializeField] public float attackRange;

    [Header("Spawn Information")]
    [SerializeField] public Transform playerTf;
    [SerializeField] public BoxCollider spawnZone;
    public int maxSpawnEntity;     // 최대 스폰 수
    private int nowSpawnEntity;     // 현재 스폰 수
    public float minRadius;         // 스폰 최소 반지름 거리
    public float maxRadius;         // 스폰 최대 반지름 거리
    public int maxAttempts = 40;

    private LayerMask groundMask = ~0;      // 바닥 판정을 위한 레이어 마스크
    private float avoidRadius = 0.6f;       // 겹침 회피
    private LayerMask avoidMask = ~0;       // 최종 위치의 충돌 검사용 구체 반경
    private float rayHeight = 50f;          // 후보지점에서 아래로 지면 탐색을 위한 Raycast 발사 높이
    private float navmeshSnapRadius = 2f;   // 바닥 스냅 지정을 NavMesh 상 위치로 보정시 허용 최대거리

    void Awake()
    {
        avoidMask = LayerMask.GetMask("Player", "Enemy", "Obstacle");
        groundMask &= ~LayerMask.GetMask("SpawnZone");

        nowSpawnEntity = 0;

        enabled = false;
    }

    void Update()
    {
        SpawmEntity();
    }

    public void DestroyZombe()
    {
        nowSpawnEntity--;
    }

    public void SpawmEntity()
    {
        if (nowSpawnEntity >= maxSpawnEntity) return;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 cand = RandomAnnulusPoint(playerTf.position, minRadius, maxRadius);

            if (!IsInsideBox(spawnZone, cand)) continue;

            if (!SnapToGround(cand, groundMask, rayHeight, out var pos)) continue;

            SnapToNavMesh(pos, navmeshSnapRadius, out pos);
            // 충돌 가능 오브젝트(avoidMask에서 구분)와 겹침 회피
            if (Physics.CheckSphere(pos, avoidRadius, avoidMask, QueryTriggerInteraction.Ignore)) continue;

            SpawnEntityWithInitialize(pos);
            return;
        }
        Debug.LogWarning("SpawnOne: 유효한 지점을 찾지 못했습니다.");
    }

    /* 플레이어 주변의 어느 방향에서 스폰할 지 선정, 이에 균일함을 위해 균일난수 사용 */
    Vector3 RandomAnnulusPoint(Vector3 center, float minR, float maxR)
    {
        // 면적 균일: r^2를 균일샘플 → r = sqrt(…)
        float r = Mathf.Sqrt(Random.Range(minR * minR, maxR * maxR));
        Vector2 dir2 = Random.insideUnitCircle.normalized;           // 단위 원에서의 방향 반환
        if (dir2.sqrMagnitude < 1e-6) dir2 = Vector2.right;  // 원점 반환 가드
        return center + new Vector3(dir2.x, 0f, dir2.y) * r;
    }

    /* 스폰 위치 후보가 스폰 존(콜라이더) 내부인지 확인 후 콜라이더의 로컬 좌표로 변환 */
    bool IsInsideBox(BoxCollider box, Vector3 worldPoint)
    {
        // 월드→로컬로 변환
        Vector3 local = box.transform.InverseTransformPoint(worldPoint) - box.center;
        Vector3 half = box.size * 0.5f;
        return Mathf.Abs(local.x) <= half.x &&
            Mathf.Abs(local.y) <= half.y &&
            Mathf.Abs(local.z) <= half.z;
    }

    // 스폰 위치에 지면이 있는지 확인
    bool SnapToGround(Vector3 candidate, LayerMask groundMask, float rayHeight, out Vector3 snapped)
    {
        Vector3 start = candidate + Vector3.up * rayHeight;
        if (Physics.Raycast(start, Vector3.down, out var hit, rayHeight * 2f, groundMask,
                        QueryTriggerInteraction.Ignore))
        {
            snapped = hit.point;
            return true;
        }
        snapped = default;
        return false;
    }

    // (선택) NavMesh 위로 보정
    bool SnapToNavMesh(Vector3 pos, float maxDist, out Vector3 snapped)
    {
        if (UnityEngine.AI.NavMesh.SamplePosition(pos, out var navHit, maxDist, UnityEngine.AI.NavMesh.AllAreas))
        { snapped = navHit.position; return true; }
        snapped = pos; return false;
    }

    // 지정된 위치에 좀비 객체 소환 및 초기화
    public void SpawnEntityWithInitialize(Vector3 pos)
    { 
        GameObject zombeObj = Instantiate(zombePrefap, pos, Quaternion.identity);

        ZombeMovement zomMov = zombeObj.GetComponent<ZombeMovement>();
        zomMov.playerTf = playerTf;
        zomMov.walkSpeed = walkSpeed;
        zomMov.damage = damage;
        zomMov.health = health;
        zomMov.zombeSpawnController = this;
        zomMov.attackCooldown = attackCooldown;
        zomMov.attackRange = attackRange;
        
        nowSpawnEntity++;
    }
}
