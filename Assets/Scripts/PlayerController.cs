using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4.0f;
    public float runSpeed = 6.5f;
    public float jumpHeight = 1.5f;
    public float gravity = -19.62f;

    [Header("Noise Settings")]
    public float landingNoiseRadius = 12.0f; // Bán kính tiếng động khi tiếp đất
    public float landingNoiseDuration = 0.5f; // Thời gian duy trì tiếng động (0.5s) để UI máy đo kịp chuyển màu đỏ

    [Header("Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2.0f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool wasGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        wasGrounded = controller.isGrounded;
    }

    void Update()
    {
        // 1. Xoay Camera
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // 2. Di chuyển WASD
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveInput = transform.right * x + transform.forward * z;

        // 3. Trọng lực & Nhảy
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        velocity.y += gravity * Time.deltaTime;

        // 4. Di chuyển Character Controller
        Vector3 finalVelocity = (moveInput * currentSpeed) + (Vector3.up * velocity.y);
        controller.Move(finalVelocity * Time.deltaTime);

        // 5. Kiểm tra sự kiện TIẾP ĐẤT
        if (controller.isGrounded && !wasGrounded)
        {
            // Kích hoạt duy trì tiếng động tiếp đất để UI chuyển màu đỏ
            StartCoroutine(EmitLandingNoiseRoutine());
        }

        wasGrounded = controller.isGrounded;
    }

    // Duy trì tiếng động liên tục trong 0.5s khi vừa tiếp đất
    private IEnumerator EmitLandingNoiseRoutine()
    {
        float timer = 0f;
        while (timer < landingNoiseDuration)
        {
            NoiseManager.EmitNoise(transform.position, landingNoiseRadius);
            timer += Time.deltaTime;
            yield return null; // Chờ sang frame tiếp theo
        }
    }
}