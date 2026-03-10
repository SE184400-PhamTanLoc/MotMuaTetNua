using UnityEngine;

/// <summary>
/// QuestAutoSetup - Tự động tìm xạp gạo và xạp thịt để gắn script tương tác
/// Giúp người chơi không cần kéo thả tay
/// </summary>
public class QuestAutoSetup : MonoBehaviour
{
    void Start()
    {
        Invoke("SetupStalls", 0.5f); // Đợi các object load xong
    }

    void SetupStalls()
    {
        // Tìm xạp gạo
        GameObject xapGao = GameObject.Find("xap_gao");
        if (xapGao != null)
        {
            if (xapGao.GetComponent<XapGao>() == null)
            {
                xapGao.AddComponent<XapGao>();
                Debug.Log("[QuestAutoSetup] Đã gắn XapGao vào " + xapGao.name);
            }
        }
        else
        {
            Debug.LogWarning("[QuestAutoSetup] Không tìm thấy object 'xap_gao'");
        }

        // Tìm xạp thịt
        GameObject xapThit = GameObject.Find("xap_thit");
        if (xapThit != null)
        {
            if (xapThit.GetComponent<XapThit>() == null)
            {
                xapThit.AddComponent<XapThit>();
                Debug.Log("[QuestAutoSetup] Đã gắn XapThit vào " + xapThit.name);
            }
        }
        else
        {
            Debug.LogWarning("[QuestAutoSetup] Không tìm thấy object 'xap_thit'");
        }

        // Tìm xạp rau củ (Trái cây)
        GameObject xapTraiCay = GameObject.Find("xap_rau_cu");
        if (xapTraiCay != null)
        {
            if (xapTraiCay.GetComponent<XapTraiCay>() == null)
            {
                xapTraiCay.AddComponent<XapTraiCay>();
                Debug.Log("[QuestAutoSetup] Đã gắn XapTraiCay vào " + xapTraiCay.name);
            }
        }
    }
}
