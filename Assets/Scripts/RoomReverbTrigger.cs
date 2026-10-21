using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class RoomReverbTrigger : MonoBehaviour
{
    [Header("FMOD Snapshot")]
    [Tooltip("Gán Snapshot Reverb từ FMOD vào đây (ví dụ: snapshot:/SmallRoom)")]
    public EventReference reverbSnapshotEvent;

    private EventInstance roomReverbInstance;

    private void Start()
    {
        // Khởi tạo Instance của Reverb Snapshot khi GameObject xuất hiện
        if (!reverbSnapshotEvent.IsNull)
        {
            roomReverbInstance = RuntimeManager.CreateInstance(reverbSnapshotEvent);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Khi Player bước vào vùng Trigger -> Bật Reverb
        if (other.CompareTag("Player") && roomReverbInstance.isValid())
        {
            roomReverbInstance.start();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Khi Player bước ra khỏi vùng Trigger -> Tắt Reverb (Fade out mượt mà)
        if (other.CompareTag("Player") && roomReverbInstance.isValid())
        {
            roomReverbInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    private void OnDestroy()
    {
        // Dừng và giải phóng bộ nhớ FMOD khi xóa object hoặc chuyển Scene
        if (roomReverbInstance.isValid())
        {
            roomReverbInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            roomReverbInstance.release();
        }
    }
}