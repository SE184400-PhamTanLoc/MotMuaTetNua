using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Quản lý trạng thái nhiệm vụ trong cảnh RoomVillage
/// </summary>
public class RoomVillageManager : MonoBehaviour
{
    public static RoomVillageManager Instance { get; private set; }

    [Header("=== TRẠNG THÁI NHIỆM VỤ ===")]
    public bool missionActive = false;
    public bool hasCloth = false;
    public bool altarCleaned = false;
    public bool incenseLit = false;

    [Header("=== EVENTS ===")]
    public UnityEvent OnMissionStarted;
    public UnityEvent OnClothPickedUp;
    public UnityEvent OnAltarCleaned;
    public UnityEvent OnIncenseLit;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // Bỏ comment nếu muốn giữ lại qua các cảnh

        // Khởi tạo Events
        if (OnMissionStarted == null) OnMissionStarted = new UnityEvent();
        if (OnClothPickedUp == null) OnClothPickedUp = new UnityEvent();
        if (OnAltarCleaned == null) OnAltarCleaned = new UnityEvent();
        if (OnIncenseLit == null) OnIncenseLit = new UnityEvent();
    }

    public void StartMission()
    {
        if (missionActive) return;
        missionActive = true;
        OnMissionStarted?.Invoke();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnThongBao?.Invoke("[ NHIỆM VỤ ] Vào nhà lau bàn thờ giúp mẹ.");
            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
        }
    }

    public void PickUpCloth()
    {
        if (!missionActive || hasCloth) return;
        hasCloth = true;
        OnClothPickedUp?.Invoke();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnThongBao?.Invoke("Đã lấy khăn lau.");
            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
        }
    }

    public void CleanAltar()
    {
        if (!missionActive || !hasCloth || altarCleaned) return;
        altarCleaned = true;
        OnAltarCleaned?.Invoke();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnThongBao?.Invoke("Đã lau sạch bàn thờ. Hãy thắp nhang.");
            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
        }
    }

    public void LightIncense()
    {
        if (!altarCleaned || incenseLit) return;
        incenseLit = true;
        OnIncenseLit?.Invoke();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnThongBao?.Invoke("Đã thắp nhang.");
            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
        }
        
        // Gọi Flashback ở đây hoặc để AltarInteract gọi
    }
}
