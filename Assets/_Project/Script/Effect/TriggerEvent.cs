using UnityEngine;
using UnityEngine.VFX;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField] VisualEffect vfx;
    static readonly int OnHitID  = Shader.PropertyToID("OnHit");
    static readonly int PosID    = Shader.PropertyToID("position");
    static readonly int NormalID = Shader.PropertyToID("normal");

    void OnCollisionEnter(Collision col) // isTrigger=false
    {
        var cp = col.GetContact(0); // 충돌 지점/법선
        using (var attr = vfx.CreateVFXEventAttribute())
        {
            attr.SetVector3(PosID, cp.point);
            attr.SetVector3(NormalID, cp.normal);
            vfx.SendEvent(OnHitID, attr);
        }
    }
}
