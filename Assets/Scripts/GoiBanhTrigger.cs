using UnityEngine;
using UnityEngine.InputSystem;

public class GoiBanhTrigger : MonoBehaviour
{
    private bool _playerInZone = false;
    private GameObject _goiYUI;

    public void Setup(GameObject ui)
    {
        _goiYUI = ui;
    }

    private void Update()
    {
        if (_playerInZone && GameManager.Instance != null)
        {
            if (_goiYUI != null && !_goiYUI.activeSelf)
            {
                _goiYUI.SetActive(true);
                var tmp = _goiYUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmp != null) tmp.text = "Nhấn <color=#FFD700><b>E</b></color> để bắt đầu gói bánh Tét cùng Mẹ 🎋";
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
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.name.ToLower().Contains("player"))
        {
            _playerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.name.ToLower().Contains("player"))
        {
            _playerInZone = false;
        }
    }
}
