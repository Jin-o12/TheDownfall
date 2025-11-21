using UnityEngine;

[CreateAssetMenu(fileName = "KeyBinding", menuName = "Settings/Input")]
public class KeyBinding : ScriptableObject
{
    [Header("Player Movement")]
    public KeyCode playerMoveUp = KeyCode.W;
    public KeyCode playerMoveDown = KeyCode.S;
    public KeyCode playerMoveLeft = KeyCode.A;
    public KeyCode playerMoveRight = KeyCode.D;

    public KeyCode playerSprint = KeyCode.LeftShift;
    public KeyCode playerCrouch = KeyCode.LeftControl;
    public KeyCode playerJump = KeyCode.Space;

    [Header("Gun Control")]
    public KeyCode GunFire = KeyCode.Mouse0; // 좌클릭
    public KeyCode GunFineSight = KeyCode.Mouse1;  // 우클릭
    public KeyCode GunReload = KeyCode.R;
    public KeyCode GunSwap = KeyCode.Q;
    public KeyCode GunDrop = KeyCode.F;
    public KeyCode GunInventory = KeyCode.Tab;

    [Header("Game Control")]    
    public KeyCode GamePause = KeyCode.Escape;
}
