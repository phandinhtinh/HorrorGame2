using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AudioDetectorDisplay : MonoBehaviour
{
    public TextMeshProUGUI meterText;
    public CharacterController playerController;

    private float currentSignal = 0f;

    void Update()
    {
        float targetSignal = 0f;

        // Tính toán mức độ tiếng động dựa vào trạng thái di chuyển
        if (playerController.isGrounded && playerController.velocity.magnitude > 0.2f)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            targetSignal = isRunning ? Random.Range(75f, 95f) : Random.Range(25f, 40f);
        }
        else
        {
            // Nhịp nhiễu sóng nhẹ khi đứng yên
            targetSignal = Random.Range(2f, 8f);
        }

        currentSignal = Mathf.Lerp(currentSignal, targetSignal, Time.deltaTime * 8f);

        if (meterText != null)
        {
            meterText.text = $"FREQ: 44.1kHz\nNOISE: {currentSignal:F1} dB";

            // Đổi màu chữ sang đỏ nếu phát tiếng động lớn
            if (currentSignal > 60f)
                meterText.color = Color.red;
            else
                meterText.color = Color.green;
        }
    }
}