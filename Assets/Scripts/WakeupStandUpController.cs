using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Flow đứng dậy cho scene wakeup:
/// - Bật phase "nhấn F để đứng dậy" sau khi thoại kết thúc.
/// - Khi nhấn F: đổi state sang tự do di chuyển và bắn event tiếp theo.
/// Không phụ thuộc sửa code cũ.
/// </summary>
public class WakeupStandUpController : MonoBehaviour
{
    [Header("Input")]
    public KeyCode standUpKey = KeyCode.F;
    public bool requireStateSittingAtDesk = true;
    [Tooltip("Khi bật phase đứng dậy, ép state về SittingAtDesk để đảm bảo phím F hoạt động.")]
    public bool forceSittingStateWhenEnablePhase = true;
    public bool debugLog;

    [Header("Position (optional)")]
    [Tooltip("Player root. Để trống sẽ tự tìm.")]
    public Transform playerRoot;
    [Tooltip("Điểm đứng dậy. Để trống = giữ nguyên vị trí hiện tại.")]
    public Transform standUpPoint;
    public bool snapToStandUpPoint = false;

    [Header("Hint (optional)")]
    public GameObject standUpHintRoot;

    [Header("State")]
    [Tooltip("State sau khi đứng dậy.")]
    public GameState stateAfterStandUp = GameState.State1_FreeOnlyChair;

    [Header("Events")]
    public UnityEvent onStandUpEnabled;
    public UnityEvent onStandUpPerformed;

    private bool standUpPhaseEnabled;
    private bool stoodUp;
    [SerializeField] private bool debugStandUpPhaseEnabled;
    [SerializeField] private bool debugStoodUp;
    [SerializeField] private GameState debugCurrentState;
    private float nextDebugLogAt;

    private void Start()
    {
        if (playerRoot == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerRoot = player.transform;
            if (playerRoot == null)
            {
                StarterAssets.FirstPersonController fps = FindFirstObjectByType<StarterAssets.FirstPersonController>();
                if (fps != null) playerRoot = fps.transform;
            }
        }

        SetHintVisible(false);
    }

    private void Update()
    {
        debugStandUpPhaseEnabled = standUpPhaseEnabled;
        debugStoodUp = stoodUp;
        if (GameFlow.Instance != null)
            debugCurrentState = GameFlow.Instance.currentState;

        if (!standUpPhaseEnabled || stoodUp) return;
        if (!Input.GetKeyDown(standUpKey))
            return;

        if (requireStateSittingAtDesk && GameFlow.Instance != null && !GameFlow.Instance.IsState(GameState.SittingAtDesk))
        {
            TryDebugLog($"[WakeupStandUpController] Bị chặn F vì state hiện tại = {GameFlow.Instance.currentState}, cần SittingAtDesk.");
            return;
        }

        PerformStandUp();
    }

    /// <summary>
    /// Gọi từ UnityEvent (ví dụ sau khi thoại "Về nhà thôi." đóng).
    /// </summary>
    public void EnableStandUpPhase()
    {
        if (stoodUp) return;

        if (forceSittingStateWhenEnablePhase && GameFlow.Instance != null)
            GameFlow.Instance.ChangeState(GameState.SittingAtDesk);

        standUpPhaseEnabled = true;
        SetHintVisible(true);
        TryDebugLog("[WakeupStandUpController] EnableStandUpPhase() -> đã bật phase nhấn F.");
        onStandUpEnabled?.Invoke();
    }

    public void PerformStandUp()
    {
        if (stoodUp) return;
        stoodUp = true;
        standUpPhaseEnabled = false;

        if (snapToStandUpPoint && playerRoot != null && standUpPoint != null)
        {
            playerRoot.position = standUpPoint.position;
            playerRoot.rotation = standUpPoint.rotation;
        }

        if (GameFlow.Instance != null)
            GameFlow.Instance.ChangeState(stateAfterStandUp);

        SetHintVisible(false);
        TryDebugLog($"[WakeupStandUpController] PerformStandUp() -> đổi state sang {stateAfterStandUp}.");
        onStandUpPerformed?.Invoke();
    }

    private void SetHintVisible(bool visible)
    {
        if (standUpHintRoot != null)
            standUpHintRoot.SetActive(visible);
    }

    private void TryDebugLog(string message)
    {
        if (!debugLog) return;
        if (Time.unscaledTime < nextDebugLogAt) return;
        nextDebugLogAt = Time.unscaledTime + 0.1f;
        Debug.Log(message);
    }
}
