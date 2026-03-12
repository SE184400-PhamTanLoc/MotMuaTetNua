using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    [Range(1f, 60f)] public float mouseSmoothing = 24f;

    float xRotation = 0f;
    private Vector2 smoothedMouseDelta;

    /// <summary>
    /// Đồng bộ pitch nội bộ theo góc camera hiện tại để tránh snap khi bật lại control.
    /// </summary>
    public void SyncCurrentPitchFromTransform()
    {
        xRotation = NormalizePitch(transform.localEulerAngles.x);
        smoothedMouseDelta = Vector2.zero;
    }

    private Vector2 ReadSmoothedMouseDelta()
    {
        Vector2 rawDelta = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );

        float dt = Time.unscaledDeltaTime;
        Vector2 targetDelta = rawDelta * mouseSensitivity * dt;
        float blend = 1f - Mathf.Exp(-mouseSmoothing * dt);
        smoothedMouseDelta = Vector2.Lerp(smoothedMouseDelta, targetDelta, blend);
        return smoothedMouseDelta;
    }

    void LateUpdate()
    {
        // Chỉ xử lý camera trong State1_FreeOnlyChair (xoay tự do)
        // Các state khác sẽ do CameraStateController xử lý
        if (GameFlow.Instance != null && 
            !GameFlow.Instance.IsState(GameState.State1_FreeOnlyChair))
        {
            smoothedMouseDelta = Vector2.zero;
            return;
        }

        // Không xoay camera khi đang hiện chuột (hội thoại, nhiệm vụ, minigame...)
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            smoothedMouseDelta = Vector2.zero;
            return;
        }

        Vector2 mouseDelta = ReadSmoothedMouseDelta();
        float mouseX = mouseDelta.x;
        float mouseY = mouseDelta.y;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private static float NormalizePitch(float x)
    {
        if (x > 180f) return x - 360f;
        return x;
    }
}
