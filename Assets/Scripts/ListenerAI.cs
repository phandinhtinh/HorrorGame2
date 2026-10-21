using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FMODUnity;

public class ListenerAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Movement Speeds")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 4.0f;

    [Header("Patrol Settings")]
    public float patrolRadius = 15.0f;

    [Header("Door Interaction Settings")]
    public float doorCheckRadius = 1.5f; // Bán kính quét tìm cửa phía trước mặt quái vật

    [Header("Attack Settings")]
    [Tooltip("Khoảng cách quái phải áp sát Player mới bắt đầu vung tay")]
    public float attackRange = 1.3f;     // Khoảng cách tự động vung tay đánh khi lại gần Player

    [Tooltip("Thời gian chờ phát trọn vẹn animation Attack (tính bằng giây) trước khi dừng game")]
    public float attackDuration = 2.2f;  // Tăng/giảm ở Inspector để vừa khít với thời lượng clip Attack của bạn

    private bool isInvestigating = false;
    private bool isAttacking = false; // Biến kiểm tra tránh gọi tấn công nhiều lần
    private Vector3 currentTargetPosition;

    [Header("FMOD Audio Events")]
    public EventReference attackAudioEvent; // Biến hiển thị ô chọn Event trên Inspector

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Lấy component Animator trên GameObject này
    }

    void OnEnable()
    {
        // Đăng ký nhận sự kiện khi có tiếng động phát ra
        NoiseManager.OnNoiseEmitted += HearNoise;
    }

    void OnDisable()
    {
        // Hủy đăng ký sự kiện khi GameObject bị tắt
        NoiseManager.OnNoiseEmitted -= HearNoise;
    }

    void Start()
    {
        agent.speed = patrolSpeed;

        // Kiểm tra an toàn: Chỉ cho di chuyển nếu quái vật đã nằm đúng trên NavMesh
        if (agent != null && agent.isOnNavMesh)
        {
            MoveToRandomPatrolPoint();
        }
        else
        {
            Debug.LogWarning("The Listener chưa chạm vào vùng NavMesh! Hãy đặt quái vật sát xuống sàn màu xanh.");
        }
    }

    void Update()
    {
        // Nếu đang trong trạng thái tấn công thì dừng mọi logic tuần tra/mở cửa
        if (isAttacking) return;

        // Tự động kiểm tra khoảng cách: Nếu Player vào tầm attackRange thì áp sát tấn công ngay
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && Vector3.Distance(transform.position, playerObj.transform.position) <= attackRange)
        {
            StartCoroutine(PerformAttack(playerObj.transform));
            return;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            // Tự động kiểm tra và mở cửa nếu tiếp cận cửa đang đóng
            CheckAndOpenDoors();

            // Cập nhật trạng thái Animation (Idle <-> Walk)
            UpdateAnimation();

            // Nếu không đi điều tra tiếng động và đã đến nơi -> Chọn điểm tuần tra mới
            if (!isInvestigating && !agent.pathPending && agent.remainingDistance < 0.5f)
            {
                MoveToRandomPatrolPoint();
            }
        }
        else if (animator != null)
        {
            // Trả về Idle nếu agent không nằm trên NavMesh hoặc bị đứng yên
            animator.SetBool("isWalking", false);
        }
    }

    // =======================================================
    // CẬP NHẬT TRẠNG THÁI ANIMATION
    // =======================================================
    void UpdateAnimation()
    {
        if (animator != null)
        {
            // Nếu vận tốc di chuyển thực tế > 0.1 -> Bật trạng thái isWalking (Walk), ngược lại tắt (Idle)
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isMoving);
        }
    }

    // =======================================================
    // TỰ ĐỘNG DÒ VÀ ĐẨY MỞ CỬA KHI TRUY ĐUỔI
    // =======================================================
    void CheckAndOpenDoors()
    {
        // Quét một vùng cầu phía trước mặt quái vật
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * 0.8f, doorCheckRadius);
        foreach (var hitCollider in hitColliders)
        {
            DoorController door = hitCollider.GetComponentInParent<DoorController>();
            if (door != null && !door.IsOpen)
            {
                Debug.Log("<color=orange>The Listener tự tay đẩy mở cửa!</color>");
                door.OpenDoor(); // Quái vật tự động đẩy mở cửa

                // Cập nhật lại đường đi ngay lập tức qua ô cửa vừa mở
                if (isInvestigating)
                {
                    agent.SetDestination(currentTargetPosition);
                }
            }
        }
    }

    // =======================================================
    // XỬ LÝ ÂM THANH & TUẦN TRA
    // =======================================================
    void HearNoise(Vector3 noisePosition, float noiseRadius)
    {
        if (isAttacking) return;

        float distanceToNoise = Vector3.Distance(transform.position, noisePosition);

        // Nếu khoảng cách từ quái vật đến nguồn âm nhỏ hơn bán kính tiếng động
        if (distanceToNoise <= noiseRadius)
        {
            Debug.Log("<color=red>The Listener nghe thấy tiếng động! Đang lao tới...</color>");

            isInvestigating = true;
            currentTargetPosition = noisePosition;
            agent.speed = chaseSpeed;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(noisePosition); // Lao tới vị trí phát ra tiếng động
            }

            // Reset thời gian: Sau 6 giây nếu không nghe thêm tiếng động sẽ quay lại tuần tra
            CancelInvoke(nameof(ResumePatrol));
            Invoke(nameof(ResumePatrol), 6.0f);
        }
    }

    void ResumePatrol()
    {
        if (isAttacking) return;

        Debug.Log("<color=yellow>The Listener mất dấu tiếng động, quay lại tuần tra.</color>");
        isInvestigating = false;
        agent.speed = patrolSpeed;

        if (agent != null && agent.isOnNavMesh)
        {
            MoveToRandomPatrolPoint();
        }
    }

    void MoveToRandomPatrolPoint()
    {
        if (agent == null || !agent.isOnNavMesh || isAttacking) return;

        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // =======================================================
    // XỬ LÝ TẤN CÔNG & KHÓA DI CHUYỂN PLAYER
    // =======================================================
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isAttacking)
        {
            StartCoroutine(PerformAttack(other.transform));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isAttacking)
        {
            StartCoroutine(PerformAttack(collision.transform));
        }
    }

    IEnumerator PerformAttack(Transform playerTransform)
    {
        isAttacking = true;

        // 1. Dừng di chuyển quái vật ngay lập tức
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // 2. KHÓA DI CHUYỂN PLAYER NGAY LẬP TỨC (Người chơi đứng chôn chân, không thể chạy trốn)
        MonoBehaviour playerController = playerTransform.GetComponent("PlayerController") as MonoBehaviour;
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        // 3. Quay mặt quái vật trực diện về phía Player
        Vector3 lookDirection = playerTransform.position - transform.position;
        lookDirection.y = 0; // Giữ nguyên độ cao nằm ngang
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // 4. Kích hoạt Animation Attack
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 5. Chờ quái vung tay ĐÁNH XONG TRỌN VẸN animation (Thời gian chạy bình thường)
        yield return new WaitForSeconds(attackDuration);

        // 6. Đánh trúng & xong hẳn động tác mới khóa thời gian Game Over
        GameOver();
    }

    void GameOver()
    {
        Debug.LogError("BẠN ĐÃ BỊ THE LISTENER BẮT! GAME OVER!");
        Time.timeScale = 0f; // Dừng thời gian toàn bộ game khi bị bắt
    }
}