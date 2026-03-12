using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorEnter : MonoBehaviour
{
    public string sceneName;
    public GameObject pressText;   // UI text: "Bấm phím O..."

    private bool playerNear = false;

    void Start()
    {
        if (pressText != null)
            pressText.SetActive(false); // ban đầu ẩn
    }

    void Update()
    {
        if (pressText != null && pressText.activeSelf) pressText.SetActive(false); // Đảm bảo luôn ẩn

        if (playerNear && Input.GetKeyDown(KeyCode.O))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;

            // if (pressText != null)
            //    pressText.SetActive(true); // hiện chữ (Đã vô hiệu hoá để tránh trùng lặp)
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;

            // if (pressText != null)
            //    pressText.SetActive(false); // ẩn chữ (Đã vô hiệu hoá để tránh trùng lặp)
        }
    }
}