using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PT_PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 5;
    public float gravity = -9.18f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.leftShiftKey.isPressed && isGrounded)
        {
            speed = 10;
        }
        else
        {
            speed = 5;
        }

        float x = 0;
        float z = 0;

        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x = -1;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x = 1;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) z = 1;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) z = -1;

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (kb.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}