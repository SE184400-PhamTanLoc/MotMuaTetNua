using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public bool sweepYardDone = false;
    public bool cleanAltarDone = false;

    public void FinishSweep()
    {
        sweepYardDone = true;
        Debug.Log("Đã quét sân");
    }

    public void FinishAltar()
    {
        cleanAltarDone = true;
        Debug.Log("Đã lau bàn thờ");
    }
}