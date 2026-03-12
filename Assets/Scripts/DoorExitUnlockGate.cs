using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Khóa/mở tương tác cửa End Game theo tiến trình đứng dậy.
/// Gắn lên cùng object với DoorExit hoặc NPCBase.
/// </summary>
public class DoorExitUnlockGate : MonoBehaviour
{
    [Header("References")]
    public NPCBase doorNpc;
    public Outline doorOutline;
    public Camera playerCamera;

    [Header("Settings")]
    [Tooltip("Khóa cửa ngay từ đầu.")]
    public bool lockOnStart = true;
    [Tooltip("Chỉ bật outline khi raycast center camera trúng cửa trong khoảng này.")]
    public float interactRayDistance = 3f;
    [Tooltip("Layer mask dùng cho raycast kiểm tra nhìn vào cửa.")]
    public LayerMask raycastMask = ~0;
    [Tooltip("Tự tìm Outline ở object hiện tại hoặc con.")]
    public bool autoFindOutline = true;
    [Tooltip("Bật log debug khóa/mở cửa.")]
    public bool debugLog;
    [SerializeField] private bool debugDoorUnlocked;
    [SerializeField] private bool debugRaycastLookedAtDoor;

    [Header("Hint UI")]
    [Tooltip("UI hint hiện khi nhìn vào cửa và cửa đã mở.")]
    public GameObject doorHintRoot;
    [Tooltip("Text hiển thị nội dung hint (không bắt buộc).")]
    public TMP_Text doorHintText;
    [Tooltip("Tự tìm TMP_Text bên trong doorHintRoot nếu chưa gán.")]
    public bool autoFindHintText = true;
    [TextArea]
    public string hintMessage = "Nhấn E để về nhà";

    private void Awake()
    {
        if (doorNpc == null)
            doorNpc = GetComponent<NPCBase>();
        if (playerCamera == null)
            playerCamera = Camera.main;
        if (autoFindOutline && doorOutline == null)
            doorOutline = GetComponent<Outline>() ?? GetComponentInChildren<Outline>(true);
        if (autoFindHintText && doorHintText == null && doorHintRoot != null)
            doorHintText = doorHintRoot.GetComponentInChildren<TMP_Text>(true);
    }

    private void Start()
    {
        if (lockOnStart)
            LockDoor();
        SetOutline(false);
        SetHintVisible(false);
    }

    private void Update()
    {
        debugDoorUnlocked = doorNpc != null && doorNpc.coTheTuongTac;
        UpdateOutlineByRaycast();
    }

    /// <summary>
    /// Nối từ event onStandUpPerformed để mở cửa.
    /// </summary>
    public void UnlockDoor()
    {
        if (doorNpc != null)
            doorNpc.coTheTuongTac = true;

        UpdateOutlineByRaycast();

        if (debugLog)
            Debug.Log("[DoorExitUnlockGate] Door unlocked.");
    }

    public void LockDoor()
    {
        if (doorNpc != null)
            doorNpc.coTheTuongTac = false;
        SetOutline(false);
        SetHintVisible(false);

        if (debugLog)
            Debug.Log("[DoorExitUnlockGate] Door locked.");
    }

    private void UpdateOutlineByRaycast()
    {
        if (doorNpc == null)
        {
            SetOutline(false);
            SetHintVisible(false);
            return;
        }

        if (!doorNpc.coTheTuongTac)
        {
            SetOutline(false);
            SetHintVisible(false);
            return;
        }

        if (playerCamera == null)
            playerCamera = Camera.main;
        if (playerCamera == null)
        {
            SetOutline(false);
            SetHintVisible(false);
            return;
        }

        bool lookedAt = IsDoorLookedAtByCenterRay();
        debugRaycastLookedAtDoor = lookedAt;
        SetOutline(lookedAt);
        SetHintVisible(lookedAt);
    }

    private bool IsDoorLookedAtByCenterRay()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Max(0.1f, interactRayDistance), raycastMask, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0) return false;

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null) continue;

            NPCBase hitNpc = hitCollider.GetComponentInParent<NPCBase>();
            if (hitNpc == null) continue;

            // Chỉ tính trúng khi object NPC đầu tiên trên tia là chính cửa này.
            return hitNpc == doorNpc;
        }

        return false;
    }

    private void SetOutline(bool enabled)
    {
        if (doorOutline == null) return;
        if (doorOutline.enabled == enabled) return;
        doorOutline.enabled = enabled;
    }

    private void SetHintVisible(bool visible)
    {
        if (doorHintText != null && !string.IsNullOrEmpty(hintMessage))
            doorHintText.text = hintMessage;

        if (doorHintRoot == null) return;
        if (doorHintRoot.activeSelf == visible) return;
        doorHintRoot.SetActive(visible);
    }
}
