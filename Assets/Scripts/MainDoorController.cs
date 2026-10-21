using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainDoorController : DoorController
{
    [Header("Lock & Key Settings")]
    public bool isLocked = true;
    public KeyItem requiredKey;

    [Header("Win Settings")]
    [Tooltip("Thời gian chờ (giây) cho cửa mở xong rồi mới hiện Win Panel")]
    public float winDelay = 1.0f;

    public override void Interact()
    {
        if (isLocked)
        {
            if (requiredKey != null && requiredKey.isPickedUp)
            {
                isLocked = false;
                Debug.Log("Đã dùng chìa khóa mở Main Door!");
                ToggleDoor();
                StartCoroutine(TriggerWinWithDelay());
            }
            else
            {
                Debug.Log("Main Door đang bị khóa! Cần tìm chìa khóa.");
                NoiseManager.EmitNoise(transform.position, 3.0f);
            }
        }
        else
        {
            ToggleDoor();
            StartCoroutine(TriggerWinWithDelay());
        }
    }

    private IEnumerator TriggerWinWithDelay()
    {
        // Chờ thời gian mở cửa hoàn tất
        yield return new WaitForSeconds(winDelay);

        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.ShowWinPanel();
        }
    }
}