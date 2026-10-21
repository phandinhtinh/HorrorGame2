using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    private CharacterController controller;

    [Header("Noise Settings")]
    public float walkNoiseRadius = 5.0f;
    public float runNoiseRadius = 12.0f;
    public float stepInterval = 0.5f;   // Thời gian giữa 2 bước chân khi đi bộ
    public float runStepInterval = 0.3f; // Nhịp bước chân nhanh hơn khi chạy

    private float stepTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller != null && controller.isGrounded && controller.velocity.magnitude > 0.2f)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float currentInterval = isRunning ? runStepInterval : stepInterval;

            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0)
            {
                EmitFootstepNoise(isRunning);
                stepTimer = currentInterval;
            }
        }
        else
        {
            stepTimer = 0f;

            // Ngắt âm thanh bước chân FMOD ngay lập tức khi dừng di chuyển hoặc rời mặt đất
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopFootstep();
            }
        }
    }

    void EmitFootstepNoise(bool isRunning)
    {
        RaycastHit hit;
        float currentNoiseRadius = isRunning ? runNoiseRadius : walkNoiseRadius;

        // Bắn Raycast xuống chân kiểm tra bề mặt
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
        {
            if (hit.collider.CompareTag("Carpet"))
            {
                currentNoiseRadius = 0f;
            }
            else if (hit.collider.CompareTag("BrokenGlass"))
            {
                currentNoiseRadius *= 2.0f;
            }
        }

        if (currentNoiseRadius > 0)
        {
            // Truyền trạng thái chạy/đi bộ sang AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayFootstep(isRunning);
            }

            NoiseManager.EmitNoise(transform.position, currentNoiseRadius);
            Debug.Log($"Phát tiếng động! Bán kính: {currentNoiseRadius}m | Chạy: {isRunning}");
        }
        else
        {
            // Đi trên thảm (NoiseRadius = 0) -> Dừng âm bước chân
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopFootstep();
            }
        }
    }
}