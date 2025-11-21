using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [SerializeField] GunController gunController;
    [SerializeField] GunManager currentGun;
    public void OnReloadEnd()
    {
        gunController.SetIsReloading(false);
    }
}
