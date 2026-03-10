using UnityEngine;

public class DayManager : MonoBehaviour
{
    public int currentDay = 29;

    public void NextDay()
    {
        currentDay++;

        if (currentDay == 30)
        {
            Debug.Log("Ngày 30 Tết bắt đầu");
        }
    }
}