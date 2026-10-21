using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Key Info")]
    public string keyName = "Main Door Key";
    public bool isPickedUp = false;

    [Header("Audio (Tùy chọn)")]
    public AudioClip pickupSound;

    // Hàm gọi chung cho PlayerInteraction khi bấm E
    public void Interact()
    {
        PickUpKey();
    }

    public void PickUpKey()
    {
        if (isPickedUp) return;

        isPickedUp = true;
        Debug.Log("Đã nhặt: " + keyName);

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Ẩn chìa khỏi Scene (vẫn giữ script trong bộ nhớ để DoorController đọc biến isPickedUp)
        gameObject.SetActive(false);
    }
}