using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GoiBanhTrigger : MonoBehaviour
{
    private bool _playerInZone = false;
    private bool _hasNotifiedThisEntry = false;
    private GameObject _goiYUI;

    public void Setup(GameObject ui)
    {
        _goiYUI = ui;
    }

    private void Update()
    {
        if (_playerInZone && GameManager.Instance != null)
        {
            // CHỈ cho phép gói bánh NẾU đã quét sân xong
            if (!GameManager.Instance.yardSwept)
            {
                if (_goiYUI != null) _goiYUI.SetActive(false);
                return;
            }

            if (GameManager.Instance != null && !_hasNotifiedThisEntry)
            {
                GameManager.Instance.HienThongBao("Nhấn <color=yellow><b>E</b></color> để bắt đầu gói bánh Tét cùng Mẹ");
                _hasNotifiedThisEntry = true;
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("[GoiBanhTrigger] ⌨️ E key pressed!");
                if (BanhTetMinigame.Instance != null)
                {
                    Debug.Log("[GoiBanhTrigger] ✅ Calling StartGame()");
                    BanhTetMinigame.Instance.StartGame();
                    if (_goiYUI != null) _goiYUI.SetActive(false);
                }
                else
                {
                    Debug.LogError("[GoiBanhTrigger] ❌ BanhTetMinigame.Instance is NULL even after standing in zone!");
                }
            }
        }
        else if (_goiYUI != null && _goiYUI.activeSelf)
        {
            _goiYUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[GoiBanhTrigger] 🚪 Object entered: {other.name} (Tag: {other.tag})");
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.name.ToLower().Contains("player"))
        {
            _playerInZone = true;
            Debug.Log("[GoiBanhTrigger] ✅ Player detected in zone!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.name.ToLower().Contains("player"))
        {
            _playerInZone = false;
            _hasNotifiedThisEntry = false; 
            if (_goiYUI != null) _goiYUI.SetActive(false);
        }
    }
}
