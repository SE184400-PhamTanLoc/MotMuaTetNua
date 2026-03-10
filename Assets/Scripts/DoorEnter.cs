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

            if (pressText != null)
                pressText.SetActive(true); // hiện chữ
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;

            if (pressText != null)
                pressText.SetActive(false); // ẩn chữ
        }
    }
}