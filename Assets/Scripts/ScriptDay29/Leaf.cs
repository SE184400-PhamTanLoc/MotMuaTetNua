using UnityEngine;

public class Leaf : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKey(KeyCode.E))
        {
            Destroy(gameObject);
        }
    }
}