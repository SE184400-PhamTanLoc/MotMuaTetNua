using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float gravity = -9.8f;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Cursor được quản lý bởi InputModeManager
    }

    void Update()
    {
        // Không di chuyển nếu đang ở Intro (đang fade) hoặc SittingAtDesk hoặc các state sau đó
        if (GameFlow.Instance != null && 
            (GameFlow.Instance.IsState(GameState.Intro) ||
             GameFlow.Instance.IsState(GameState.SittingAtDesk) ||
             GameFlow.Instance.IsState(GameState.ComputerActive) ||
             GameFlow.Instance.IsState(GameState.ComputerFinished) ||
             GameFlow.Instance.IsState(GameState.AlbumFocus) ||
             GameFlow.Instance.IsState(GameState.AlbumReading) ||
             GameFlow.Instance.IsState(GameState.AlbumInteractable)))
        {
            return;
        }

        float x = Input.GetAxis("Horizontal"); // A/D
        float z = Input.GetAxis("Vertical");   // W/S

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Gravity
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = 0f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
