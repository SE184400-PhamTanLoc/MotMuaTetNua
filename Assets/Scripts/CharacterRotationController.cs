using UnityEngine;

/// <summary>
/// Chuyên xử lý xoay cho các NPC là nhân vật (Mẹ, Ông Nội, Bố)
/// Đảm bảo luôn đứng thẳng và nhìn về phía Player mà không bị lật.
/// </summary>
public class CharacterRotationController : MonoBehaviour
{
    [Header("=== CÀI ĐẶT XOAY ===")]
    public float tocDoQuay = 5f;
    public bool dangXoay = false;

    private Transform _playerTransform;
    private Quaternion _initialLocalRotation;
    private bool _hasCached = false;

    private void Start()
    {
        // Cache rotation ban đầu
        _initialLocalRotation = transform.localRotation;
        _hasCached = true;

        // Tìm player
        var player = FindObjectOfType<StarterAssets.FirstPersonController>();
        if (player != null) _playerTransform = player.transform;
        else
        {
            var oldPlayer = FindObjectOfType<PlayerMovement>();
            if (oldPlayer != null) _playerTransform = oldPlayer.transform;
        }
    }

    private void LateUpdate()
    {
        if (!_hasCached) return;

        if (dangXoay && _playerTransform != null)
        {
            QuayVePhiaPlayer();
        }
        else
        {
            // Trả về rotation ban đầu nhưng giữ nguyên trục Y hiện tại nếu muốn
            // Ở đây ta đơn giản là giữ nguyên localRotation để tránh giật
        }
    }

    private void QuayVePhiaPlayer()
    {
        Vector3 direction = _playerTransform.position - transform.position;
        direction.y = 0; // Luôn đứng thẳng

        if (direction.sqrMagnitude > 0.001f)
        {
            // 1. Tính rotation mục tiêu trong không gian WORLD
            Quaternion targetWorldRotation = Quaternion.LookRotation(direction, Vector3.up);

            // 2. Chuyển đổi sang không gian LOCAL của parent (CỰC KỲ QUAN TRỌNG ĐỂ CHỐNG LẬT)
            if (transform.parent != null)
            {
                // Công thức: LocalRotation = Inverse(ParentWorldRotation) * TargetWorldRotation
                Quaternion targetLocalRotation = Quaternion.Inverse(transform.parent.rotation) * targetWorldRotation;
                
                // Khóa trục X và Z của local rotation theo rotation ban đầu
                // Chỉ cho phép Y thay đổi
                Vector3 targetLocalEuler = targetLocalRotation.eulerAngles;
                Vector3 currentLocalEuler = _initialLocalRotation.eulerAngles;
                
                Quaternion finalLocalTarget = Quaternion.Euler(currentLocalEuler.x, targetLocalEuler.y, currentLocalEuler.z);
                
                transform.localRotation = Quaternion.Slerp(transform.localRotation, finalLocalTarget, tocDoQuay * Time.deltaTime);
            }
            else
            {
                // Không có parent, dùng world rotation đơn giản
                Vector3 targetEuler = targetWorldRotation.eulerAngles;
                Vector3 currentEuler = _initialLocalRotation.eulerAngles;
                Quaternion finalTarget = Quaternion.Euler(currentEuler.x, targetEuler.y, currentEuler.z);
                
                transform.rotation = Quaternion.Slerp(transform.rotation, finalTarget, tocDoQuay * Time.deltaTime);
            }
        }
    }

    public void SetRotationActive(bool active)
    {
        dangXoay = active;
    }
}
