using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Khoảng cách tối đa từ mắt người chơi tới cửa/vật thể (nên để 3 - 5)")]
    public float interactDistance = 3.5f;
    public LayerMask interactLayer = ~0; // Mặc định quét tất cả các Layer
    public Camera playerCamera;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // 1. Tự động lấy Main Camera nếu chưa kéo vào Inspector
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("[PlayerInteraction] LỖI: Chưa gán Camera vào PlayerInteraction HOẶC Camera của bạn chưa đặt Tag là 'MainCamera'!");
                return;
            }
        }

        // Tạo tia Ray từ giữa màn hình Camera
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Vẽ tia màu đỏ trong tab Scene (kéo dài 2 giây) để bạn thấy hướng bắn của tia
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red, 2.0f);

        // 2. Thực hiện Raycast
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            Debug.Log("[PlayerInteraction] Tia E đã bắn trúng Object: " + hit.collider.gameObject.name);

            // Tương tác với CỬA (nhận diện cả DoorController thường lẫn MainDoorController)
            DoorController door = hit.collider.GetComponentInParent<DoorController>();
            if (door != null)
            {
                Debug.Log("[PlayerInteraction] Tìm thấy DoorController trên: " + door.gameObject.name + " -> Gọi Interact()");
                door.Interact();
                return;
            }

            // Tương tác với CHÌA KHÓA
            KeyItem key = hit.collider.GetComponentInParent<KeyItem>();
            if (key != null)
            {
                Debug.Log("[PlayerInteraction] Tìm thấy KeyItem -> Nhặt chìa!");
                key.PickUpKey();
                return;
            }

            // Nếu bắn trúng vật thể khác không phải Cửa hay Chìa
            Debug.LogWarning("[PlayerInteraction] Object " + hit.collider.gameObject.name + " không chứa DoorController hay KeyItem!");
        }
        else
        {
            Debug.Log("[PlayerInteraction] Bấm E nhưng tia Raycast không bắn trúng bất kỳ vật thể nào! (Hãy đứng gần cửa hơn)");
        }
    }
}