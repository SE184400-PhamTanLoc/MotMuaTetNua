using UnityEngine;

public class LightIncense : MonoBehaviour
{
    public DayManager dayManager;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Thắp nhang");
            dayManager.NextDay();
        }
    }
}