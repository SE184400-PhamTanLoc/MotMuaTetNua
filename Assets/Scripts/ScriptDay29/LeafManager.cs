using UnityEngine;

public class LeafManager : MonoBehaviour
{
    public int leafCount = 5;

    public void LeafRemoved()
    {
        leafCount--;

        if (leafCount <= 0)
        {
            FindObjectOfType<QuestManager>().FinishSweep();
        }
    }
}