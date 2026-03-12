using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public float openAngle = 90f;
    public float speed = 2f;

    private bool isOpen = false;
    private Quaternion startRotation;
    private Quaternion openRotation;

    void Start()
    {
        startRotation = transform.rotation;
        openRotation = Quaternion.Euler(0, openAngle, 0) * startRotation;
    }

    void Update()
    {
        if (isOpen)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, Time.deltaTime * speed);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        /* Đã vô hiệu hoá tự động mở để dùng tương tác bằng phím E
        if (other.CompareTag("Player"))
        {
            isOpen = true;
        }
        */
    }
}