using UnityEngine;

/// <summary>
/// NPCBase - Lớp cơ sở cho tất cả NPC có thể tương tác
/// Các NPC cụ thể sẽ kế thừa lớp này
/// </summary>
public abstract class NPCBase : MonoBehaviour
{
    [Header("=== THÔNG TIN NPC ===")]
    public string tenNPC = "NPC";
    public string hanhDongTuongTac = "nói chuyện";
    public bool coTheTuongTac = true;

    [Header("=== QUAY VỀ PHÍA PLAYER ===")]
    public bool quayVePhiaPlayer = true;
    public float tocDoQuay = 5f;

    // --- Private ---
    protected Transform _playerTransform;
    private bool _dangTuongTac = false;

    // --- Cache: lưu rotation ban đầu ĐÚNG của root (X, Z) và children (local) ---
    // QUAN TRỌNG: Không được reset X, Z vì model có thể cần Z=90 để đứng thẳng
    private float _rootInitialX;   // X của root khi scene load (giữ nguyên mãi)
    private float _rootInitialZ;   // Z của root khi scene load (giữ nguyên mãi)
    private Quaternion[] _childOriginalLocalRots; // Local rotation của children
    private Transform[] _directChildren;          // Danh sách children trực tiếp

    protected virtual void Awake()
    {
        // Bước 1: Tắt Animator root motion để animation không di chuyển transform
        var anims = GetComponentsInChildren<Animator>(true);
        foreach (var anim in anims)
            anim.applyRootMotion = false;

        // Bước 2: Vô hiệu hóa CharacterController vì nó bypass isKinematic
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        foreach (var c in GetComponentsInChildren<CharacterController>(true))
            c.enabled = false;

        // Bước 3: Cache FULL rotation của root AS-IS (KHÔNG THAY ĐỔI GÌ!)
        // Model có thể cần Z=90 hoặc X=-90 để đứng thẳng - ta phải tôn trọng điều đó
        Vector3 rootEuler = transform.eulerAngles;
        _rootInitialX = rootEuler.x;
        _rootInitialZ = rootEuler.z;
        Debug.Log($"[NPCBase] {gameObject.name}: Cache root rotation X={_rootInitialX:F1}, Y={rootEuler.y:F1}, Z={_rootInitialZ:F1}");

        // Bước 4: Cache local rotation của tất cả children trực tiếp AS-IS
        _directChildren = new Transform[transform.childCount];
        _childOriginalLocalRots = new Quaternion[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {
            _directChildren[i] = child;
            _childOriginalLocalRots[i] = child.localRotation; // Cache nguyên xi không sửa
            i++;
        }

        // Bước 5: Khóa Rigidbody
        FreezeAllRigidbodies();
    }

    public void FreezeAllRigidbodies()
    {
        var allRbs = GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in allRbs)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        KhoaTrucThangDung();
    }

    protected virtual void Start()
    {
        var player = FindObjectOfType<StarterAssets.FirstPersonController>();
        if (player != null)
            _playerTransform = player.transform;
        else
        {
            var playerOld = FindObjectOfType<PlayerMovement>();
            if (playerOld != null)
                _playerTransform = playerOld.transform;
        }
    }

    protected virtual void Update()
    {
        if (_dangTuongTac && quayVePhiaPlayer && _playerTransform != null)
            QuayVePhiaPlayer();
        else
            KhoaTrucThangDung();
    }

    protected virtual void LateUpdate()
    {
        // Khóa thêm lần nữa sau khi Animator update
        KhoaTrucThangDung();
    }

    protected virtual void FixedUpdate()
    {
        KhoaTrucThangDung();
    }

    /// <summary>
    /// Khóa rotation về đúng pose ban đầu.
    /// Chỉ cho phép Y thay đổi (hướng nhìn), X và Z giữ nguyên như scene-authoring.
    /// </summary>
    private void KhoaTrucThangDung()
    {
        // Lấy Y hiện tại (hướng nhìn) để giữ lại
        float currentY = transform.eulerAngles.y;

        // Khôi phục X và Z từ cache (đây là pose đúng từ scene)
        // Chỉ Y được phép thay đổi
        transform.rotation = Quaternion.Euler(_rootInitialX, currentY, _rootInitialZ);

        // Khôi phục local rotation của children trực tiếp
        if (_directChildren != null)
        {
            for (int i = 0; i < _directChildren.Length; i++)
            {
                if (_directChildren[i] != null)
                    _directChildren[i].localRotation = _childOriginalLocalRots[i];
            }
        }
    }

    public void BatDauTuongTac()
    {
        if (!coTheTuongTac) return;
        _dangTuongTac = true;
        OnTuongTac();
    }

    protected abstract void OnTuongTac();

    protected void KetThucTuongTac()
    {
        _dangTuongTac = false;
    }

    private void QuayVePhiaPlayer()
    {
        Vector3 huong = _playerTransform.position - transform.position;
        huong.y = 0;
        if (huong.sqrMagnitude > 0.001f)
        {
            // Tính góc Y mục tiêu
            float targetY = Quaternion.LookRotation(huong, Vector3.up).eulerAngles.y;
            float smoothY = Mathf.LerpAngle(transform.eulerAngles.y, targetY, tocDoQuay * Time.deltaTime);

            // Áp dụng quay: giữ X và Z từ cache, chỉ đổi Y
            transform.rotation = Quaternion.Euler(_rootInitialX, smoothY, _rootInitialZ);
        }
    }
}
