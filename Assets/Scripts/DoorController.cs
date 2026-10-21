using UnityEngine;
using UnityEngine.AI;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public float openAngle = -90.0f;
    public float doorSpeed = 3.0f;

    [Header("Double Door (Cửa đôi)")]
    public DoorController siblingDoor;

    protected bool isOpen = false;
    public bool IsOpen => isOpen;

    protected Quaternion closedRotation;
    protected Quaternion openRotation;
    protected NavMeshObstacle navObstacle;

    protected virtual void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        navObstacle = GetComponent<NavMeshObstacle>();
        UpdateObstacle();
    }

    protected virtual void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * doorSpeed);
    }

    public virtual void Interact()
    {
        ToggleDoor();
    }

    public virtual void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    public virtual void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;
        UpdateObstacle();

        if (siblingDoor != null && !siblingDoor.IsOpen)
        {
            siblingDoor.OpenDoor();
        }

        NoiseManager.EmitNoise(transform.position, 8.0f);
        Debug.Log("Cửa đã mở!");
    }

    public virtual void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;
        UpdateObstacle();

        if (siblingDoor != null && siblingDoor.IsOpen)
        {
            siblingDoor.CloseDoor();
        }

        NoiseManager.EmitNoise(transform.position, 8.0f);
        Debug.Log("Cửa đã đóng!");
    }

    protected void UpdateObstacle()
    {
        if (navObstacle != null)
        {
            navObstacle.enabled = !isOpen;
        }
    }
}