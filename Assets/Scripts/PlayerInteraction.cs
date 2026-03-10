using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerInteraction - Gắn vào Player để phát hiện và tương tác với NPC
/// Sử dụng Input System mới
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("=== CÀI ĐẶT TƯƠNG TÁC ===")]
    public float khoangCachTuongTac = 5f;
    public LayerMask npcLayer;

    [Header("=== UI ===")]
    public GameObject goiYTuongTacUI;

    private Camera _mainCamera;
    private NPCBase _npcHienTai;

    private void Start()
    {
        _mainCamera = Camera.main;
        if (goiYTuongTacUI != null)
            goiYTuongTacUI.SetActive(false);
    }

    private void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.DangHoiThoai)
        {
            AnGoiY();
            return;
        }

        KiemTraNPC();
        XuLyTuongTac();
    }

    private void KiemTraNPC()
    {
        NPCBase[] tatCaNPC = FindObjectsOfType<NPCBase>();
        NPCBase npcGanNhat = null;
        float khoangCachGanNhat = khoangCachTuongTac;

        foreach (var npc in tatCaNPC)
        {
            if (!npc.coTheTuongTac) continue;

            float khoangCach = Vector3.Distance(transform.position, npc.transform.position);
            if (khoangCach < khoangCachGanNhat)
            {
                khoangCachGanNhat = khoangCach;
                npcGanNhat = npc;
            }
        }

        if (npcGanNhat != null)
        {
            _npcHienTai = npcGanNhat;
            HienGoiY(npcGanNhat.tenNPC);
        }
        else
        {
            _npcHienTai = null;
            AnGoiY();
        }
    }

    private void XuLyTuongTac()
    {
        if (_npcHienTai != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            _npcHienTai.BatDauTuongTac();
            AnGoiY();
        }
    }

    private void HienGoiY(string tenNPC)
    {
        if (goiYTuongTacUI != null)
        {
            goiYTuongTacUI.SetActive(true);
            var tmp = goiYTuongTacUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmp != null && _npcHienTai != null)
                tmp.text = $"Nhấn <color=#FFD700><b>E</b></color> để {_npcHienTai.hanhDongTuongTac} với {tenNPC}";
        }
    }

    private void AnGoiY()
    {
        if (goiYTuongTacUI != null)
            goiYTuongTacUI.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, khoangCachTuongTac);
    }
}
