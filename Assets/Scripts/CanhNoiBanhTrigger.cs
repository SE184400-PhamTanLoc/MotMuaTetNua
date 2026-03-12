using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CanhNoiBanhTrigger : MonoBehaviour
{
    private bool _playerInZone = false;
    private bool _hasNotifiedThisEntry = false;
    private GameObject _interactionUI;
    private bool _isInteracting = false;

    public void Setup(GameObject ui)
    {
        _interactionUI = ui;
    }

    private void Update()
    {
        if (_playerInZone && !_isInteracting)
        {
            if (GameManager.Instance != null && !_hasNotifiedThisEntry)
            {
                GameManager.Instance.HienThongBao("Nhấn <color=yellow><b>E</b></color> để Canh nồi bánh cùng Ông và Bố");
                _hasNotifiedThisEntry = true;
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                _isInteracting = true;
                if (_interactionUI != null) _interactionUI.SetActive(false);
                
                if (CanhNoiBanhMinigame.Instance != null)
                {
                    CanhNoiBanhMinigame.Instance.StartGame();
                }
            }
        }
        else if (!_playerInZone && _interactionUI != null && _interactionUI.activeSelf)
        {
            _interactionUI.SetActive(false);
        }

        // Nếu đang trong game, cho phép nhấn E để bốc củi (thêm củi)
        if (_isInteracting && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (CanhNoiBanhMinigame.Instance != null)
            {
                CanhNoiBanhMinigame.Instance.AddFirewood();
            }
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
            _hasNotifiedThisEntry = false;
            if (_interactionUI != null) _interactionUI.SetActive(false);
        }
    }
}
