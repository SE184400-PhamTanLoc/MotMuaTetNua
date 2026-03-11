using UnityEngine;

public class PickBroom : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Nhặt chổi");
            gameObject.SetActive(false);
        }
    }
}