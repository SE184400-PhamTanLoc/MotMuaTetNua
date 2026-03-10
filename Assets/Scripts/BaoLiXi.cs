using UnityEngine;

/// <summary>
/// BaoLiXi - Bao lì xì rải rác ở chợ Tết
/// Player đi qua sẽ nhặt được tiền
/// Gắn vào GameObject có Collider (IsTrigger = true)
/// </summary>
public class BaoLiXi : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    [Tooltip("Số tiền trong bao lì xì (nghìn đồng)")]
    public int soTien = 50; // 50.000đ

    [Tooltip("Tốc độ xoay (độ/giây)")]
    public float tocDoXoay = 90f;

    [Tooltip("Biên độ bay lên xuống")]
    public float bienDoBay = 0.3f;

    [Tooltip("Tốc độ bay lên xuống")]
    public float tocDoBay = 2f;

    [Header("=== HIỆU ỨNG ===")]
    [Tooltip("Hiệu ứng khi nhặt")]
    public ParticleSystem hieuUngNhat;

    [Tooltip("Âm thanh khi nhặt")]
    public AudioClip tiengNhat;

    // --- Private ---
    private Vector3 _viTriBanDau;
    private bool _daNhat = false;

    private void Start()
    {
        _viTriBanDau = transform.position;
    }

    private void Update()
    {
        if (_daNhat) return;

        // Hiệu ứng xoay
        transform.Rotate(Vector3.up, tocDoXoay * Time.deltaTime);

        // Hiệu ứng bay lên xuống
        float yOffset = Mathf.Sin(Time.time * tocDoBay) * bienDoBay;
        transform.position = _viTriBanDau + Vector3.up * yOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_daNhat) return;

        // Kiểm tra có phải player không
        if (other.GetComponent<StarterAssets.FirstPersonController>() != null ||
            other.GetComponent<PlayerMovement>() != null ||
            other.CompareTag("Player"))
        {
            NhatLiXi();
        }
    }

    private void NhatLiXi()
    {
        _daNhat = true;

        // Thêm tiền
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ThemTien(soTien);
        }

        // Hiệu ứng
        if (hieuUngNhat != null)
        {
            hieuUngNhat.transform.parent = null; // Tách ra để không bị xóa theo
            hieuUngNhat.Play();
            Destroy(hieuUngNhat.gameObject, 3f);
        }

        if (tiengNhat != null)
        {
            AudioSource.PlayClipAtPoint(tiengNhat, transform.position);
        }

        Debug.Log($"[BaoLiXi] 🧧 Nhặt được lì xì {GameManager.FormatTien(soTien)}!");

        // Xóa bao lì xì
        Destroy(gameObject);
    }
}
