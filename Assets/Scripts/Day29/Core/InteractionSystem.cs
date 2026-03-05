using UnityEngine;

namespace Day29.Core
{
    public interface IInteractable
    {
        string GetInteractText();
        void Interact();
    }

    public class InteractionSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private LayerMask interactLayer;
        [SerializeField] private Transform playerCamera;

        private void Update()
        {
            CheckInteraction();
        }

        private void CheckInteraction()
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    // UI can be updated here with GetInteractText()
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        interactable.Interact();
                    }
                }
            }
        }
    }
}
