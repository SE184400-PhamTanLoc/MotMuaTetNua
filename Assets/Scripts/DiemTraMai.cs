using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// DiemTraMai - Đặt ở vị trí nhà mẹ/bàn thờ
/// Khi player đã mua mai và đến đây → hoàn thành nhiệm vụ
/// </summary>
public class DiemTraMai : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    public GameObject goiYText;
    public ParticleSystem hieuUngHoanThanh;
    public AudioClip tiengHoanThanh;
    public GameObject muiTenChiDuong;

    private bool _daHoanThanh = false;
    private AudioSource _audioSource;
    private bool _playerTrongVung = false;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (goiYText != null) goiYText.SetActive(false);
        if (muiTenChiDuong != null) muiTenChiDuong.SetActive(false);
    }

    private void Update()
    {
        if (!_daHoanThanh && muiTenChiDuong != null && GameManager.Instance != null)
        {
            muiTenChiDuong.SetActive(GameManager.Instance.daMuaMai);
        }

        if (_playerTrongVung && !_daHoanThanh && GameManager.Instance != null && GameManager.Instance.daMuaMai && GameManager.Instance.daLayMai)
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                HoanThanhNhiemVu();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_daHoanThanh) return;

        if (other.GetComponent<StarterAssets.FirstPersonController>() != null ||
            other.GetComponent<PlayerMovement>() != null ||
            other.GetComponent<CharacterController>() != null ||
            other.CompareTag("Player"))
        {
            _playerTrongVung = true;

            if (GameManager.Instance != null && GameManager.Instance.daMuaMai && GameManager.Instance.daLayMai)
            {
                if (goiYText == null)
                {
                    var player = FindObjectOfType<PlayerInteraction>();
                    if (player != null) goiYText = player.goiYTuongTacUI;
                }

                if (goiYText != null)
                {
                    goiYText.SetActive(true);
                    var tmp = goiYText.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (tmp != null)
                        tmp.text = "Nhấn <color=#FFD700><b>F</b></color> để đặt mai cho mẹ 🌼";
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _playerTrongVung = false;
        if (goiYText != null) goiYText.SetActive(false);
    }

    private void HoanThanhNhiemVu()
    {
        _daHoanThanh = true;
        _playerTrongVung = false;

        GameManager.Instance.MangMaiVeMeHoanThanh();

        if (hieuUngHoanThanh != null)
            hieuUngHoanThanh.Play();

        if (_audioSource != null && tiengHoanThanh != null)
            _audioSource.PlayOneShot(tiengHoanThanh);

        if (goiYText != null) goiYText.SetActive(false);
        if (muiTenChiDuong != null) muiTenChiDuong.SetActive(false);
    }
}
