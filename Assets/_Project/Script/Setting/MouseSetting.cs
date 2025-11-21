using UnityEngine;

/*
    * 마우스 이동에 따른 카메라 회전
    * 마우스 감도 조절
    * 커서 잠금 및 숨김
    * 헤드밥 효과
    * 무기 회전
    * 카메라 및 무기 회전의 부드러운 적용
    * FPS 표시 기능
    */

public class MouseSetting : MonoBehaviour
{
    [Header("Objects")]
    public Transform myCamera;

    [Header("Mouse Settings")]
    public float mouseSensitvity;


    [HideInInspector]
    public float zRotation = 0.0f;

    void Start()
    {

    }

    void Update()
    {

    }

    void MouseInputMovement()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitvity * 0.02f;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitvity * 0.02f;

        transform.Rotate(0, mouseX, 0);
        myCamera.Rotate(-mouseY, 0, 0);

        // Clamp the vertical rotation of the camera
        Vector3 currentRotation = myCamera.localEulerAngles;
        if (currentRotation.x > 180) currentRotation.x -= 360; // Convert to -180 to 180 range
        currentRotation.x = Mathf.Clamp(currentRotation.x, -90, 90); // Limit vertical rotation
        myCamera.localEulerAngles = new Vector3(currentRotation.x, 0, zRotation); // Apply zRotation for head bob effect
    }
}
