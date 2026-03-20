using UnityEngine;
using UnityEngine.SceneManagement;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    [Range(1f, 60f)] public float mouseSmoothing = 24f;

    public float XRotation { get => xRotation; set => xRotation = value; }
    private float xRotation = 0f;
    private Vector2 smoothedMouseDelta;

    private void Start()
    {
        SyncXRotation();
    }

    private void SyncXRotation()
    {
        // Khởi tạo xRotation từ góc hiện tại để tránh bị giật camera (snap) khi bắt đầu
        Vector3 rot = transform.localRotation.eulerAngles;
        xRotation = NormalizeAngle(rot.x);
        Debug.Log($"[MouseLook] SyncXRotation: {xRotation}");
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
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
        float mouseXInput = Input.GetAxisRaw("Mouse X");
        float mouseYInput = Input.GetAxisRaw("Mouse Y");

        Vector2 rawDelta = new Vector2(mouseXInput, mouseYInput);

        float dt = Time.unscaledDeltaTime;
        // KHÔNG nhân với Time.deltaTime vì GetAxisRaw đã là delta của Frame rồi.
        // Dùng 0.5f làm hệ số scale để nhạy hơn một chút và mượt hơn.
        Vector2 targetDelta = rawDelta * (mouseSensitivity * 0.5f); 
        float blend = 1f - Mathf.Exp(-mouseSmoothing * dt);
        smoothedMouseDelta = Vector2.Lerp(smoothedMouseDelta, targetDelta, blend);
        return smoothedMouseDelta;
    }

    void LateUpdate()
    {
        // 1. Nếu có FirstPersonController đang hoạt động thì tự tắt để tránh xung đột
        var fpc = GetComponent<StarterAssets.FirstPersonController>();
        if (fpc == null) fpc = GetComponentInParent<StarterAssets.FirstPersonController>();
        if (fpc != null && fpc.enabled)
        {
            this.enabled = false;
            return;
        }

        // 2. Chỉ xử lý camera trong State1_FreeOnlyChair (xoay tự do)
        // Ngoại lệ: các scene free-roam (Village/RoomVillage/RoomScene/Day_28) không dùng flow state chặt,
        // nên cho phép xoay như FPS bình thường để tránh cảm giác "giật/khóa" camera.
        string sceneName = SceneManager.GetActiveScene().name;
        bool allowFreeLookInScene =
            sceneName == "VillageScene" || sceneName == "RoomVillage" || sceneName == "Day_28_Scene";

        if (!allowFreeLookInScene &&
            GameFlow.Instance != null &&
            !GameFlow.Instance.IsState(GameState.State1_FreeOnlyChair))
        {
            smoothedMouseDelta = Vector2.zero;
            return;
        }

        // 3. Không xoay camera khi đang hiện chuột
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            smoothedMouseDelta = Vector2.zero;
            return;
        }

        Vector2 mouseDelta = ReadSmoothedMouseDelta();
        float mouseX = mouseDelta.x * Time.unscaledDeltaTime;
        float mouseY = mouseDelta.y * Time.unscaledDeltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    public void ForceSyncRotation(float newX)
    {
        xRotation = newX;
    }

    private static float NormalizePitch(float x)
    {
        if (x > 180f) return x - 360f;
        return x;
    }
}
