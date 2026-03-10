using UnityEngine;

/// <summary>
/// NPCBase - Lớp cơ sở cho tất cả NPC có thể tương tác
/// Các NPC cụ thể sẽ kế thừa lớp này
/// </summary>
public abstract class NPCBase : MonoBehaviour
{
    [Header("=== THÔNG TIN NPC ===")]
    public string tenNPC = "NPC";
    public bool coTheTuongTac = true;

    [Header("=== QUAY VỀ PHÍA PLAYER ===")]
    public bool quayVePhiaPlayer = true;
    public float tocDoQuay = 5f;

    // --- Private ---
    protected Transform _playerTransform;
    private bool _dangTuongTac = false;

    protected virtual void Start()
    {
        // Tìm player
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
        {
            QuayVePhiaPlayer();
        }
    }

    /// <summary>
    /// Gọi khi player tương tác (nhấn E)
    /// </summary>
    public void BatDauTuongTac()
    {
        if (!coTheTuongTac) return;

        _dangTuongTac = true;
        OnTuongTac();
    }

    /// <summary>
    /// Override trong lớp con để xử lý tương tác
    /// </summary>
    protected abstract void OnTuongTac();

    /// <summary>
    /// Gọi khi kết thúc tương tác
    /// </summary>
    protected void KetThucTuongTac()
    {
        _dangTuongTac = false;
    }

    /// <summary>
    /// Quay NPC về phía player
    /// </summary>
    private void QuayVePhiaPlayer()
    {
        Vector3 huong = _playerTransform.position - transform.position;
        huong.y = 0; // Chỉ quay ngang
        if (huong != Vector3.zero)
        {
            Quaternion gocDoMoi = Quaternion.LookRotation(huong);
            transform.rotation = Quaternion.Slerp(transform.rotation, gocDoMoi, tocDoQuay * Time.deltaTime);
        }
    }
}
