using UnityEngine;

public class CleanAltar : MonoBehaviour
{
    public QuestManager quest;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKey(KeyCode.E))
        {
            quest.FinishAltar();
            Debug.Log("Đã lau bàn thờ");
        }
    }
}