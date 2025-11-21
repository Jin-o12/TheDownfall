using UnityEngine;

public class SetTargetTexture : MonoBehaviour
{
    public RenderTexture targetRenderTexture;

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null && targetRenderTexture != null)
        {
            cam.targetTexture = targetRenderTexture;
        }
    }
}