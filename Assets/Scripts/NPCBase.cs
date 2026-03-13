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

    [Header("=== KHÓA TRỤC XOAY (TUỲ CHỌN) ===")]
    [Tooltip("Bật để ép trục X về một góc cố định khi NPC quay theo player.")]
    public bool forceFixedXRotation = false;
    [Tooltip("Góc X cố định khi forceFixedXRotation bật.")]
    public float fixedXRotation = 0f;
    [Tooltip("Bật để ép trục Z về một góc cố định khi NPC quay theo player.")]
    public bool forceFixedZRotation = false;
    [Tooltip("Góc Z cố định khi forceFixedZRotation bật (thường dùng 0).")]
    public float fixedZRotation = 0f;

    // --- Private ---
    protected Transform _playerTransform;
    private bool _dangTuongTac = false;

    // --- Cache: lưu rotation ban đầu ĐÚNG của root (X, Z) và children (local) ---
    // QUAN TRỌNG: Không được reset X, Z vì model có thể cần Z=90 để đứng thẳng
    private Vector3 _initialLocalEuler;           // Local rotation ban đầu ĐÚNG
    private Quaternion[] _childOriginalLocalRots; // Local rotation của children
    private Transform[] _directChildren;          // Danh sách children trực tiếp
    private bool _hasCachedRotation = false;      // Flag để đảm bảo chỉ cache 1 lần

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

        // Cache sẽ được thực hiện ở Start để đảm bảo scene đã load xong hoàn toàn
        // Tuy nhiên vẫn cần tắt Root Motion sớm

        // Bước 4: Cache local rotation của tất cả children trực tiếp AS-IS
        _directChildren = new Transform[transform.childCount];
        _childOriginalLocalRots = new Quaternion[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {_directChildren[i] = child;
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
        // Cache rotation ở Start là an toàn nhất vì lúc này prefab/scene đã ổn định
        CacheInitialRotation();

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

    private void CacheInitialRotation()
    {
        if (_hasCachedRotation) return;

        // Model có thể cần Z=90 hoặc X=-90 để đứng thẳng - ta phải tôn trọng điều đó
        // Dùng local để không bị ảnh hưởng bởi parent (vd: Group Market bị xoay)
        _initialLocalEuler = transform.localEulerAngles;
        
        Debug.Log($"[NPCBase] {gameObject.name}: Cached local rotation {_initialLocalEuler}");

        // Cache local rotation của tất cả children trực tiếp AS-IS
        _directChildren = new Transform[transform.childCount];
        _childOriginalLocalRots = new Quaternion[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {
            _directChildren[i] = child;
            _childOriginalLocalRots[i] = child.localRotation;
            i++;
        }

        _hasCachedRotation = true;
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
        if (!_hasCachedRotation) return;

        // Lấy Y hiện tại (hướng nhìn) để giữ lại
        float currentY = transform.localEulerAngles.y;
        float targetX = forceFixedXRotation ? fixedXRotation : _initialLocalEuler.x;
        float targetZ = forceFixedZRotation ? fixedZRotation : _initialLocalEuler.z;

        // Khôi phục X và Z (local)
        transform.localRotation = Quaternion.Euler(targetX, currentY, targetZ);

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
        huong.y = 0;if (huong.sqrMagnitude > 0.001f)
        {
            // Tính góc Y mục tiêu
            Quaternion targetRot = Quaternion.LookRotation(huong, Vector3.up);
            float targetY = targetRot.eulerAngles.y;
            
            // Nếu có parent, ta cần chuyển world Y sang local Y
            if (transform.parent != null)
            {
                // Cách đơn giản nhất: dùng Quaternion.RotateTowards hoặc Slerp trên localRotation
                // Nhưng để giữ logic của người dùng, ta convert targetY về local
                targetY -= transform.parent.eulerAngles.y;
            }

            float smoothY = Mathf.LerpAngle(transform.localEulerAngles.y, targetY, tocDoQuay * Time.deltaTime);
            float targetX = forceFixedXRotation ? fixedXRotation : _initialLocalEuler.x;
            float targetZ = forceFixedZRotation ? fixedZRotation : _initialLocalEuler.z;

            // Áp dụng quay local
            transform.localRotation = Quaternion.Euler(targetX, smoothY, targetZ);
        }
    }
}