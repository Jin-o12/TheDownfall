using UnityEngine;
using UnityEngine.VFX;

public class FootstepVFXRelay : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] VisualEffect vfx;               // 캐릭터에 붙인 1개 VFX 컴포넌트(풀링/매니저도 OK)
    [SerializeField] string vfxEventName = "OnFootstepVFX"; // 그래프의 Event 이름과 일치
    [SerializeField] LayerMask groundMask = ~0;      // 발 디딜 표면 레이어
    [SerializeField] float rayLen = 0.8f;            // 발 아래로 레이 길이
    [SerializeField] float surfaceOffset = 0.01f;    // 표면에서 살짝 띄우기

    Animator _anim;
    Transform _leftFoot, _rightFoot;
    int _evtID;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _leftFoot  = _anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        _rightFoot = _anim.GetBoneTransform(HumanBodyBones.RightFoot);
        _evtID = Shader.PropertyToID(vfxEventName);
    }

    // 애니메이션 이벤트로 자동 호출 (Starter Assets가 쓰는 시그니처 그대로)
    public void OnFootstep(AnimationEvent e)
    {
        // 레이어 블렌딩 중복 호출 방지(Starter Assets 기본 코드와 동일한 패턴)
        if (e.animatorClipInfo.weight <= 0.5f) return;

        // 어떤 발에서 호출됐는지 문자열 파라미터로 구분 가능(없으면 더 낮은 발 선택)
        Transform foot = e.stringParameter == "R" ? _rightFoot :
                         e.stringParameter == "L" ? _leftFoot  :
                         (_leftFoot.position.y < _rightFoot.position.y ? _leftFoot : _rightFoot);

        if (Physics.Raycast(foot.position + Vector3.up * 0.05f, Vector3.down, out var hit, rayLen, groundMask))
        {
            using (var attr = vfx.CreateVFXEventAttribute())
            {
                attr.SetVector3("position", hit.point + hit.normal * surfaceOffset);
                attr.SetVector3("normal",   hit.normal);
                vfx.SendEvent(_evtID, attr);  // 그래프의 Event Spawn이 받도록
            }
        }
    }

    // 점프 착지 효과도 쓰고 싶다면
    public void OnLand(AnimationEvent e)
    {
        if (e.animatorClipInfo.weight <= 0.5f) return;
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        if (Physics.Raycast(origin, Vector3.down, out var hit, 2f, groundMask))
        {
            using (var attr = vfx.CreateVFXEventAttribute())
            {
                attr.SetVector3("position", hit.point + hit.normal * 0.02f);
                attr.SetVector3("normal",   hit.normal);
                vfx.SendEvent(_evtID, attr);
            }
        }
    }
}
