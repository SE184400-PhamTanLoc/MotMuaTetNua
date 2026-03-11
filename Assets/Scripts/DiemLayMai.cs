using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// DiemLayMai - Đặt tại vị trí các chậu mai
/// Khi player đã trả tiền (daMuaMai) nhưng chưa lấy (daLayMai) thì có thể tương tác lấy mai
/// </summary>
public class DiemLayMai : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    public GameObject goiYText;
    public ParticleSystem hieuUngLayMai;
    public AudioClip tiengLayMai;

    private AudioSource _audioSource;
    private bool _playerTrongVung = false;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (goiYText != null) goiYText.SetActive(false);
    }

    private void Update()
    {
        // Phổ biến hơn: Nếu player ở quá gần (phòng trường hợp trigger không bắt được lúc mới load scene)
        if (!_playerTrongVung)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, 3f);
            foreach (var col in cols)
            {
                if (col.CompareTag("Player") || col.GetComponent<CharacterController>() != null)
                {
                    _playerTrongVung = true;
                    break;
                }
            }
        }

        if (_playerTrongVung && GameManager.Instance != null && GameManager.Instance.daMuaMai && !GameManager.Instance.daLayMai)
        {
            // Hiển thị UI gợi ý
            if (goiYText == null)
            {
                var player = FindObjectOfType<PlayerInteraction>();
                if (player != null) goiYText = player.goiYTuongTacUI;
            }

            if (goiYText != null && !goiYText.activeSelf)
            {
                goiYText.SetActive(true);
                var tmp = goiYText.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmp != null)
                    tmp.text = "Nhấn <color=#FFD700><b>F</b></color> để cầm mai mang về 🌼";
            }

            // Nhấn F để lấy
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                ThucHienLayMai();
            }
        }
        else if (goiYText != null && goiYText.activeSelf)
        {
            // Ẩn UI nếu không đủ điều kiện
            goiYText.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<StarterAssets.FirstPersonController>() != null ||
            other.GetComponent<PlayerMovement>() != null ||
            other.GetComponent<CharacterController>() != null ||
            other.CompareTag("Player"))
        {
            _playerTrongVung = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _playerTrongVung = false;
        if (goiYText != null) goiYText.SetActive(false);
    }

    private void ThucHienLayMai()
    {
        _playerTrongVung = false;

        GameManager.Instance.LayMai();

        if (hieuUngLayMai != null)
            hieuUngLayMai.Play();

        if (_audioSource != null && tiengLayMai != null)
            _audioSource.PlayOneShot(tiengLayMai);

        if (goiYText != null) goiYText.SetActive(false);
    }
}
